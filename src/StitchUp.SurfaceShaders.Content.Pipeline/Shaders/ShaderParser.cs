using System;
using System.Collections.Generic;
using System.Linq;
using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;
using StitchUp.Content.Pipeline.FragmentLinking.Parser;
using StitchUp.SurfaceShaders.Content.Pipeline.Properties;

namespace StitchUp.SurfaceShaders.Content.Pipeline.Shaders
{
	public class ShaderParser : Parser
	{
		public ShaderParser(string path, Token[] tokens)
			: base(path, tokens)
		{
		}

		public ShaderNode Parse()
		{
			TokenIndex = 0;

			ShaderNode fragmentNode = ParseFragmentDeclaration();
			return fragmentNode;
		}

		private ShaderNode ParseFragmentDeclaration()
		{
			Token fileDeclarationType;
			IdentifierToken fragmentName = ParseFileDeclaration(TokenType.Identifier, out fileDeclarationType);

			ShaderFileType shaderType;
			switch (fileDeclarationType.ToString())
			{
				case "surface":
					shaderType = ShaderFileType.SurfaceShader;
					break;
				case "effect":
					shaderType = ShaderFileType.Effect;
					break;
				default:
					ReportError("Expected 'surface' or 'effect'.");
					throw new NotSupportedException();
			}

			ShaderNode shaderNode = new ShaderNode
			{
				Type = shaderType,
				Name = fragmentName.Identifier
			};

			switch (shaderType)
			{
				case ShaderFileType.SurfaceShader:
				{
					List<ParseNode> blocks = ParseBlocks();

					shaderNode.Interpolators =
						blocks.OfType<ParameterBlockNode>().FirstOrDefault(b => b.Type == ParameterBlockType.Interpolators);
					shaderNode.Parameters =
						blocks.OfType<ParameterBlockNode>().FirstOrDefault(b => b.Type == ParameterBlockType.Parameters);
					shaderNode.VertexAttributes =
						blocks.OfType<ParameterBlockNode>().FirstOrDefault(b => b.Type == ParameterBlockType.VertexAttributes);
					shaderNode.Textures = blocks.OfType<ParameterBlockNode>().FirstOrDefault(b => b.Type == ParameterBlockType.Textures);
					shaderNode.HeaderCode = blocks.OfType<HeaderCodeBlockNode>().FirstOrDefault();
					shaderNode.PixelOutputs =
						blocks.OfType<ParameterBlockNode>().FirstOrDefault(b => b.Type == ParameterBlockType.PixelOutputs);
					shaderNode.LightingModel = blocks.OfType<LightingModelBlockNode>().FirstOrDefault();
					shaderNode.VertexShaders =
						new ShaderCodeBlockNodeCollection(
							blocks.OfType<ShaderCodeBlockNode>().Where(c => c.ShaderType == ShaderType.VertexShader));
					shaderNode.PixelShaders =
						new ShaderCodeBlockNodeCollection(
							blocks.OfType<ShaderCodeBlockNode>().Where(c => c.ShaderType == ShaderType.PixelShader));

					break;
				}
				case ShaderFileType.Effect:
				{
					shaderNode.EffectCode = ParseEffectCode();
					break;
				}
			}
			return shaderNode;
		}

		private string ParseEffectCode()
		{
			ShaderCodeToken shaderCode = (ShaderCodeToken) Eat(TokenType.ShaderCode);
			return shaderCode.ShaderCode;
		}

		protected override Func<ParseNode> GetBlockParseMethod(IdentifierToken blockName)
		{
			switch (blockName.Identifier)
			{
				case "vs":
				case "ps":
					return () => ParseShaderCodeBlock(blockName);
				case "lightingmodel":
					return () => ParseLightingModelBlock(blockName);
				default:
					return base.GetBlockParseMethod(blockName);
			}
		}

		protected LightingModelBlockNode ParseLightingModelBlock(IdentifierToken blockName)
		{
			IdentifierToken lightingModel = (IdentifierToken) Eat(TokenType.Identifier);
			Eat(TokenType.CloseSquare);

			return new LightingModelBlockNode
			{
				LightingModel = lightingModel.Identifier
			};
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
				default:
					ReportError(Resources.ShaderParserParameterBlockTypeExpected, blockName.Identifier);
					throw new NotSupportedException();
			}
		}
	}
}