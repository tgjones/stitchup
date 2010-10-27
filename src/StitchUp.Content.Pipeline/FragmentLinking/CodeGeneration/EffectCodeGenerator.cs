using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using StitchUp.Content.Pipeline.Graphics;

namespace StitchUp.Content.Pipeline.FragmentLinking.CodeGeneration
{
	public class EffectCodeGenerator
	{
		private readonly FragmentContentCollection _fragments;
		private readonly StringBuilder _output;

		public EffectCodeGenerator(IEnumerable<FragmentContent> fragments)
		{
			_fragments = new FragmentContentCollection(fragments);
			_output = new StringBuilder();
		}

		public string GenerateCode()
		{
			WriteAllParams();
			WriteAllSamplers();
			WriteAllVertexInputStructures();
			WriteAllPixelInputStructures();
			WriteAllVertexOutputStructures();
			WritePixelOutputStructure();
			WriteAllVertexShaders();
			WriteAllPixelShaders();
			WriteTechnique();

			return _output.ToString();
		}

		private void WritePixelOutputStructure()
		{
			_output.AppendLineFormat("// -------- pixel output type --------");
			_output.AppendLine("struct PIXELOUTPUT");
			_output.AppendLine("{");
			_output.AppendLine("\tfloat4 color : COLOR0;");
			_output.AppendLine("};");
			_output.AppendLine();
		}

		private void WriteAllVertexShaders()
		{
			_fragments.ForEach(WriteVertexShader);

			_output.AppendLine("// -------- vertex shader entrypoint --------");
			_output.AppendLine("VERTEXOUTPUT vs(const VERTEXINPUT i)");
			_output.AppendLine("{");
			_output.AppendLine("\tgVertexInput = i;");
			_output.AppendLine();
			_output.AppendLine("\tVERTEXOUTPUT output = (VERTEXOUTPUT) 0;");
			_output.AppendLine();
			_fragments.ForEach((fragment, mangledFragmentName) => _output.AppendLineFormat("\t{0}_vs(gVertexInput.{0}, output);", mangledFragmentName));
			_output.AppendLine();
			_output.AppendLine("\treturn output;");
			_output.AppendLine("}");
			_output.AppendLine();
		}

		private void WriteVertexShader(FragmentContent fragment, string mangledFragmentName)
		{
			_output.AppendLineFormat("// -------- vertex shader {0} --------", mangledFragmentName);

			FragmentCodeContent shader = fragment.CodeBlocks.FirstOrDefault(s => s.ShaderType == FragmentCodeShaderType.VertexShader);

			if (shader != null)
			{
				string mangledCode = shader.Code;

				// Replace interpolators and sampler names which are used in the code with the mangled names.
				fragment.InterfaceParams.ToList().ForEach(i =>
					mangledCode = Regex.Replace(mangledCode, @"([^\w\.])(" + i + @")(\W)", "$1" + mangledFragmentName + "_$2$3"));
				fragment.InterfaceInterpolators.ToList().ForEach(i =>
					mangledCode = Regex.Replace(mangledCode, @"([^\w\.])(" + i + @")(\W)", "$1" + mangledFragmentName + "_$2$3"));
				fragment.InterfaceTextures.ForEach(t =>
					mangledCode = Regex.Replace(mangledCode, @"(\W)(" + t + @")(\W)", "$1" + mangledFragmentName + "_$2$3"));

				mangledCode = mangledCode.Replace("void main(", string.Format("void {0}_vs(", mangledFragmentName));
				mangledCode = mangledCode.Replace("INPUT", string.Format("{0}_VERTEXINPUT", mangledFragmentName));
				mangledCode = mangledCode.Replace("OUTPUT", "VERTEXOUTPUT");
				_output.Append(mangledCode);
			}
			else
			{
				// Need to auto-generate vertex shader. Simply pass through all vertex inputs.
				_output.AppendLineFormat("void {0}_vs({0}_VERTEXINPUT input, inout VERTEXOUTPUT output)", mangledFragmentName);
				_output.AppendLine("{");
				fragment.InterfaceVertex.ForEach(v => _output.AppendLineFormat("\toutput.{0}.{1} = input.{1};", mangledFragmentName, v));
				_output.AppendLine("}");
			}

			_output.AppendLine();
			_output.AppendLine();
		}

		private void WriteAllPixelShaders()
		{
			_fragments.ForEach(WritePixelShader);

			_output.AppendLine("// -------- pixel shader entrypoint --------");
			_output.AppendLine("PIXELOUTPUT ps(const PIXELINPUT i)");
			_output.AppendLine("{");
			_output.AppendLine("\tgPixelInput = i;");
			_output.AppendLine();
			_output.AppendLine("\tPIXELOUTPUT output = (PIXELOUTPUT) 0;");
			_output.AppendLine();
			_fragments.ForEach((fragment, mangledFragmentName) =>
			{
				FragmentCodeContent shader = fragment.CodeBlocks.FirstOrDefault(s => s.ShaderType == FragmentCodeShaderType.PixelShader);
				if (shader != null)
					_output.AppendLineFormat("\t{0}_ps(gPixelInput.{0}, output);", mangledFragmentName);
			});
			_output.AppendLine();
			_output.AppendLine("\treturn output;");
			_output.AppendLine("}");
			_output.AppendLine();
		}

		private void WritePixelShader(FragmentContent fragment, string mangledFragmentName)
		{
			FragmentCodeContent shader = fragment.CodeBlocks.FirstOrDefault(s => s.ShaderType == FragmentCodeShaderType.PixelShader);

			if (shader != null)
			{
				_output.AppendLineFormat("// -------- pixel shader {0} --------", mangledFragmentName);

				string mangledCode = shader.Code;

				// Replace interpolators and sampler names which are used in the code with the mangled names.
				fragment.InterfaceParams.ToList().ForEach(i =>
					mangledCode = Regex.Replace(mangledCode, @"([^\w\.])(" + i + @")(\W)", "$1" + mangledFragmentName + "_$2$3"));
				fragment.InterfaceInterpolators.ToList().ForEach(i =>
					mangledCode = Regex.Replace(mangledCode, @"([^\w\.])(" + i + @")(\W)", "$1" + mangledFragmentName + "_$2$3"));
				fragment.InterfaceTextures.ForEach(t =>
					mangledCode = Regex.Replace(mangledCode, @"(\W)(" + t + @")(\W)", "$1" + mangledFragmentName + "_$2_sampler$3"));

				mangledCode = mangledCode.Replace("void main(", string.Format("void {0}_ps(", mangledFragmentName));
				mangledCode = mangledCode.Replace("INPUT", string.Format("{0}_PIXELINPUT", mangledFragmentName));
				mangledCode = mangledCode.Replace("OUTPUT", "PIXELOUTPUT");
				_output.Append(mangledCode);

				_output.AppendLine();
				_output.AppendLine();
			}
		}

		private void WriteTechnique()
		{
			_output.AppendLine("technique");
			_output.AppendLine("{");
			_output.AppendLine("\tpass");
			_output.AppendLine("\t{");
			_output.AppendLine("\t\tVertexShader = compile vs_2_0 vs();");
			_output.AppendLine("\t\tPixelShader = compile ps_2_0 ps();");
			_output.AppendLine("\t}");
			_output.AppendLine("};");
		}

		private void WriteAllParams()
		{
			_fragments.ForEach(WriteParams);
		}

		private void WriteParams(FragmentContent fragment, string mangledFragmentName)
		{
			if (!fragment.InterfaceParams.Any())
				return;

			_output.AppendLineFormat("// {0} params", mangledFragmentName);
			fragment.InterfaceParams.ForEach(p =>
			{
				FragmentParameterContent parameter = fragment.InterfaceParameterMetadata[p];
				string semantic = (!string.IsNullOrEmpty(parameter.Semantic)) ? " : " + parameter.Semantic : string.Empty;
				_output.AppendLineFormat("{0} {1}_{2}{3};", FragmentParameterDataTypeUtility.ToString(parameter.DataType),
					mangledFragmentName, p, semantic);
			});
			_output.AppendLine();
		}

		private void WriteAllSamplers()
		{
			_fragments.ForEach(WriteSamplers);
		}

		private void WriteSamplers(FragmentContent fragment, string mangledFragmentName)
		{
			if (!fragment.InterfaceTextures.Any())
				return;

			_output.AppendLineFormat("// {0} textures", mangledFragmentName);
			fragment.InterfaceTextures.ForEach(t =>
			{
				_output.AppendLineFormat("texture2D {0};", GetSamplerName(mangledFragmentName, t));
				_output.AppendLineFormat("sampler {0}_sampler = sampler_state {{ Texture = ({0}); }};",
					GetSamplerName(mangledFragmentName, t));
			});
			_output.AppendLine();
		}

		private void WriteAllVertexInputStructures()
		{
			_output.AppendLine("// -------- vertex input structures --------");
			int index = 0;
			_fragments.ForEach((fragment, mangledFragmentName) => WriteVertexInputStructure(fragment, mangledFragmentName, ref index));

			_output.AppendLine("struct VERTEXINPUT");
			_output.AppendLine("{");
			_fragments.ForEach((fragment, mangledFragmentName) => _output.AppendLineFormat("\t{0}_VERTEXINPUT {0};", mangledFragmentName));
			_output.AppendLine("};");
			_output.AppendLine();
			_output.AppendLine("static VERTEXINPUT gVertexInput;");
			_output.AppendLine();
		}

		private void WriteVertexInputStructure(FragmentContent fragment, string mangledFragmentName, ref int index)
		{
			_output.AppendLineFormat("struct {0}_VERTEXINPUT", mangledFragmentName);
			_output.AppendLine("{");

			int tempIndex = index;
			fragment.InterfaceVertex.ForEach(i =>
			{
				FragmentParameterContent parameter = fragment.InterfaceParameterMetadata[i];
				string semantic;
				if (!string.IsNullOrEmpty(parameter.Semantic))
					semantic = parameter.Semantic;
				else
					semantic = "TEXCOORD" + tempIndex++;
				_output.AppendLineFormat("\t{0} {1} : {2};", FragmentParameterDataTypeUtility.ToString(parameter.DataType), i, semantic);
			});
			index = tempIndex;

			_output.AppendLine("};");
			_output.AppendLine();
		}

		private void WriteAllVertexOutputStructures()
		{
			_output.AppendLine("// -------- vertex output structures --------");
			_output.AppendLine("struct VERTEXOUTPUT");
			_output.AppendLine("{");
			_output.AppendLineFormat("\tfloat4 position : POSITION;");
			_fragments.ForEach((fragment, mangledFragmentName) =>
			{
				if (fragment.InterfaceInterpolators.Any())
					_output.AppendLineFormat("\t{0}_PIXELINPUT {0};", mangledFragmentName);
			});
			_output.AppendLine("};");
			_output.AppendLine();
		}

		private void WriteAllPixelInputStructures()
		{
			_output.AppendLine("// -------- pixel input structures --------");
			int index = 0;
			_fragments.ForEach((fragment, mangledFragmentName) => WritePixelInputStructure(fragment, mangledFragmentName, ref index));

			_output.AppendLine("struct PIXELINPUT");
			_output.AppendLine("{");
			_fragments.ForEach((fragment, mangledFragmentName) =>
			{
				if (fragment.InterfaceInterpolators.Any())
					_output.AppendLineFormat("\t{0}_PIXELINPUT {0};", mangledFragmentName);
			});
			_output.AppendLine("};");
			_output.AppendLine();
			_output.AppendLine("static PIXELINPUT gPixelInput;");
			_output.AppendLine();
		}

		private void WritePixelInputStructure(FragmentContent fragment, string mangledFragmentName, ref int index)
		{
			if (!fragment.InterfaceInterpolators.Any())
				return;

			_output.AppendLineFormat("struct {0}_PIXELINPUT", mangledFragmentName);
			_output.AppendLine("{");

			int tempIndex = index;
			fragment.InterfaceInterpolators.ForEach(i =>
			{
				FragmentParameterContent parameter = fragment.InterfaceParameterMetadata[i];
				_output.AppendLineFormat("\t{0} {1} : TEXCOORD{2};", FragmentParameterDataTypeUtility.ToString(parameter.DataType), i, tempIndex++);
			});
			index = tempIndex;

			_output.AppendLine("};");
			_output.AppendLine();
		}

		private static string GetSamplerName(string mangledFragmentName, string textureName)
		{
			return string.Format("{0}_{1}", mangledFragmentName, textureName);
		}

		private static string GetInputStructName(string mangledFragmentName)
		{
			return string.Format("{0}_INPUT", mangledFragmentName);
		}
	}
}