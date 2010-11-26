using System;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;
using StitchUp.Content.Pipeline.FragmentLinking.Parser;
using StitchUp.Content.Pipeline.Graphics;
using ErrorEventArgs = StitchUp.Content.Pipeline.FragmentLinking.Parser.ErrorEventArgs;

namespace StitchUp.Content.Pipeline
{
	[ContentImporter(".fragment", DisplayName = "Fragment Importer - StitchUp", DefaultProcessor = null)]
	public class FragmentImporter : ParsingImporter<FragmentContent, FragmentParser>
	{
		protected override string ImporterName
		{
			get { return "Fragment Importer"; }
		}

		protected override FragmentParser GetParser(string fileName, Token[] tokens, ContentIdentity identity)
		{
			return new FragmentParser(fileName, tokens);
		}

		protected override FragmentContent CreateContent(FragmentParser parser)
		{
			FragmentNode fragmentNode = parser.Parse();
			return new FragmentContent
			{
				FragmentNode = fragmentNode
			};
		}
	}
}
