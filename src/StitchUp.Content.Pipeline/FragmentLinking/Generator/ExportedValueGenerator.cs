using System.Collections.Generic;
using StitchUp.Content.Pipeline.FragmentLinking.PreProcessor;

namespace StitchUp.Content.Pipeline.FragmentLinking.Generator
{
	public static class ExportedValueGenerator
	{
		public static void GenerateExportDeclarations(EffectCodeGenerator generator, List<ExportedValue> exportedValues)
		{
			generator.Writer.WriteLine("// exported values");
			exportedValues.ForEach(ev => generator.Writer.WriteLine("static {0} {1}; // exported value", ev.Type, ev.Name));
			generator.Writer.WriteLine();
			generator.Writer.WriteLine();
		}
	}
}