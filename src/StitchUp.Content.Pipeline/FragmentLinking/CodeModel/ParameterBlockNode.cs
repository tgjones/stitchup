using System.Collections.Generic;

namespace StitchUp.Content.Pipeline.FragmentLinking.CodeModel
{
	public class ParameterBlockNode : ParseNode
	{
		public ParameterBlockType Type { get; set; }
		public List<VariableDeclarationNode> VariableDeclarations { get; set; }
	}
}