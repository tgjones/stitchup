using System.Collections.Generic;

namespace StitchUp.Content.Pipeline.FragmentLinking.EffectModel
{
	public class TechniqueSymbol
	{
		public string Name { get; set; }
		public List<TechniquePassSymbol> Passes { get; set; }
	}
}