using System.Collections.Generic;

namespace StitchUp.Content.Pipeline.FragmentLinking.CodeModel
{
	public class TechniqueBlockNode : ParseNode
	{
		public List<TechniqueNode> Techniques { get; set; }
	}
}