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
			IEnumerable<FragmentContent> fragments = LoadFragments(context, input.Fragments);

			// Load into intermediate objects which keep track of each fragment's unique name.
			List<StitchedFragmentNode> stitchedFragments = fragments
				.Select((f, i) => new StitchedFragmentNode(f.FragmentNode.Name + i, f.FragmentNode))
				.ToList();

			// Load into intermediate object, which keeps track of each 
			StitchedEffectNode stitchedEffect = new StitchedEffectNode(stitchedFragments);

			// Pre-process stitched effect - this replaces the export / import calls with their expansions.
			StitchedEffectPreProcessor preProcessor = new StitchedEffectPreProcessor();
			preProcessor.PreProcess(stitchedEffect);

			// Find out which shader model to compile for.
			ShaderModel shaderModel = GetTargetShaderModel(stitchedEffect);

			// Generate effect code.
			EffectCodeGenerator codeGenerator = new EffectCodeGenerator(stitchedEffect, shaderModel);
			string effectCode = codeGenerator.GenerateCode();

			// Save effect code so that if there are errors, we'll be able to view the generated .fx file.
			context.Logger.LogImportantMessage(effectCode.Replace("{", "{{").Replace("}", "}}"));
			string tempEffectFile = Path.Combine(Path.GetTempPath(), "StitchedEffect.fx");
			File.WriteAllText(tempEffectFile, effectCode, Encoding.GetEncoding(1252));

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

			return effectProcessor.Process(effectContent, context);
		}

		private static ShaderModel GetTargetShaderModel(StitchedEffectNode stitchedEffect)
		{
			foreach (ShaderModel shaderModel in Enum.GetValues(typeof(ShaderModel)))
				if (stitchedEffect.CanBeCompiledForShaderModel(shaderModel))
					return shaderModel;
			throw new InvalidContentException("Could not find shader model compatible with this stitched effect.");
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