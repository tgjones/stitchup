using System.Linq;
using StitchUp.Content.Pipeline.FragmentLinking.EffectModel;

namespace StitchUp.Content.Pipeline.FragmentLinking.Generator
{
	public static class SamplerGenerator
	{
		public static void GenerateAllSamplers(EffectCodeGenerator generator, StitchedEffectSymbol stitchedEffect)
		{
			generator.ForEachFragment(WriteSamplers);
		}

		private static void WriteSamplers(EffectCodeGenerator generator, StitchedFragmentSymbol stitchedFragment)
		{
			if (stitchedFragment.FragmentNode.Textures == null || !stitchedFragment.FragmentNode.Textures.VariableDeclarations.Any())
				return;

			generator.Writer.WriteLine("// {0} textures", stitchedFragment.UniqueName);
			stitchedFragment.FragmentNode.Textures.VariableDeclarations.ForEach(t =>
			{
				generator.Writer.WriteLine(generator.GetVariableDeclaration(stitchedFragment, t));
				generator.Writer.WriteLine("sampler {0}_{1}_sampler = sampler_state {{ Texture = ({0}_{1}); }};",
					stitchedFragment.UniqueName, t.Name);
			});
			generator.Writer.WriteLine();
		}
	}
}