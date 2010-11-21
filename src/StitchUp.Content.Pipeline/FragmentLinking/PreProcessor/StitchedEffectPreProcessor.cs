using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Content.Pipeline;
using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;
using StitchUp.Content.Pipeline.Graphics;

namespace StitchUp.Content.Pipeline.FragmentLinking.PreProcessor
{
	public class StitchedEffectPreProcessor
	{
		public void PreProcess(StitchedEffectNode stitchedEffect)
		{
			// Replace all calls to export(...) and import(...)
			// with expanded versions.
			Dictionary<string, List<string>> exports = new Dictionary<string, List<string>>();
			foreach (StitchedFragmentNode stitchedFragment in stitchedEffect.StitchedFragments)
			{
				foreach (ShaderCodeBlockNode codeBlock in stitchedFragment.FragmentNode.VertexShaders)
					PreProcessCodeBlock(stitchedFragment.UniqueName, codeBlock, exports);
				foreach (ShaderCodeBlockNode codeBlock in stitchedFragment.FragmentNode.PixelShaders)
					PreProcessCodeBlock(stitchedFragment.UniqueName, codeBlock, exports);
			}
		}

		private static void PreProcessCodeBlock(string uniqueFragmentName, ShaderCodeBlockNode codeBlock, Dictionary<string, List<string>> exports)
		{
			string processedCode = codeBlock.Code;

			// Check program for imports.
			processedCode = ProcessImports(processedCode, exports);

			// Check program for exports.
			processedCode = ProcessExports(processedCode, exports, uniqueFragmentName);

			codeBlock.Code = processedCode;
		}

		private static string ProcessExports(string processedCode, Dictionary<string, List<string>> exports, string uniqueFragmentName)
		{
			const string exportPattern = @"export\((?<TYPE>[\w]+), (?<NAME>[\w]+), (?<VALUE>[\s\S]+?)\);";
			MatchCollection exportMatches = Regex.Matches(processedCode, exportPattern);
			foreach (Match match in exportMatches)
			{
				// Values might be exporting used the same name by multiple fragments; in this case, we should declare
				// a variable for each fragment that exports the value. But if the same fragment exports a value multiple
				// times, we should only declare the variable once.
				string exportName = match.Groups["NAME"].Value;
				if (!exports.ContainsKey(exportName))
					exports[exportName] = new List<string>();

				string variableName = string.Format("{0}_export_{1}", uniqueFragmentName, exportName);
				if (!exports[exportName].Contains(variableName))
				{
					processedCode = string.Format("static {0} {1}; // exported value", match.Groups["TYPE"].Value, variableName)
						+ Environment.NewLine
							+ processedCode;
					exports[exportName].Add(variableName);
				}
			}
			processedCode = Regex.Replace(processedCode, exportPattern, string.Format("// metafunction: $0\n\t{0}_export_${{NAME}} = ${{VALUE}};", uniqueFragmentName));
			return processedCode;
		}

		private static string ProcessImports(string processedCode, Dictionary<string, List<string>> exports)
		{
			const string importPattern = @"import\((?<NAME>[\w]+), (?<OPERATION>[\s\S]+?)\);";
			processedCode = Regex.Replace(processedCode, importPattern,
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
			return processedCode;
		}
	}
}