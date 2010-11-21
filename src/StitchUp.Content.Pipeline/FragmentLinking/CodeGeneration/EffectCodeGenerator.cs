using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;

namespace StitchUp.Content.Pipeline.FragmentLinking.CodeGeneration
{
	public class EffectCodeGenerator
	{
		private readonly StitchedEffectNode _stitchedEffect;
		private readonly ShaderProfile _targetShaderProfile;
		private readonly StringBuilder _output;

		public EffectCodeGenerator(StitchedEffectNode stitchedEffect, ShaderProfile targetShaderProfile)
		{
			_stitchedEffect = stitchedEffect;
			_targetShaderProfile = targetShaderProfile;
			_output = new StringBuilder();
		}

		public string GenerateCode()
		{
			WriteAllHeaderCode();
			WriteAllParams();
			WriteAllSamplers();
			WriteAllVertexInputStructures();
			WriteAllPixelInputStructures();
			WriteAllVertexOutputStructures();
			WriteAllPixelOutputStructures();
			WriteAllVertexShaders();
			WriteAllPixelShaders();
			WriteTechnique();

			return _output.ToString();
		}

		private void WriteAllPixelOutputStructures()
		{
			_output.AppendLine("// -------- pixel output structures --------");

			SemanticGenerator semanticGenerator = new SemanticGenerator("COLOR", 1);
			ForEachFragment(f => WritePixelOutputStructure(f, semanticGenerator));

			_output.AppendLine("struct PIXELOUTPUT");
			_output.AppendLine("{");
			_output.AppendLineFormat("\tfloat4 color : COLOR0;");
			ForEachFragment(f =>
			{
				if (f.FragmentNode.PixelOutputs != null && f.FragmentNode.PixelOutputs.VariableDeclarations.Any())
					_output.AppendLineFormat("\t{0}_PIXELOUTPUT {0};", f.UniqueName);
			});
			_output.AppendLine("};");
			_output.AppendLine();
		}

		private void WritePixelOutputStructure(StitchedFragmentNode stitchedFragment, SemanticGenerator semanticGenerator)
		{
			WriteShaderInputStructure(stitchedFragment, semanticGenerator, "PIXELOUTPUT", stitchedFragment.FragmentNode.PixelOutputs, true);
		}

		private void WriteAllVertexShaders()
		{
			ForEachFragment(WriteVertexShader);

			_output.AppendLine("// -------- vertex shader entrypoint --------");
			_output.AppendLine("VERTEXOUTPUT vs(const VERTEXINPUT i)");
			_output.AppendLine("{");
			_output.AppendLine("\tgVertexInput = i;");
			_output.AppendLine();
			_output.AppendLine("\tVERTEXOUTPUT output = (VERTEXOUTPUT) 0;");
			_output.AppendLine();

			ForEachFragment(f => _output.AppendLineFormat("\t{0}_vs(gVertexInput.{0}, output);", f.UniqueName));

			_output.AppendLine();
			_output.AppendLine("\treturn output;");
			_output.AppendLine("}");
			_output.AppendLine();
		}

		private void WriteVertexShader(StitchedFragmentNode stitchedFragment)
		{
			_output.AppendLineFormat("// -------- vertex shader {0} --------", stitchedFragment.UniqueName);

			ShaderCodeBlockNode shader = stitchedFragment.FragmentNode.VertexShaders.GetCodeBlock(_targetShaderProfile);

			if (shader != null)
			{
				string shaderCode = ReplaceOutputCalls(shader.Code, stitchedFragment.UniqueName);
				WriteShaderCode(stitchedFragment, shaderCode, "VERTEXINPUT", "VERTEXOUTPUT", "vs");
			}
			else
			{
				// Need to auto-generate vertex shader. Simply pass through all vertex inputs.
				_output.AppendLineFormat("void {0}_vs({0}_VERTEXINPUT input, inout VERTEXOUTPUT output)", stitchedFragment.UniqueName);
				_output.AppendLine("{");
				_output.Append(GetVertexPassThroughCode(stitchedFragment));
				_output.AppendLine("}");
			}

			_output.AppendLine();
			_output.AppendLine();
		}

		private static string ReplaceOutputCalls(string shaderCode, string uniqueName)
		{
			// Replace "output(*variable*);" with code to pass input value (i.e. vertex attribute)
			// with matching name through to output value (i.e. interpolator).
			return Regex.Replace(shaderCode, @"output\((?<NAME>[\w]+), (?<VALUE>[\s\S]+?)\);",
				string.Format("output.{0}.${{NAME}} = ${{VALUE}};", uniqueName));
		}

		private static string GetVertexPassThroughCode(StitchedFragmentNode stitchedFragment)
		{
			StringBuilder sb = new StringBuilder();
			if (stitchedFragment.FragmentNode.VertexAttributes != null)
				foreach (var variable in stitchedFragment.FragmentNode.VertexAttributes.VariableDeclarations)
					sb.AppendLine(GetVertexPassThroughCode(stitchedFragment, variable));
			return sb.ToString();
		}

		private static string GetVertexPassThroughCode(StitchedFragmentNode stitchedFragment, VariableDeclarationNode variable)
		{
			return string.Format("\toutput.{0}.{1} = input.{1};", stitchedFragment.UniqueName, variable.Name);
		}

		private void WriteShaderCode(StitchedFragmentNode stitchedFragment, string shaderCode,
			string inputStructName, string outputStructName, string functionSuffix)
		{
			string mangledCode = shaderCode;

			// Replace interpolators and sampler names which are used in the code with the mangled names.
			mangledCode = ReplaceVariableNames(stitchedFragment, stitchedFragment.FragmentNode.Parameters, mangledCode);
			//mangledCode = ReplaceVariableNames(stitchedFragment, stitchedFragment.FragmentNode.Interpolators, mangledCode);
			if (stitchedFragment.FragmentNode.Textures != null)
				stitchedFragment.FragmentNode.Textures.VariableDeclarations.ForEach(t =>
					mangledCode = Regex.Replace(mangledCode, @"(\W)(" + t.Name + @")(\W)", "$1" + stitchedFragment.UniqueName + "_$2_sampler$3"));

			mangledCode = mangledCode.Replace("void main(", string.Format("void {0}_{1}(", stitchedFragment.UniqueName, functionSuffix));
			mangledCode = mangledCode.Replace("INPUT", string.Format("{0}_{1}", stitchedFragment.UniqueName, inputStructName));
			mangledCode = mangledCode.Replace("OUTPUT", outputStructName);

			_output.Append(mangledCode.Replace("\r", Environment.NewLine));
		}

		private static string ReplaceVariableNames(StitchedFragmentNode stitchedFragment, ParameterBlockNode parameters, string mangledCode)
		{
			if (parameters != null)
				parameters.VariableDeclarations.ToList().ForEach(i =>
					mangledCode = Regex.Replace(mangledCode, @"([^\w\.])(" + i.Name + @")(\W)", "$1" + stitchedFragment.UniqueName + "_$2$3"));
			return mangledCode;
		}

		private void WriteAllPixelShaders()
		{
			ForEachFragment(WritePixelShader);

			_output.AppendLine("// -------- pixel shader entrypoint --------");
			_output.AppendLine("PIXELOUTPUT ps(const PIXELINPUT i)");
			_output.AppendLine("{");
			_output.AppendLine("\tgPixelInput = i;");
			_output.AppendLine();
			_output.AppendLine("\tPIXELOUTPUT output = (PIXELOUTPUT) 0;");
			_output.AppendLine();
			ForEachFragment(f =>
			{
				ShaderCodeBlockNode shader = f.FragmentNode.PixelShaders.GetCodeBlock(_targetShaderProfile);
				if (shader != null)
					_output.AppendLineFormat("\t{0}_ps(gPixelInput.{0}, output);", f.UniqueName);
			});
			_output.AppendLine();
			_output.AppendLine("\treturn output;");
			_output.AppendLine("}");
			_output.AppendLine();
		}

		private void WritePixelShader(StitchedFragmentNode stitchedFragment)
		{
			ShaderCodeBlockNode codeBlock = stitchedFragment.FragmentNode.PixelShaders.GetCodeBlock(_targetShaderProfile);
			if (codeBlock != null)
			{
				_output.AppendLineFormat("// -------- pixel shader {0} --------", stitchedFragment.UniqueName);

				string shaderCode = ReplaceOutputCalls(codeBlock.Code, stitchedFragment.UniqueName);
				WriteShaderCode(stitchedFragment, shaderCode, "PIXELINPUT", "PIXELOUTPUT", "ps");

				_output.AppendLine();
				_output.AppendLine();
			}
		}

		private void WriteTechnique()
		{
			_output.AppendLine("// -------- technique --------");
			_output.AppendLine("technique");
			_output.AppendLine("{");
			_output.AppendLine("\tpass");
			_output.AppendLine("\t{");
			_output.AppendLineFormat("\t\tVertexShader = compile vs_{0} vs();", _targetShaderProfile.GetDescription());
			_output.AppendLineFormat("\t\tPixelShader = compile ps_{0} ps();", _targetShaderProfile.GetDescription());
			_output.AppendLine("\t}");
			_output.AppendLine("};");
		}

		private void WriteAllHeaderCode()
		{
		    List<string> seenFragmentTypes = new List<string>();
            foreach (StitchedFragmentNode stitchedFragmentNode in _stitchedEffect.StitchedFragments)
            {
                if (seenFragmentTypes.Contains(stitchedFragmentNode.FragmentNode.Name))
                    continue;
                WriteHeaderCode(stitchedFragmentNode);
                seenFragmentTypes.Add(stitchedFragmentNode.FragmentNode.Name);
            }
		}

		private void WriteHeaderCode(StitchedFragmentNode stitchedFragment)
		{
			if (stitchedFragment.FragmentNode.HeaderCode == null || string.IsNullOrEmpty(stitchedFragment.FragmentNode.HeaderCode.Code))
				return;

			_output.AppendLineFormat("// {0} header code", stitchedFragment.UniqueName);
			_output.Append(stitchedFragment.FragmentNode.HeaderCode.Code);
			_output.AppendLine();
			_output.AppendLine();
		}

		private void WriteAllParams()
		{
			ForEachFragment(WriteParams);
		}

		private void WriteParams(StitchedFragmentNode stitchedFragment)
		{
			if (stitchedFragment.FragmentNode.Parameters == null || !stitchedFragment.FragmentNode.Parameters.VariableDeclarations.Any())
				return;

			_output.AppendLineFormat("// {0} params", stitchedFragment.UniqueName);
			stitchedFragment.FragmentNode.Parameters.VariableDeclarations.ForEach(p => _output.AppendLine(GetVariableDeclaration(stitchedFragment, p)));
			_output.AppendLine();
		}

		private void WriteAllSamplers()
		{
			ForEachFragment(WriteSamplers);
		}

		private void WriteSamplers(StitchedFragmentNode stitchedFragment)
		{
			if (stitchedFragment.FragmentNode.Textures == null || !stitchedFragment.FragmentNode.Textures.VariableDeclarations.Any())
				return;

			_output.AppendLineFormat("// {0} textures", stitchedFragment.UniqueName);
			stitchedFragment.FragmentNode.Textures.VariableDeclarations.ForEach(t =>
			{
				_output.AppendLineFormat(GetVariableDeclaration(stitchedFragment, t));
				_output.AppendLineFormat("sampler {0}_{1}_sampler = sampler_state {{ Texture = ({0}_{1}); }};",
					stitchedFragment.UniqueName, t.Name);
			});
			_output.AppendLine();
		}

		private void WriteAllVertexInputStructures()
		{
			_output.AppendLine("// -------- vertex input structures --------");

			SemanticGenerator semanticGenerator = new SemanticGenerator("TEXCOORD");
			ForEachFragment(f => WriteVertexInputStructure(f, semanticGenerator));

			_output.AppendLine("struct VERTEXINPUT");
			_output.AppendLine("{");

			ForEachFragment(f => _output.AppendLineFormat("\t{0}_VERTEXINPUT {0};", f.UniqueName));

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
			ForEachFragment(f =>
			{
				if (f.FragmentNode.Interpolators != null && f.FragmentNode.Interpolators.VariableDeclarations.Any())
					_output.AppendLineFormat("\t{0}_PIXELINPUT {0};", f.UniqueName);
			});
			_output.AppendLine("};");
			_output.AppendLine();
		}

		private void WriteAllPixelInputStructures()
		{
			_output.AppendLine("// -------- pixel input structures --------");

			SemanticGenerator semanticGenerator = new SemanticGenerator("TEXCOORD");
			ForEachFragment(f => WritePixelInputStructure(f, semanticGenerator));

			_output.AppendLine("struct PIXELINPUT");
			_output.AppendLine("{");
			ForEachFragment(f => _output.AppendLineFormat("\t{0}_PIXELINPUT {0};", f.UniqueName));
			_output.AppendLine("};");
			_output.AppendLine();
			_output.AppendLine("static PIXELINPUT gPixelInput;");
			_output.AppendLine();
		}

		private void WriteVertexInputStructure(StitchedFragmentNode stitchedFragment, SemanticGenerator semanticGenerator)
		{
			WriteShaderInputStructure(stitchedFragment, semanticGenerator, "VERTEXINPUT", stitchedFragment.FragmentNode.VertexAttributes, false);
		}

		private void WritePixelInputStructure(StitchedFragmentNode stitchedFragment, SemanticGenerator semanticGenerator)
		{
			WriteShaderInputStructure(stitchedFragment, semanticGenerator, "PIXELINPUT", stitchedFragment.FragmentNode.Interpolators, true);
		}

		private void WriteShaderInputStructure(StitchedFragmentNode stitchedFragment, SemanticGenerator semanticGenerator, string structSuffix,
			ParameterBlockNode parameterBlock, bool alwaysUseTexCoords)
		{
			WriteShaderStructure(stitchedFragment, semanticGenerator, structSuffix, parameterBlock);
		}

		private void WriteShaderStructure(StitchedFragmentNode stitchedFragment, SemanticGenerator semanticGenerator,
			string structSuffix, ParameterBlockNode parameterBlock)
		{
			_output.AppendLineFormat("struct {0}_{1}", stitchedFragment.UniqueName, structSuffix);
			_output.AppendLine("{");

			if (parameterBlock != null)
				parameterBlock.VariableDeclarations.ForEach(v =>
				{
					string semantic = semanticGenerator.GetNextSemantic(v);
					_output.AppendLineFormat("\t{0} {1} : {2};",
						Token.GetString(v.DataType), v.Name, semantic);
				});

			_output.AppendLine("};");
			_output.AppendLine();
		}

		private static string GetVariableDeclaration(StitchedFragmentNode stitchedFragment, VariableDeclarationNode variable)
		{
			string arrayStuff = (variable.IsArray && variable.ArraySize != null) ? "[" + variable.ArraySize.Value + "]" : string.Empty;
			string semantic = (!string.IsNullOrEmpty(variable.Semantic)) ? " : " + variable.Semantic : string.Empty;
			string initialValue = (!string.IsNullOrEmpty(variable.InitialValue)) ? " = " + variable.InitialValue : string.Empty;

            return string.Format("{0} {1}_{2}{3}{4}{5};",
				Token.GetString(variable.DataType), stitchedFragment.UniqueName,
				variable.Name, arrayStuff, semantic, initialValue);
		}

		private void ForEachFragment(Action<StitchedFragmentNode> action)
		{
			foreach (StitchedFragmentNode stitchedFragmentNode in _stitchedEffect.StitchedFragments)
				action(stitchedFragmentNode);
		}
	}
}