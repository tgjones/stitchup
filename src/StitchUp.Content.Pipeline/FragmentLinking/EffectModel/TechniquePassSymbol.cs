using System;
using System.Collections.Generic;
using StitchUp.Content.Pipeline.FragmentLinking.Generator;

namespace StitchUp.Content.Pipeline.FragmentLinking.EffectModel
{
	public class TechniquePassSymbol
	{
		public TechniqueSymbol Technique { get; set; }
		public string Name { get; set; }
		public List<StitchedFragmentSymbol> Fragments { get; set; }

		internal void ForEachFragment(EffectCodeGenerator generator, Action<EffectCodeGenerator, StitchedFragmentSymbol, string> action)
		{
			foreach (StitchedFragmentSymbol stitchedFragmentNode in Fragments)
				action(generator, stitchedFragmentNode, string.Format("{0}_{1}", Technique.Name, Name));
		}
	}
}