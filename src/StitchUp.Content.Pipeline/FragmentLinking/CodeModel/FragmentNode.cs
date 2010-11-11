using System.Collections.Generic;

namespace StitchUp.Content.Pipeline.FragmentLinking.CodeModel
{
	public abstract class ParseNode
	{
		
	}

	public class FragmentNode : ParseNode
	{
		public string Name { get; private set; }

		public List<BlockNode> Blocks { get; private set; }

		public FragmentNode(string name, List<BlockNode> blocks)
		{
			Name = name;
			Blocks = blocks;
		}
	}

	public class BlockNode : ParseNode
	{
		public string Name { get; private set; }
		public List<VariableDeclarationNode> VariableDeclarations { get; private set; }

		public BlockNode(string name, List<VariableDeclarationNode> variableDeclarations)
		{
			Name = name;
			VariableDeclarations = variableDeclarations;
		}
	}

	public class VariableDeclarationNode : ParseNode
	{
		public TokenType DataType { get; private set; }
		public string Name { get; private set; }

		public VariableDeclarationNode(string name, TokenType dataType)
		{
			Name = name;
			DataType = dataType;
		}
	}
}