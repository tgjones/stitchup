using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using StitchUp.Content.Pipeline.FragmentLinking.CodeGeneration;
using StitchUp.Content.Pipeline.Graphics;

namespace StitchUp.Content.Pipeline.Processors
{
	[ContentProcessor(DisplayName = "Stitched Effect - StitchUp")]
	public class StitchedEffectProcessor : ContentProcessor<StitchedEffectContent, CompiledEffectContent>
	{
		public override CompiledEffectContent Process(StitchedEffectContent input, ContentProcessorContext context)
		{
			// Load fragments.
			IEnumerable<FragmentContent> fragments = LoadFragments(context, input.Fragments);

			// Generate effect code.
			EffectCodeGenerator codeGenerator = new EffectCodeGenerator(fragments);
			string effectCode = codeGenerator.GenerateCode();

			context.Logger.LogImportantMessage(effectCode.Replace("{", "{{").Replace("}", "}}"));

			// Process effect code.
			EffectProcessor effectProcessor = new EffectProcessor
			{
				DebugMode = EffectProcessorDebugMode.Auto,
				Defines = null
			};
			EffectContent effectContent = new EffectContent
			{
				EffectCode = effectCode,
				Identity = input.Identity,
				Name = input.Name
			};
			return effectProcessor.Process(effectContent, context);
		}

		private static IEnumerable<FragmentContent> LoadFragments(ContentProcessorContext context, IEnumerable<string> fragmentAssetNames)
		{
			List<FragmentContent> result = new List<FragmentContent>();
			foreach (string fragmentAssetName in fragmentAssetNames)
			{
				ExternalReference<FragmentContent> sourceFragment = new ExternalReference<FragmentContent>(fragmentAssetName);
				FragmentContent shaderFragment = context.BuildAndLoadAsset<FragmentContent, FragmentContent>(sourceFragment, null);
				result.Add(shaderFragment);
			}
			return result;
		}
	}
}