using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace StitchUp.Content.Pipeline.Graphics
{
	public class FragmentContent : ContentItem
	{
		public string InterfaceName { get; set; }

		public Dictionary<string, FragmentParameterContent> InterfaceParameterMetadata { get; private set; }
		public List<string> InterfaceParams { get; private set; }
		public List<string> InterfaceTextures { get; private set; }
		public List<string> InterfaceVertex { get; private set; }
		public List<string> InterfaceInterpolators { get; private set; }

		public List<FragmentCodeContent> CodeBlocks { get; private set; }

		public FragmentContent()
		{
			InterfaceParameterMetadata = new Dictionary<string, FragmentParameterContent>();
			InterfaceParams = new List<string>();
			InterfaceTextures = new List<string>();
			InterfaceVertex = new List<string>();
			InterfaceInterpolators = new List<string>();

			CodeBlocks = new List<FragmentCodeContent>();
		}
	}
}