using System.Collections.Generic;
using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;

namespace StitchUp.Content.Pipeline.FragmentLinking.EffectModel
{
	public class TechniquePassSymbol
	{
		public string Name { get; set; }
		public List<StitchedFragmentSymbol> Fragments { get; set; }
	}
}