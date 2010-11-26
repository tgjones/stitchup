using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline;
using StitchUp.Content.Pipeline.Graphics;

namespace StitchUp.Content.Pipeline.FragmentLinking.CodeModel
{
	public class FragmentBlockNode : ParseNode
	{
		public Dictionary<string, ExternalReference<FragmentContent>> FragmentDeclarations { get; set; }
	}
}