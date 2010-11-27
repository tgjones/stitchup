using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Content.Pipeline;
using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;
using StitchUp.Content.Pipeline.FragmentLinking.EffectModel;

namespace StitchUp.Content.Pipeline.FragmentLinking.PreProcessor
{
	public static class StitchedEffectPreProcessor
	{
		public static List<ExportedValue> GetExportedValues(StitchedEffectSymbol stitchedEffect)
		{
			ExportDictionary exports = new ExportDictionary();
			List<ExportedValue> exportedValues = new List<ExportedValue>();
			foreach (StitchedFragmentSymbol stitchedFragment in stitchedEffect.StitchedFragments)
			{
				foreach (ShaderCodeBlockNode codeBlock in stitchedFragment.FragmentNode.VertexShaders)
					PreProcessCodeBlock(stitchedFragment.UniqueName, codeBlock, exports, exportedValues, true);
				foreach (ShaderCodeBlockNode codeBlock in stitchedFragment.FragmentNode.PixelShaders)
					PreProcessCodeBlock(stitchedFragment.UniqueName, codeBlock, exports, exportedValues, true);
			}
			return exportedValues;
		}

		public static string PreProcessCodeBlock(string uniqueFragmentName, ShaderCodeBlockNode codeBlock, Dictionary<string, List<string>> exports, List<ExportedValue> exportedValues = null, bool onlyExports = false)
		{
			string processedCode = codeBlock.Code;

			if (!onlyExports)
			{
				// Check program for imports.
				processedCode = ProcessImports(processedCode, exports);
			}

			// Check program for exports.
			processedCode = ProcessExports(processedCode, exports, uniqueFragmentName, exportedValues);

			return processedCode;
		}

		private static string ProcessExports(string processedCode, Dictionary<string, List<string>> exports, string uniqueFragmentName, List<ExportedValue> exportedValues)
		{
			const string exportPattern = @"(?<WHITESPACE>[ \t]+)(?<EXPORT>export\((?<TYPE>[\w]+), (?<NAME>[\w]+), (?<VALUE>[\s\S]+?)\);)";
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
					if (exportedValues != null)
						exportedValues.Add(new ExportedValue
						{
							Type = match.Groups["TYPE"].Value,
							Name = variableName
						});
					exports[exportName].Add(variableName);
				}
			}
			processedCode = Regex.Replace(processedCode, exportPattern,
				string.Format("${{WHITESPACE}}// metafunction: ${{EXPORT}}\r${{WHITESPACE}}{0}_export_${{NAME}} = ${{VALUE}};", uniqueFragmentName));
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