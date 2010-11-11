using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Content.Pipeline;
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
			// We keep track of what has been exported using this dictionary.
			Dictionary<string, List<string>> exports = new Dictionary<string, List<string>>();

			_fragments.ForEach((content, s) => WriteVertexShader(content, s, exports));

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

		private void WriteVertexShader(FragmentContent fragment, string mangledFragmentName, Dictionary<string, List<string>> exports)
		{
			_output.AppendLineFormat("// -------- vertex shader {0} --------", mangledFragmentName);

			FragmentCodeContent shader = fragment.CodeBlocks.FirstOrDefault(s => s.ShaderType == FragmentCodeShaderType.VertexShader);

			if (shader != null)
			{
				WriteShaderCode(fragment, mangledFragmentName, shader, "VERTEXINPUT", "VERTEXOUTPUT", "vs", exports);
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

		private void WriteShaderCode(FragmentContent fragment, string mangledFragmentName, FragmentCodeContent shader,
			string inputStructName, string outputStructName, string functionSuffix, Dictionary<string, List<string>> exports)
		{
			string mangledCode = shader.Code;

			// Replace interpolators and sampler names which are used in the code with the mangled names.
			fragment.InterfaceParams.ToList().ForEach(i =>
				mangledCode = Regex.Replace(mangledCode, @"([^\w\.])(" + i + @")(\W)", "$1" + mangledFragmentName + "_$2$3"));
			fragment.InterfaceTextures.ForEach(t =>
				mangledCode = Regex.Replace(mangledCode, @"(\W)(" + t + @")(\W)", "$1" + mangledFragmentName + "_$2_sampler$3"));

			// Check program for exports.
			const string exportPattern = @"export\((?<TYPE>[\w]+), (?<NAME>[\w]+), (?<VALUE>[\s\S]+?)\);";
			MatchCollection exportMatches = Regex.Matches(mangledCode, exportPattern);
			foreach (Match match in exportMatches)
			{
				// Values might be exporting used the same name by multiple fragments; in this case, we should declare
				// a variable for each fragment that exports the value. But if the same fragment exports a value multiple
				// times, we should only declare the variable once.
				string exportName = match.Groups["NAME"].Value;
				if (!exports.ContainsKey(exportName))
					exports[exportName] = new List<string>();

				string variableName = string.Format("{0}_export_{1}", mangledFragmentName, exportName);
				if (!exports[exportName].Contains(variableName))
				{
					_output.AppendLine(string.Format("static {0} {1}; // exported value", match.Groups["TYPE"].Value, variableName));
					exports[exportName].Add(variableName);
				}
			}

			mangledCode = mangledCode.Replace("void main(", string.Format("void {0}_{1}(", mangledFragmentName, functionSuffix));
			mangledCode = mangledCode.Replace("INPUT", string.Format("{0}_{1}", mangledFragmentName, inputStructName));
			mangledCode = mangledCode.Replace("OUTPUT", outputStructName);

			// Check program for imports.
			const string importPattern = @"import\((?<NAME>[\w]+), (?<OPERATION>[\s\S]+?)\);";
			mangledCode = Regex.Replace(mangledCode, importPattern,
				m =>
				{
					// Look up full variable names from matched name
					if (!exports.ContainsKey(m.Groups["NAME"].Value))
						throw new InvalidContentException(string.Format("Export with name '{0}' was not found.", m.Groups["NAME"].Value));
					List<string> variableNames = exports[m.Groups["NAME"].Value];

					string replacement = "// metafunction: $0";
					foreach (string variableName in variableNames)
						replacement += string.Format("\n\t{0};",
							Regex.Replace(m.Groups["OPERATION"].Value, @"(\W)(" + m.Groups["NAME"].Value + @")(\W|$)", "$1" + variableName + "$3")
						);

					return m.Result(replacement);
				}
			);

			_output.Append(mangledCode);
		}

		private void WriteAllPixelShaders()
		{
			// We keep track of what has been exported using this dictionary.
			Dictionary<string, List<string>> exports = new Dictionary<string, List<string>>();
			_fragments.ForEach((content, s) => WritePixelShader(content, s, exports));

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

		private void WritePixelShader(FragmentContent fragment, string mangledFragmentName, Dictionary<string, List<string>> exports)
		{
			FragmentCodeContent shader = fragment.CodeBlocks.FirstOrDefault(s => s.ShaderType == FragmentCodeShaderType.PixelShader);
			if (shader != null)
			{
				_output.AppendLineFormat("// -------- pixel shader {0} --------", mangledFragmentName);

				WriteShaderCode(fragment, mangledFragmentName, shader, "PIXELINPUT", "PIXELOUTPUT", "ps", exports);

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
			fragment.InterfaceParams.ForEach(p => _output.AppendLine("\t" + GetVariableDeclaration(fragment, mangledFragmentName, p)));
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
				_output.AppendLineFormat(GetVariableDeclaration(fragment, mangledFragmentName, t,
					new FragmentParameterContent { DataType = FragmentParameterDataType.Texture2D }));
				_output.AppendLineFormat("sampler {0}_{1}_sampler = sampler_state {{ Texture = ({0}_{1}); }};",
					mangledFragmentName, t);
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

		private void WriteVertexInputStructure(FragmentContent fragment, string mangledFragmentName, ref int index)
		{
			WriteShaderInputStructure(fragment, mangledFragmentName, ref index, "VERTEXINPUT", fragment.InterfaceVertex);
		}

		private void WritePixelInputStructure(FragmentContent fragment, string mangledFragmentName, ref int index)
		{
			if (!fragment.InterfaceInterpolators.Any())
				return;

			WriteShaderInputStructure(fragment, mangledFragmentName, ref index, "PIXELINPUT", fragment.InterfaceInterpolators);
		}

		private void WriteShaderInputStructure(FragmentContent fragment, string mangledFragmentName, ref int index, string structSuffix,
			List<string> fields)
		{
			_output.AppendLineFormat("struct {0}_{1}", mangledFragmentName, structSuffix);
			_output.AppendLine("{");

			int tempIndex = index;
			fields.ForEach(i =>
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

		private static string GetVariableDeclaration(FragmentContent fragment, string mangledFragmentName, string variableName,
			FragmentParameterContent fallbackMetadata = null)
		{
			FragmentParameterContent metadata;
			if (fragment.InterfaceParameterMetadata.ContainsKey(variableName))
				metadata = fragment.InterfaceParameterMetadata[variableName];
			else if (fallbackMetadata != null)
				metadata = fallbackMetadata;
			else
				throw new Exception("Could not find parameter metadata and no fallback was supplied.");

			string semantic = (!string.IsNullOrEmpty(metadata.Semantic)) ? " : " + metadata.Semantic : string.Empty;
			return string.Format("{0} {1}_{2}{3};", FragmentParameterDataTypeUtility.ToString(metadata.DataType),
					mangledFragmentName, variableName, semantic);
		}
	}
}