using System;
using System.Collections.Generic;
using System.Linq;
using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;
using StitchUp.Content.Pipeline.Properties;

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

		protected override Func<ParseNode> GetBlockParseMethod(IdentifierToken blockName)
		{
			switch (blockName.Identifier)
			{
				case "vs":
				case "ps":
					return () => ParseShaderCodeBlock(blockName);
				default:
					return base.GetBlockParseMethod(blockName);
			}
		}

		protected override ParameterBlockType GetParameterBlockType(IdentifierToken blockName)
		{
			switch (blockName.Identifier)
			{
				case "interpolators":
					return ParameterBlockType.Interpolators;
				case "parameters":
				case "params":
					return ParameterBlockType.Parameters;
				case "textures":
					return ParameterBlockType.Textures;
				case "vertexattributes":
				case "vertex":
					return ParameterBlockType.VertexAttributes;
				case "pixeloutputs":
					return ParameterBlockType.PixelOutputs;
				default:
					ReportError(Resources.FragmentParserParameterBlockTypeExpected, blockName.Identifier);
					throw new NotSupportedException();
			}
		}
	}
}