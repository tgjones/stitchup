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
