using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Content.Pipeline;
using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;
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
			if (stitchedEffectContent.StitchedEffectNode.Fragments == null)
				stitchedEffectContent.StitchedEffectNode.Fragments = new FragmentBlockNode
				{
					FragmentDeclarations = new Dictionary<string, ExternalReference<FragmentContent>>()
				};

			// If fragments inside technique passes were declared with literal strings, replace them
			// with identifiers, so that the rest of the code can treat them all as identifiers.
			// This should really be done as a separate pass.
			int autoIndex = 0;
			foreach (TechniqueNode techniqueNode in stitchedEffectContent.StitchedEffectNode.Techniques.Techniques)
			{
				foreach (TechniquePassNode passNode in techniqueNode.Passes)
				{
					for (int i = 0; i < passNode.Fragments.Count; ++i)
					{
						if (passNode.Fragments[i].Type == TokenType.Literal)
						{
							string autoName = "_auto_" + autoIndex++;
							stitchedEffectContent.StitchedEffectNode.Fragments.FragmentDeclarations.Add(
								autoName, new ExternalReference<FragmentContent>(((StringToken) passNode.Fragments[i]).Value, stitchedEffectContent.Identity));

							passNode.Fragments[i] = new IdentifierToken(autoName,
								passNode.Fragments[i].SourcePath,
								passNode.Fragments[i].Position);
						}
					}
				}
			}

			//System.Diagnostics.Debugger.Launch();

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
						Fragments = tpn.Fragments
							.Select(t => stitchedFragmentDictionary[((IdentifierToken) t).Identifier])
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