using System.Collections.Generic;

namespace StitchUp.Content.Pipeline.FragmentLinking.CodeModel
{
	public class TechniquePassNode : ParseNode
	{
		public string Name { get; set; }
		public List<string> FragmentIdentifiers { get; set; }
	}
}