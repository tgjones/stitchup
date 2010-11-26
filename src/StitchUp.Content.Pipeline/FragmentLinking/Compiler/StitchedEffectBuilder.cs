using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Content.Pipeline;
using StitchUp.Content.Pipeline.FragmentLinking.EffectModel;
using StitchUp.Content.Pipeline.Graphics;

namespace StitchUp.Content.Pipeline.FragmentLinking.Compiler
{
	public static class StitchedEffectBuilder
	{
		public static StitchedEffectSymbol BuildStitchedEffect(
			StitchedEffectContent stitchedEffectContent,
			ContentProcessorContext context)
		{
			// Load fragments.
			Dictionary<string, FragmentContent> fragmentDictionary = stitchedEffectContent.StitchedEffectNode.Fragments.FragmentDeclarations.ToDictionary(fd => fd.Key,
				fd => context.BuildAndLoadAsset<FragmentContent, FragmentContent>(fd.Value, null));

			// Load into intermediate objects which keep track of each fragment's unique name.
			Dictionary<string, StitchedFragmentSymbol> stitchedFragmentDictionary = fragmentDictionary
				.Select((f, i) => new KeyValuePair<string, StitchedFragmentSymbol>(f.Key, new StitchedFragmentSymbol(f.Value.FragmentNode.Name + i, f.Value.FragmentNode)))
				.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			// Load into techniques.
			List<TechniqueSymbol> techniques = stitchedEffectContent.StitchedEffectNode.Techniques.Techniques
				.Select(tn => new TechniqueSymbol
				{
					Name = tn.Name,
					Passes = tn.Passes.Select(tpn => new TechniquePassSymbol
					{
						Name = tpn.Name,
						Fragments = stitchedFragmentDictionary
							.Where(kvp => tpn.FragmentIdentifiers.Contains(kvp.Key))
							.Select(kvp => kvp.Value)
							.ToList()
					}).ToList()
				}).ToList();

			return new StitchedEffectSymbol
			{
				StitchedFragments = stitchedFragmentDictionary.Values.ToList(),
				Techniques = techniques
			};
		}
	}
}