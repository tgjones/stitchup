using System.Collections.Generic;
using System.Linq;
using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;

namespace StitchUp.Content.Pipeline.FragmentLinking.Parser
{
	public class FragmentParser : Parser
	{
		public FragmentParser(string path, Token[] tokens)
			: base(path, tokens)
		{
		}

		public FragmentNode Parse()
		{
			TokenIndex = 0;

			FragmentNode fragmentNode = ParseFragmentDeclaration();
			return fragmentNode;
		}

		private FragmentNode ParseFragmentDeclaration()
		{
			IdentifierToken fragmentName = ParseFileDeclaration(TokenType.Fragment);

			List<ParseNode> blocks = ParseBlocks();

			return new FragmentNode
			{
				Name = fragmentName.Identifier,
				Interpolators = blocks.OfType<ParameterBlockNode>().FirstOrDefault(b => b.Type == ParameterBlockType.Interpolators),
				Parameters = blocks.OfType<ParameterBlockNode>().FirstOrDefault(b => b.Type == ParameterBlockType.Parameters),
				VertexAttributes =
					blocks.OfType<ParameterBlockNode>().FirstOrDefault(b => b.Type == ParameterBlockType.VertexAttributes),
				Textures = blocks.OfType<ParameterBlockNode>().FirstOrDefault(b => b.Type == ParameterBlockType.Textures),
				HeaderCode = blocks.OfType<HeaderCodeBlockNode>().FirstOrDefault(),
				PixelOutputs =
					blocks.OfType<ParameterBlockNode>().FirstOrDefault(b => b.Type == ParameterBlockType.PixelOutputs),
				VertexShaders =
					new ShaderCodeBlockNodeCollection(blocks.OfType<ShaderCodeBlockNode>().Where(c => c.ShaderType == ShaderType.VertexShader)),
				PixelShaders =
					new ShaderCodeBlockNodeCollection(blocks.OfType<ShaderCodeBlockNode>().Where(c => c.ShaderType == ShaderType.PixelShader))
			};
		}
	}
}