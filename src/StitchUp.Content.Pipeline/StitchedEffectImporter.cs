using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Content.Pipeline;
using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;
using StitchUp.Content.Pipeline.FragmentLinking.Parser;
using StitchUp.Content.Pipeline.Graphics;

namespace StitchUp.Content.Pipeline
{
	[ContentImporter(".stitchedeffect", DisplayName = "Stitched Effect - StitchUp", DefaultProcessor = "StitchedEffectProcessor")]
	public class StitchedEffectImporter : ParsingImporter<StitchedEffectContent, StitchedEffectParser>
	{
		protected override string ImporterName
		{
			get { return "Stitched Effect Importer"; }
		}

		protected override StitchedEffectContent BeginParse(string fileName, string text, ContentIdentity identity)
		{
			if (!text.StartsWith("stitchedeffect"))
			{
				string[] fragments = text.Split(new [] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);

				Dictionary<string, string> fragmentDeclarations = fragments
					.Select((f, i) => new KeyValuePair<string, string>("auto_" + i, f))
					.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

				return new StitchedEffectContent
				{
					StitchedEffectNode = new StitchedEffectNode
					{
						Name = Path.GetFileNameWithoutExtension(fileName),
						Fragments = new FragmentBlockNode
						{
							FragmentDeclarations = fragmentDeclarations.ToDictionary(kvp => kvp.Key,
								kvp => new ExternalReference<FragmentContent>(kvp.Value, identity))
						},
						Techniques = new TechniqueBlockNode
						{
							Techniques = new List<TechniqueNode>
							{
								new TechniqueNode
								{
									Name = "default_technique",
									Passes = new List<TechniquePassNode>
									{
										new TechniquePassNode
										{
											Name = "default_pass",
											Fragments = fragmentDeclarations.Keys
												.Select(k => new IdentifierToken(k, null, new BufferPosition()))
												.Cast<Token>()
												.ToList()
										}
									}
								}
							}
						}
					}
				};
			}

			return null;
		}

		protected override StitchedEffectParser GetParser(string fileName, Token[] tokens, ContentIdentity identity)
		{
			return new StitchedEffectParser(fileName, tokens, identity);
		}

		protected override StitchedEffectContent CreateContent(StitchedEffectParser parser)
		{
			StitchedEffectNode stitchedEffectNode = parser.Parse();
			return new StitchedEffectContent
			{
				StitchedEffectNode = stitchedEffectNode
			};
		}
	}
}
