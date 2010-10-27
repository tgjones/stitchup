using System;
using System.ComponentModel;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using StitchUp.Content.Pipeline.Graphics;

namespace StitchUp.Content.Pipeline.Processors
{
	[ContentProcessor(DisplayName = "Model - StitchUp")]
	public class StitchedEffectModelProcessor : ModelProcessor
	{
		[Browsable(false)]
		public override MaterialProcessorDefaultEffect DefaultEffect
		{
			get { return base.DefaultEffect; }
			set { base.DefaultEffect = value; }
		}

		[DisplayName("Stitched Effect")]
		public string StitchedEffect
		{
			get;
			set;
		}

		protected override MaterialContent ConvertMaterial(MaterialContent material, ContentProcessorContext context)
		{
			if (string.IsNullOrEmpty(StitchedEffect))
				throw new Exception("Stitched Effect property must be set for StitchUp Model Processor.");

			string fullPath = Path.GetFullPath(Path.Combine(new FileInfo(material.Identity.SourceFilename).DirectoryName, StitchedEffect));
			context.AddDependency(fullPath);

			EffectMaterialContent effectMaterial = new EffectMaterialContent
			{
				CompiledEffect = context.BuildAsset<StitchedEffectContent, CompiledEffectContent>(new ExternalReference<StitchedEffectContent>(fullPath), typeof(StitchedEffectProcessor).Name),
				Identity = material.Identity,
				Name = material.Name
			};

			return effectMaterial;
		}
	}
}