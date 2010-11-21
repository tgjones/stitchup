using System;
using System.Collections.Generic;
using System.Linq;
using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;
using StitchUp.Content.Pipeline.Properties;

namespace StitchUp.Content.Pipeline.FragmentLinking.Parser
{
	public class FragmentParser
	{
		public event ErrorEventHandler Error;

		private readonly string _path;
		private readonly Token[] _tokens;
		private int _tokenIndex;
		private BufferPosition _lastErrorPosition;

		public FragmentParser(string path, Token[] tokens)
		{
			_path = path;
			_tokens = tokens;
		}

		public FragmentNode Parse()
		{
			_tokenIndex = 0;

			FragmentNode fragmentNode = ParseFragmentDeclaration();
			return fragmentNode;
		}

		private FragmentNode ParseFragmentDeclaration()
		{
			Eat(TokenType.Fragment);

			IdentifierToken fragmentName = (IdentifierToken) Eat(TokenType.Identifier);
			Eat(TokenType.Semicolon);

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
				VertexShaders =
					new ShaderCodeBlockNodeCollection(blocks.OfType<ShaderCodeBlockNode>().Where(c => c.ShaderType == ShaderType.VertexShader)),
				PixelShaders =
					new ShaderCodeBlockNodeCollection(blocks.OfType<ShaderCodeBlockNode>().Where(c => c.ShaderType == ShaderType.PixelShader))
			};
		}

		private List<ParseNode> ParseBlocks()
		{
			List<ParseNode> result = new List<ParseNode>();

			while (PeekType() == TokenType.OpenSquare)
				result.Add(ParseBlock());

			return result;
		}

		private ParseNode ParseBlock()
		{
			Eat(TokenType.OpenSquare);

			IdentifierToken blockName = (IdentifierToken) Eat(TokenType.Identifier);
			switch (blockName.Identifier)
			{
				case "vs":
				case "ps":
					return ParseShaderCodeBlock(blockName);
				case "headercode" :
					return ParseHeaderCodeBlock();
				default:
					return ParseParameterBlock(blockName);
			}
		}

		private CodeBlockNodeBase ParseHeaderCodeBlock()
		{
			Eat(TokenType.CloseSquare);

			ShaderCodeToken shaderCode = (ShaderCodeToken)Eat(TokenType.ShaderCode);

			return new HeaderCodeBlockNode
			{
				Code = shaderCode.ShaderCode
			};
		}

		private CodeBlockNodeBase ParseShaderCodeBlock(IdentifierToken blockName)
		{
			ShaderType shaderType = GetShaderType(blockName);

            // Shader profile is optional.
            ShaderProfile? shaderProfile = null;
            if (PeekType() == TokenType.Identifier)
            {
                IdentifierToken version = (IdentifierToken) Eat(TokenType.Identifier);
                shaderProfile = GetShaderProfile(version);
            }

		    Eat(TokenType.CloseSquare);

			ShaderCodeToken shaderCode = (ShaderCodeToken) Eat(TokenType.ShaderCode);

			return new ShaderCodeBlockNode
			{
				ShaderType = shaderType,
				ShaderProfile = shaderProfile,
				Code = shaderCode.ShaderCode
			};
		}

		private static ShaderType GetShaderType(IdentifierToken blockName)
		{
			switch (blockName.Identifier)
			{
				case "vs":
					return ShaderType.VertexShader;
				case "ps" :
					return ShaderType.PixelShader;
				default :
					throw new NotSupportedException();
			}
		}

		private ShaderProfile GetShaderProfile(IdentifierToken model)
		{
			switch (model.Identifier)
			{
				case "2_0":
					return ShaderProfile.Version2_0;
				case "3_0":
					return ShaderProfile.Version3_0;
				default:
					ReportError(Resources.ParserShaderProfileNotSupported, model.Identifier);
					throw new NotSupportedException();
			}
		}

		private ParameterBlockNode ParseParameterBlock(IdentifierToken blockName)
		{
			Eat(TokenType.CloseSquare);

			ParameterBlockType blockType = GetParameterBlockType(blockName);

			bool allowInitialValue = (blockType == ParameterBlockType.Parameters);
			List<VariableDeclarationNode> variableDeclarations = ParseVariableDeclarations(allowInitialValue);
			return new ParameterBlockNode
			{
				Type = blockType,
				VariableDeclarations = variableDeclarations
			};
		}

		private ParameterBlockType GetParameterBlockType(IdentifierToken blockName)
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
				case "header":
					return ParameterBlockType.Header;
				default:
					ReportError(Resources.ParserParameterBlockTypeExpected, blockName.Identifier);
					throw new NotSupportedException();
			}
		}

		private List<VariableDeclarationNode> ParseVariableDeclarations(bool allowInitialValue)
		{
			List<VariableDeclarationNode> result = new List<VariableDeclarationNode>();

			while (PeekType() != TokenType.OpenSquare && PeekType() != TokenType.Eof)
				result.Add(ParseVariableDeclaration(allowInitialValue));

			return result;
		}

		private VariableDeclarationNode ParseVariableDeclaration(bool allowInitialValue)
		{
			Token dataType = EatDataType();
			IdentifierToken variableName = (IdentifierToken) Eat(TokenType.Identifier);

			string semantic = null;
			if (PeekType() == TokenType.Colon)
			{
				Eat(TokenType.Colon);
				semantic = ((IdentifierToken) Eat(TokenType.Identifier)).Identifier;
			}

			string initialValue = null;
			if (PeekType() == TokenType.Equal)
			{
				if (!allowInitialValue)
					ReportError(Resources.ParserInitialValueUnexpected);

				Eat(TokenType.Equal);
				while (PeekType() != TokenType.Semicolon)
					initialValue += NextToken().ToString();
			}

			Eat(TokenType.Semicolon);

			return new VariableDeclarationNode
			{
				Name = variableName.Identifier,
				DataType = dataType.Type,
				Semantic = semantic,
				InitialValue = initialValue
			};
		}

		private Token EatDataType()
		{
            if (Token.IsDataType(PeekType()))
				return NextToken();
			ReportError(Resources.ParserDataTypeExpected, PeekToken());
			return ErrorToken();
		}

		private Token Eat(TokenType type)
		{
			if (PeekType() == type)
				return NextToken();
			ReportTokenExpectedError(type);
			return ErrorToken();
		}

		private Token NextToken()
		{
			return _tokens[_tokenIndex++];
		}

		private TokenType PeekType(int index = 0)
		{
			return PeekToken(index).Type;
		}

		private Token PeekToken(int index = 0)
		{
			return _tokens[_tokenIndex + index];
		}

		private Token ErrorToken()
		{
			return new Token(TokenType.Error, _path, PeekToken().Position);
		}

		private void ReportTokenExpectedError(TokenType type)
		{
			ReportError(Resources.ParserTokenExpected, Token.GetString(type));
		}

		private void ReportUnexpectedError(TokenType type)
		{
			ReportError(Resources.ParserTokenUnexpected, Token.GetString(type));
		}

		private void ReportError(string message, params object[] args)
		{
			ReportError(message, PeekToken(), args);
		}

		private void ReportError(string message, Token token, params object[] args)
		{
			BufferPosition position = token.Position;
			if (Error != null && _lastErrorPosition != position)
				Error(this, new ErrorEventArgs(string.Format(message, args), position));
			_lastErrorPosition = position;
		}
	}
}