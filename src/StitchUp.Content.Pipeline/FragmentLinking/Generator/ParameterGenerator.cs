using System.Linq;
using StitchUp.Content.Pipeline.FragmentLinking.EffectModel;

namespace StitchUp.Content.Pipeline.FragmentLinking.Generator
{
	public static class ParameterGenerator
	{
		public static void GenerateAllParameters(EffectCodeGenerator generator, StitchedEffectSymbol stitchedEffect)
		{
			generator.ForEachFragment(WriteParams);
		}

		private static void WriteParams(EffectCodeGenerator generator, StitchedFragmentSymbol stitchedFragment)
		{
			if (stitchedFragment.FragmentNode.Parameters == null || !stitchedFragment.FragmentNode.Parameters.VariableDeclarations.Any())
				return;

			generator.Writer.WriteLine("// {0} params", stitchedFragment.UniqueName);
			stitchedFragment.FragmentNode.Parameters.VariableDeclarations.ForEach(p => generator.Writer.WriteLine(generator.GetVariableDeclaration(stitchedFragment, p)));
			generator.Writer.WriteLine();
		}
	}
}