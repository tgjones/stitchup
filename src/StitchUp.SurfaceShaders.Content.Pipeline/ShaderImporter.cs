using Microsoft.Xna.Framework.Content.Pipeline;
using StitchUp.Content.Pipeline;
using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;
using StitchUp.SurfaceShaders.Content.Pipeline.Shaders;

namespace StitchUp.SurfaceShaders.Content.Pipeline
{
	[ContentImporter(".shader", DisplayName = "Shader - StitchUp", DefaultProcessor = "ShaderProcessor")]
	public class ShaderImporter : ParsingImporter<ShaderContent, ShaderParser>
	{
		protected override string ImporterName
		{
			get { return "Shader Importer"; }
		}

		protected override ShaderParser GetParser(string fileName, Token[] tokens, ContentIdentity identity)
		{
			return new ShaderParser(fileName, tokens);
		}

		protected override ShaderContent CreateContent(ShaderParser parser, ContentIdentity identity)
		{
			// Parse this shader.
			ShaderNode shaderNode = parser.Parse();
			return new ShaderContent
			{
				Identity = identity,
				ShaderNode = shaderNode
			};
		}
	}
}
