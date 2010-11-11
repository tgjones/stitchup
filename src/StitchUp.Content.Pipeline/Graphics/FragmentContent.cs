using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;

namespace StitchUp.Content.Pipeline.Graphics
{
	public class FragmentContent : ContentItem
	{
		public FragmentNode FragmentNode { get; set; }
	}
}