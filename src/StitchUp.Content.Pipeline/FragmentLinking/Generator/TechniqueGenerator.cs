using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;
using StitchUp.Content.Pipeline.FragmentLinking.EffectModel;

namespace StitchUp.Content.Pipeline.FragmentLinking.Generator
{
	public static class TechniqueGenerator
	{
		public static void GenerateAllTechniques(EffectCodeGenerator generator, StitchedEffectSymbol stitchedEffect)
		{
			foreach (TechniqueSymbol technique in stitchedEffect.Techniques)
				GenerateTechnique(generator, technique);
		}

		private static void GenerateTechnique(EffectCodeGenerator generator, TechniqueSymbol technique)
		{
			ScriptTextWriter writer = generator.Writer;
			writer.WriteLine("// -------- technique {0} --------", technique.Name);
			writer.WriteLine("technique {0}", technique.Name);
			writer.WriteLine("{");

			foreach (TechniquePassSymbol pass in technique.Passes)
				GeneratePass(generator, technique, pass);

			writer.WriteLine("};");
		}

		private static void GeneratePass(EffectCodeGenerator generator, TechniqueSymbol technique, TechniquePassSymbol pass)
		{
			ScriptTextWriter writer = generator.Writer;
			writer.WriteLine("\tpass {0}", pass.Name);
			writer.WriteLine("\t{");
			writer.WriteLine("\t\tVertexShader = compile vs_{0} {1}_{2}_vs();",
				generator.Context.TargetShaderProfile.GetDescription(),
				technique.Name, pass.Name);
			writer.WriteLine("\t\tPixelShader = compile ps_{0} {1}_{2}_ps();",
				generator.Context.TargetShaderProfile.GetDescription(),
				technique.Name, pass.Name);
			writer.WriteLine("\t}");
		}
	}
}