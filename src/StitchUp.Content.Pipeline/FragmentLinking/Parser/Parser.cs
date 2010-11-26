using System;
using System.Collections.Generic;
using System.Linq;
using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;
using StitchUp.Content.Pipeline.Properties;

namespace StitchUp.Content.Pipeline.FragmentLinking.Parser
{
	public abstract class Parser
	{
		public event ErrorEventHandler Error;

		private readonly string _path;
		private readonly Token[] _tokens;
		private BufferPosition _lastErrorPosition;

		protected int TokenIndex { get; set; }

		protected Parser(string path, Token[] tokens)
		{
			_path = path;
			_tokens = tokens;
		}

		protected IdentifierToken ParseFileDeclaration(TokenType fileDeclarationType)
		{
			Eat(fileDeclarationType);

			IdentifierToken name = (IdentifierToken)Eat(TokenType.Identifier);
			Eat(TokenType.Semicolon);

			return name;
		}

		protected List<ParseNode> ParseBlocks()
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
			return GetBlockParseMethod(blockName)();
		}

		protected virtual Func<ParseNode> GetBlockParseMethod(IdentifierToken blockName)
		{
			switch (blockName.Identifier)
			{
				case "headercode":
					return () => ParseHeaderCodeBlock();
				default:
					return () => ParseParameterBlock(blockName);
			}
		}

		protected CodeBlockNodeBase ParseHeaderCodeBlock()
		{
			Eat(TokenType.CloseSquare);

			ShaderCodeToken shaderCode = (ShaderCodeToken)Eat(TokenType.ShaderCode);

			return new HeaderCodeBlockNode
			{
				Code = shaderCode.ShaderCode
			};
		}

		protected CodeBlockNodeBase ParseShaderCodeBlock(IdentifierToken blockName)
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

		protected ParameterBlockNode ParseParameterBlock(IdentifierToken blockName)
		{
			Eat(TokenType.CloseSquare);

			ParameterBlockType blockType = GetParameterBlockType(blockName);

			bool allowInitialValue = (blockType == ParameterBlockType.Parameters);
			List<VariableDeclarationNode> variableDeclarations = ParseVariableDeclarations(allowInitialValue, false, true, true, null);
			return new ParameterBlockNode
			{
				Type = blockType,
				VariableDeclarations = variableDeclarations
			};
		}

		protected abstract ParameterBlockType GetParameterBlockType(IdentifierToken blockName);

		protected List<VariableDeclarationNode> ParseVariableDeclarations(bool allowInitialValue, bool requireInitialValue,
			bool allowArray, bool allowSemantic,
			TokenType? requiredDataType)
		{
			List<VariableDeclarationNode> result = new List<VariableDeclarationNode>();

			while (PeekType() != TokenType.OpenSquare && PeekType() != TokenType.Eof)
				result.Add(ParseVariableDeclaration(allowInitialValue, requireInitialValue, allowArray, allowSemantic, requiredDataType));

			return result;
		}

		private VariableDeclarationNode ParseVariableDeclaration(bool allowInitialValue, bool requireInitialValue,
			bool allowArray, bool allowSemantic,
			TokenType? requiredDataType)
		{
			Token dataType = (requiredDataType != null) ? Eat(requiredDataType.Value) : EatDataType();
			IdentifierToken variableName = (IdentifierToken) Eat(TokenType.Identifier);

			bool isArray = false;
			Token arraySize = null;
			if (allowArray && PeekType() == TokenType.OpenSquare)
			{
				isArray = true;

				Eat(TokenType.OpenSquare);
				switch (PeekType())
				{
					case TokenType.Literal:
						LiteralToken arraySizeToken = (LiteralToken) Eat(TokenType.Literal);
						if (arraySizeToken.LiteralType != LiteralTokenType.Int || ((IntToken) arraySizeToken).Value < 1)
						{
							ReportError(Resources.ParserArrayIndexExpected);
							throw new NotSupportedException();
						}
						arraySize = arraySizeToken;
						break;
					case TokenType.Identifier:
						arraySize = Eat(TokenType.Identifier);
						break;
					default:
						ReportError(Resources.ParserArrayIndexExpected);
						throw new NotSupportedException();
				}

				Eat(TokenType.CloseSquare);
			}

			string semantic = null;
			if (allowSemantic && PeekType() == TokenType.Colon)
			{
				Eat(TokenType.Colon);
				semantic = ((IdentifierToken) Eat(TokenType.Identifier)).Identifier;
			}

			string initialValue = null;
			if (requireInitialValue || PeekType() == TokenType.Equal)
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
				DataType = dataType.Type,
				Name = variableName.Identifier,
				IsArray = isArray,
				ArraySize = arraySize,
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

		protected Token Eat(TokenType type)
		{
			if (PeekType() == type)
				return NextToken();
			ReportTokenExpectedError(type);
			return ErrorToken();
		}

		private Token NextToken()
		{
			return _tokens[TokenIndex++];
		}

		private TokenType PeekType(int index = 0)
		{
			return PeekToken(index).Type;
		}

		private Token PeekToken(int index = 0)
		{
			return _tokens[TokenIndex + index];
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

		protected void ReportError(string message, params object[] args)
		{
			ReportError(message, PeekToken(), args);
		}

		protected void ReportError(string message, Token token, params object[] args)
		{
			BufferPosition position = token.Position;
			if (Error != null && _lastErrorPosition != position)
				Error(this, new ErrorEventArgs(string.Format(message, args), position));
			_lastErrorPosition = position;
		}
	}
}