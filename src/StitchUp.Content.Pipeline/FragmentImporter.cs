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
	public class FragmentImporter : ContentImporter<FragmentContent>
	{
		public override FragmentContent Import(string filename, ContentImporterContext context)
		{
			if (context == null)
				throw new ArgumentNullException("context");
			if (string.IsNullOrEmpty(filename))
				throw new ArgumentException("Filename cannot be null or empty.", "filename");
			FileInfo info = new FileInfo(filename);
			if (!info.Exists)
				throw new FileNotFoundException("File not found", filename);

			Lexer lexer = new Lexer(filename, File.ReadAllText(filename));
			lexer.Error += (sender, e) => ThrowParserException(e, info);
			Token[] tokens = lexer.GetTokens();

			FragmentParser parser = new FragmentParser(filename, tokens);
			parser.Error += (sender, e) => ThrowParserException(e, info);
			FragmentNode fragmentNode = parser.Parse();

			FragmentContent content = new FragmentContent { FragmentNode = fragmentNode };
			content.Identity = new ContentIdentity(info.FullName, "Fragment Importer");
			return content;
		}

		private static void ThrowParserException(ErrorEventArgs e, FileInfo info)
		{
			string identifier = string.Format("{0},{1}", e.Position.Line + 1, e.Position.Column + 1);
			ContentIdentity contentIdentity = new ContentIdentity(info.FullName, "Fragment Importer", identifier);
			throw new InvalidContentException(e.Message, contentIdentity);
		}
	}
}
