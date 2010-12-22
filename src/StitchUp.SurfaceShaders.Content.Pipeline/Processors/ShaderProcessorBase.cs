using System;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using StitchUp.Content.Pipeline.Graphics;
using StitchUp.SurfaceShaders.Content.Pipeline.Shaders;

namespace StitchUp.SurfaceShaders.Content.Pipeline.Processors
{
	public abstract class ShaderProcessorBase : ContentProcessor<ShaderContent, CompiledEffectContent>
	{
		public override CompiledEffectContent Process(ShaderContent input, ContentProcessorContext context)
		{
			switch (input.ShaderNode.Type)
			{
				case ShaderFileType.SurfaceShader:
					// Build new .stitchedeffect file - this is the part that is engine-specific.
					string stitchedEffectContents = BuildStitchedEffectFile(input);

					// Save to temporary .stitchedeffect file.
					string tempStitchedEffectFile = GetTempFileName(input, ".stitchedeffect");
					File.WriteAllText(tempStitchedEffectFile, stitchedEffectContents, Encoding.GetEncoding(1252));

					// Run standard StitchUp processor.
					return context.BuildAndLoadAsset<StitchedEffectContent, CompiledEffectContent>(
						new ExternalReference<StitchedEffectContent>(tempStitchedEffectFile),
						"StitchedEffectProcessor");
				case ShaderFileType.Effect :
					// Save to temporary .fx file.
					string tempEffectFile = GetTempFileName(input, ".fx");
					File.WriteAllText(tempEffectFile, input.ShaderNode.EffectCode, Encoding.GetEncoding(1252));

					return context.BuildAndLoadAsset<EffectContent, CompiledEffectContent>(
						new ExternalReference<EffectContent>(tempEffectFile),
						"EffectProcessor");
				default :
					throw new NotSupportedException();
			}
		}

		// Use a semi-unique filename so that multiple shaders can be worked on and the resulting
		// stitchedeffect files opened simultaneously. This does mean you'll end up with several of the resulting
		// stitchedeffect files in your temp folder, but they're quite small files.
		protected string GetTempFileName(ShaderContent input, string extension)
		{
			return Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetFileName(input.Identity.SourceFilename), extension));
		}

		protected abstract string BuildStitchedEffectFile(ShaderContent input);
	}
}