using System.Collections.Generic;
using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;
using StitchUp.Content.Pipeline.Properties;

namespace StitchUp.Content.Pipeline.FragmentLinking.Parser
{
	public class Parser
	{
		public event ErrorEventHandler Error;

		private readonly string _path;
		private readonly Token[] _tokens;
		private int _tokenIndex;
		private BufferPosition _lastErrorPosition;

		public Parser(string path, Token[] tokens)
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

			List<BlockNode> blocks = ParseBlocks();

			return new FragmentNode(fragmentName.Identifier, blocks);
		}

		private List<BlockNode> ParseBlocks()
		{
			List<BlockNode> result = new List<BlockNode>();

			while (PeekType() == TokenType.OpenSquare)
				result.Add(ParseBlock());

			return result;
		}

		private BlockNode ParseBlock()
		{
			Eat(TokenType.OpenSquare);
			IdentifierToken blockName = (IdentifierToken)Eat(TokenType.Identifier);
			Eat(TokenType.CloseSquare);

			List<VariableDeclarationNode> variableDeclarations = ParseVariableDeclarations();

			return new BlockNode(blockName.Identifier, variableDeclarations);
		}

		private List<VariableDeclarationNode> ParseVariableDeclarations()
		{
			List<VariableDeclarationNode> result = new List<VariableDeclarationNode>();

			while (PeekType() != TokenType.OpenSquare && PeekType() != TokenType.Eof)
				result.Add(ParseVariableDeclaration());

			return result;
		}

		private VariableDeclarationNode ParseVariableDeclaration()
		{
			Token dataType = EatDataType();
			IdentifierToken variableName = (IdentifierToken) Eat(TokenType.Identifier);
			Eat(TokenType.Semicolon);

			return new VariableDeclarationNode(variableName.Identifier, dataType.Type);
		}

		private Token EatDataType()
		{
			switch (PeekType())
			{
				case TokenType.Bool :
				case TokenType.Float :
				case TokenType.Float2 :
				case TokenType.Float3 :
				case TokenType.Float4 :
				case TokenType.Matrix :
				case TokenType.Texture2D:
					return NextToken();
			}
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

		private TokenType PeekType()
		{
			return PeekToken().Type;
		}

		private Token PeekToken()
		{
			return this.PeekToken(0);
		}

		private Token PeekToken(int index)
		{
			return _tokens[_tokenIndex + index];
		}

		private Token ErrorToken()
		{
			return new Token(TokenType.Error, this._path, this.PeekToken().Position);
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