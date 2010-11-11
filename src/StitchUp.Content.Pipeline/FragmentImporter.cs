using System;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using StitchUp.Content.Pipeline.FragmentLinking.Parser;
using StitchUp.Content.Pipeline.Graphics;

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

			FragmentParser parser = new FragmentParser(File.ReadAllText(filename));

			try
			{
				parser.Parse();
			}
			catch (ParserException ex)
			{
				throw new InvalidContentException(ex.Message, new ContentIdentity(info.FullName, "Fragment Importer", ex.LineNumber.ToString()), ex);
			}

			FragmentContent content = parser.Fragment;
			content.Identity = new ContentIdentity(info.FullName, "Fragment Importer");
			return content;
		}
	}
}
