using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;
using StitchUp.Content.Pipeline.FragmentLinking.EffectModel;
using StitchUp.Content.Pipeline.FragmentLinking.PreProcessor;

namespace StitchUp.Content.Pipeline.FragmentLinking.Generator
{
	public static class ShaderGenerator
	{
		public static void WriteAllVertexShaders(EffectCodeGenerator generator, StitchedEffectSymbol stitchedEffect)
		{
			generator.ForEachPass(WriteVertexShaders);
		}

		public static void WriteVertexShaders(EffectCodeGenerator generator, 
			TechniqueSymbol technique, TechniquePassSymbol pass, string uniquePassName)
		{
			Dictionary<string, List<string>> exports = new Dictionary<string, List<string>>();
			pass.ForEachFragment(generator, (codeGenerator, symbol, s) => WriteVertexShader(codeGenerator, symbol, s, exports));

			ScriptTextWriter writer = generator.Writer;

			// Write one vertex shader for each technique pass.
			writer.WriteLine("// -------- technique {0}, pass {1} vertex shader entrypoint --------", technique.Name, pass.Name);
			writer.WriteLine("{0}_VERTEXOUTPUT {0}_vs(const {0}_VERTEXINPUT i)", uniquePassName);
			writer.WriteLine("{");
			writer.WriteLine("\tgVertexInput_{0} = i;", uniquePassName);
			writer.WriteLine();
			writer.WriteLine("\t{0}_VERTEXOUTPUT output = ({0}_VERTEXOUTPUT) 0;", uniquePassName);
			writer.WriteLine();

			pass.ForEachFragment(generator, (g, f, s) => g.Writer.WriteLine("\t{0}_{1}_vs(gVertexInput_{0}.{1}, output);", uniquePassName, f.UniqueName));

			writer.WriteLine();
			writer.WriteLine("\treturn output;");
			writer.WriteLine("}");
			writer.WriteLine();
		}

		private static void WriteVertexShader(EffectCodeGenerator generator, StitchedFragmentSymbol fragment,
			string uniquePassName, Dictionary<string, List<string>> exports)
		{
			ScriptTextWriter writer = generator.Writer;

			writer.WriteLine("// -------- vertex shader {0} --------", fragment.UniqueName);

			ShaderCodeBlockNode shader = fragment.FragmentNode.VertexShaders.GetCodeBlock(generator.Context.TargetShaderProfile);

			if (shader != null)
			{
				string shaderCode = StitchedEffectPreProcessor.PreProcessCodeBlock(fragment.UniqueName, shader, exports);
				shaderCode = ReplaceOutputCalls(shaderCode, fragment.UniqueName);
				WriteShaderCode(generator, fragment, shaderCode, "VERTEXINPUT", "VERTEXOUTPUT", "vs", uniquePassName);
			}
			else
			{
				// Need to auto-generate vertex shader. Simply pass through all vertex inputs.
				writer.WriteLine("void {0}_{1}_vs({0}_{1}_VERTEXINPUT input, inout {0}_VERTEXOUTPUT output)", uniquePassName, fragment.UniqueName);
				writer.WriteLine("{");
				writer.Write(GetVertexPassThroughCode(fragment));
				writer.WriteLine("}");
			}

			writer.WriteLine();
			writer.WriteLine();
		}

		private static string ReplaceOutputCalls(string shaderCode, string uniqueName)
		{
			// Replace "output(*variable*);" with code to pass input value (i.e. vertex attribute)
			// with matching name through to output value (i.e. interpolator).
			return Regex.Replace(shaderCode, @"output\((?<NAME>[\w]+), (?<VALUE>[\s\S]+?)\);",
				string.Format("output.{0}.${{NAME}} = ${{VALUE}};", uniqueName));
		}

		private static string GetVertexPassThroughCode(StitchedFragmentSymbol stitchedFragment)
		{
			StringBuilder sb = new StringBuilder();
			if (stitchedFragment.FragmentNode.VertexAttributes != null)
				foreach (var variable in stitchedFragment.FragmentNode.VertexAttributes.VariableDeclarations)
					sb.AppendLine(GetVertexPassThroughCode(stitchedFragment, variable));
			return sb.ToString();
		}

		private static string GetVertexPassThroughCode(StitchedFragmentSymbol stitchedFragment, VariableDeclarationNode variable)
		{
			return string.Format("\toutput.{0}.{1} = input.{1};", stitchedFragment.UniqueName, variable.Name);
		}

		private static void WriteShaderCode(EffectCodeGenerator generator, StitchedFragmentSymbol stitchedFragment, string shaderCode,
			string inputStructName, string outputStructName, string functionSuffix, string uniquePassName)
		{
			string mangledCode = shaderCode;

			// Replace interpolators and sampler names which are used in the code with the mangled names.
			mangledCode = ReplaceVariableNames(stitchedFragment, stitchedFragment.FragmentNode.Parameters, mangledCode);
			//mangledCode = ReplaceVariableNames(stitchedFragment, stitchedFragment.FragmentNode.Interpolators, mangledCode);
			if (stitchedFragment.FragmentNode.Textures != null)
				stitchedFragment.FragmentNode.Textures.VariableDeclarations.ForEach(t =>
					mangledCode = Regex.Replace(mangledCode, @"(\W)(" + t.Name + @")(\W)", "$1" + stitchedFragment.UniqueName + "_$2_sampler$3"));

			mangledCode = mangledCode.Replace("void main(", string.Format("void {0}_{1}_{2}(", uniquePassName, stitchedFragment.UniqueName, functionSuffix));

			// This is just here to support surface shaders. Ideally it would be done somewhere else.
			mangledCode = Regex.Replace(mangledCode, @"(\b)surface\(", string.Format("$1{0}_{1}_{2}_surface(", uniquePassName, stitchedFragment.UniqueName, functionSuffix));

			mangledCode = mangledCode.Replace("INPUT", string.Format("{0}_{1}_{2}", uniquePassName, stitchedFragment.UniqueName, inputStructName));
			mangledCode = mangledCode.Replace("OUTPUT", uniquePassName + "_" + outputStructName);

			generator.Writer.Write(mangledCode.Replace("\r", Environment.NewLine));
		}

		private static string ReplaceVariableNames(StitchedFragmentSymbol stitchedFragment, ParameterBlockNode parameters, string mangledCode)
		{
			if (parameters != null)
				parameters.VariableDeclarations.ToList().ForEach(i =>
					mangledCode = Regex.Replace(mangledCode, @"([^\w\.])(" + i.Name + @")(\W)", "$1" + stitchedFragment.UniqueName + "_$2$3"));
			return mangledCode;
		}

		public static void WriteAllPixelShaders(EffectCodeGenerator generator, StitchedEffectSymbol stitchedEffect)
		{
			generator.ForEachPass(WritePixelShaders);
		}

		private static void WritePixelShaders(EffectCodeGenerator generator, 
			TechniqueSymbol technique, TechniquePassSymbol pass, string uniquePassName)
		{
			Dictionary<string, List<string>> exports = new Dictionary<string, List<string>>();
			pass.ForEachFragment(generator, (codeGenerator, symbol, s) => WritePixelShader(codeGenerator, symbol, s, exports));

			ScriptTextWriter writer = generator.Writer;
			writer.WriteLine("// -------- technique {0}, pass {1} pixel shader entrypoint --------", technique.Name, pass.Name);
			writer.WriteLine("{0}_PIXELOUTPUT {0}_ps(const {0}_PIXELINPUT i)", uniquePassName);
			writer.WriteLine("{");
			writer.WriteLine("\tgPixelInput_{0} = i;", uniquePassName);
			writer.WriteLine();
			writer.WriteLine("\t{0}_PIXELOUTPUT output = ({0}_PIXELOUTPUT) 0;", uniquePassName);
			writer.WriteLine();
			pass.ForEachFragment(generator, (g, f, s) =>
			{
				ShaderCodeBlockNode shader = f.FragmentNode.PixelShaders.GetCodeBlock(generator.Context.TargetShaderProfile);
				if (shader != null)
					writer.WriteLine("\t{0}_{1}_ps(gPixelInput_{0}.{1}, output);", uniquePassName, f.UniqueName);
			});
			writer.WriteLine();
			writer.WriteLine("\treturn output;");
			writer.WriteLine("}");
			writer.WriteLine();
		}

		private static void WritePixelShader(EffectCodeGenerator generator, StitchedFragmentSymbol stitchedFragment,
			string uniquePassName, Dictionary<string, List<string>> exports)
		{
			ShaderCodeBlockNode codeBlock = stitchedFragment.FragmentNode.PixelShaders.GetCodeBlock(generator.Context.TargetShaderProfile);
			if (codeBlock != null)
			{
				generator.Writer.WriteLine("// -------- pixel shader {0} --------", stitchedFragment.UniqueName);

				string shaderCode = StitchedEffectPreProcessor.PreProcessCodeBlock(stitchedFragment.UniqueName, codeBlock, exports);
				shaderCode = ReplaceOutputCalls(shaderCode, stitchedFragment.UniqueName);
				WriteShaderCode(generator, stitchedFragment, shaderCode, "PIXELINPUT", "PIXELOUTPUT", "ps", uniquePassName);

				generator.Writer.WriteLine();
				generator.Writer.WriteLine();
			}
		}
	}
}