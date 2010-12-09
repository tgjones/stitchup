using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework.Content.Pipeline;
using StitchUp.Content.Pipeline.Graphics;

namespace StitchUp.Content.Pipeline.FragmentLinking.CodeModel
{
	public abstract class FragmentSource
	{
		public abstract FragmentContent LoadFragmentContent(ContentProcessorContext context);

		protected FragmentContent LoadFragmentContent(ContentProcessorContext context, string fileName, ContentIdentity relativeToContent = null)
		{
			ExternalReference<FragmentContent> externalReference = (relativeToContent != null)
				? new ExternalReference<FragmentContent>(fileName, relativeToContent)
				: new ExternalReference<FragmentContent>(fileName);
			return context.BuildAndLoadAsset<FragmentContent, FragmentContent>(externalReference, null);
		}
	}

	public class FileFragmentSource : FragmentSource
	{
		private readonly string _fileName;
		private readonly ContentIdentity _relativeToContent;

		public FileFragmentSource(string fileName, ContentIdentity relativeToContent)
		{
			_fileName = fileName;
			_relativeToContent = relativeToContent;
		}

		public override FragmentContent LoadFragmentContent(ContentProcessorContext context)
		{
			return LoadFragmentContent(context, _fileName, _relativeToContent);
		}
	}

	public class EmbeddedFragmentSource : FragmentSource
	{
		private readonly string _resourceName;

		public EmbeddedFragmentSource(string resourceName)
		{
			_resourceName = resourceName;
		}

		public override FragmentContent LoadFragmentContent(ContentProcessorContext context)
		{
			// Save embedded resource as temporary file.
			string temporaryFileName = Path.Combine(Path.GetTempPath(), _resourceName);
			Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(StitchedEffectImporter), _resourceName);
			if (stream == null)
				throw new InvalidContentException("Could not find library fragment with name '" + _resourceName + "'.");
			StreamReader streamReader = new StreamReader(stream);
			File.WriteAllText(temporaryFileName, streamReader.ReadToEnd());

			try
			{
				return LoadFragmentContent(context, temporaryFileName);
			}
			finally
			{
				File.Delete(temporaryFileName);
			}
		}
	}
}