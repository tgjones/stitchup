using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using StitchUp.Content.Pipeline.FragmentLinking.CodeGeneration;
using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;
using StitchUp.Content.Pipeline.FragmentLinking.PreProcessor;
using StitchUp.Content.Pipeline.Graphics;

namespace StitchUp.Content.Pipeline.Processors
{
	[ContentProcessor(DisplayName = "Stitched Effect - StitchUp")]
	public class StitchedEffectProcessor : ContentProcessor<StitchedEffectContent, CompiledEffectContent>
	{
		public override CompiledEffectContent Process(StitchedEffectContent input, ContentProcessorContext context)
		{
			// Load fragments.
			IEnumerable<FragmentContent> fragments = input.Fragments.Select(f =>
                context.BuildAndLoadAsset<FragmentContent, FragmentContent>(f, null));

			// Load into intermediate objects which keep track of each fragment's unique name.
			List<StitchedFragmentNode> stitchedFragments = fragments
				.Select((f, i) => new StitchedFragmentNode(f.FragmentNode.Name + i, f.FragmentNode))
				.ToList();

			// Load into intermediate object, which keeps track of each 
			StitchedEffectNode stitchedEffect = new StitchedEffectNode(stitchedFragments);

			// Pre-process stitched effect - this replaces the export / import calls with their expansions.
			StitchedEffectPreProcessor preProcessor = new StitchedEffectPreProcessor();
			preProcessor.PreProcess(stitchedEffect);

			// Find out which shader profile to attempt to compile for first.
            ShaderProfile minimumShaderProfile = GetMinimumTargetShaderProfile(stitchedEffect);

            foreach (ShaderProfile shaderProfile in Enum.GetValues(typeof(ShaderProfile)))
            {
                if (shaderProfile < minimumShaderProfile)
                    continue;

                CompiledEffectContent compiledEffect;
                if (AttemptEffectCompilation(context, input, stitchedEffect, shaderProfile, out compiledEffect))
                    return compiledEffect;
            }

		    throw new InvalidContentException(
                "Could not find a shader profile compatible with this stitched effect.",
                input.Identity);
		}

        private static bool AttemptEffectCompilation(
            ContentProcessorContext context, StitchedEffectContent input,
            StitchedEffectNode stitchedEffect, ShaderProfile shaderProfile,
            out CompiledEffectContent compiledEffect)
        {
            // Generate effect code.
            EffectCodeGenerator codeGenerator = new EffectCodeGenerator(stitchedEffect, shaderProfile);
            string effectCode = codeGenerator.GenerateCode();

            // Save effect code so that if there are errors, we'll be able to view the generated .fx file.
            string tempEffectFile = GetTempEffectFileName(input);
            File.WriteAllText(tempEffectFile, effectCode, Encoding.GetEncoding(1252));
            context.Logger.LogImportantMessage(string.Format("{0} :	Stitched effect generated (double-click this message to view).", tempEffectFile));

            // Process effect code.
            EffectProcessor effectProcessor = new EffectProcessor
            {
                DebugMode = EffectProcessorDebugMode.Auto,
                Defines = null
            };

            EffectContent effectContent = new EffectContent
            {
                EffectCode = effectCode,
                Identity = new ContentIdentity(tempEffectFile),
                Name = input.Name
            };

            try
            {
                compiledEffect = effectProcessor.Process(effectContent, context);
                return true;
            }
            catch (InvalidContentException ex)
            {
                if (ex.Message.Contains("error X5608") || ex.Message.Contains("error X5609"))
                {
                    compiledEffect = null;
                    return false;
                }
                throw;
            }
        }

		// Use a semi-unique filename so that multiple stitched effects can be worked on and the resulting
		// effect files opened simultaneously. This does mean you'll end up with several of the resulting
		// effect files in your temp folder, but they're quite small files.
		private static string GetTempEffectFileName(StitchedEffectContent input)
		{
			return Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetFileName(input.Identity.SourceFilename), ".fx"));
		}

		private static ShaderProfile GetMinimumTargetShaderProfile(StitchedEffectNode stitchedEffect)
		{
			foreach (ShaderProfile shaderProfile in Enum.GetValues(typeof(ShaderProfile)))
				if (stitchedEffect.CanBeCompiledForShaderProfile(shaderProfile))
					return shaderProfile;
			throw new InvalidContentException("Could not find shader profile compatible with this stitched effect.");
		}
	}
}