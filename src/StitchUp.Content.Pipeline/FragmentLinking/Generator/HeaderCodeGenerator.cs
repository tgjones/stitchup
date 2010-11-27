using System;
using System.Collections.Generic;
using StitchUp.Content.Pipeline.FragmentLinking.EffectModel;

namespace StitchUp.Content.Pipeline.FragmentLinking.Generator
{
	public static class HeaderCodeGenerator
	{
		public static void GenerateAllHeaderCode(EffectCodeGenerator generator, StitchedEffectSymbol stitchedEffect)
		{
			List<string> seenFragmentTypes = new List<string>();
			foreach (StitchedFragmentSymbol stitchedFragmentNode in stitchedEffect.StitchedFragments)
			{
				if (seenFragmentTypes.Contains(stitchedFragmentNode.FragmentNode.Name))
					continue;
				GenerateHeaderCode(generator, stitchedFragmentNode);
				seenFragmentTypes.Add(stitchedFragmentNode.FragmentNode.Name);
			}
		}

		private static void GenerateHeaderCode(EffectCodeGenerator generator, StitchedFragmentSymbol stitchedFragment)
		{
			if (stitchedFragment.FragmentNode.HeaderCode == null || string.IsNullOrEmpty(stitchedFragment.FragmentNode.HeaderCode.Code))
				return;

			generator.Writer.WriteLine("// {0} header code", stitchedFragment.UniqueName);
			generator.Writer.Write(stitchedFragment.FragmentNode.HeaderCode.Code.Replace("\r", Environment.NewLine));
			generator.Writer.WriteLine();
			generator.Writer.WriteLine();
		}
	}
}