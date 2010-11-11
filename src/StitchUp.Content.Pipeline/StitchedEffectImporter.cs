using System;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using StitchUp.Content.Pipeline.Graphics;

namespace StitchUp.Content.Pipeline
{
	[ContentImporter(".stitchedeffect", DisplayName = "Stitched Effect - StitchUp", DefaultProcessor = "StitchedEffectProcessor")]
	public class StitchedEffectImporter : ContentImporter<StitchedEffectContent>
	{
		public override StitchedEffectContent Import(string filename, ContentImporterContext context)
		{
			if (context == null)
				throw new ArgumentNullException("context");
			if (string.IsNullOrEmpty(filename))
				throw new ArgumentException("Filename cannot be null or empty.", "filename");
			FileInfo info = new FileInfo(filename);
			if (!info.Exists)
				throw new FileNotFoundException("File not found", filename);

			StitchedEffectContent content = new StitchedEffectContent();
			content.Identity = new ContentIdentity(info.FullName, "Effect Importer");
			content.Fragments.AddRange(File.ReadLines(filename));
			return content;
		}
	}
}
