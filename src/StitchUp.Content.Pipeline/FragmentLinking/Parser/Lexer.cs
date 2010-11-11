using System;
using System.Collections.Generic;
using System.Text;
using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;
using StitchUp.Content.Pipeline.Properties;

namespace StitchUp.Content.Pipeline.FragmentLinking.Parser
{
	public class Lexer
	{
		public event ErrorEventHandler Error;

		private readonly string _path;
		private readonly TextBuffer _buffer;
		private BufferPosition _position;

		// Temporary variables used while lexing.
		private readonly StringBuilder _value;

		public bool IsEof
		{
			get { return _buffer.IsEof; }
		}

		public Lexer(string path, string text)
		{
			_path = path;
			_buffer = new TextBuffer(text);
			_value = new StringBuilder();
		}

		public Token[] GetTokens()
		{
			List<Token> result = new List<Token>();
			while (!IsEof)
				result.Add(NextToken());
			result.Add(NextToken()); // We want EOF as a token.
			return result.ToArray();
		}

		public Token NextToken()
		{
			EatWhiteSpace();
			StartToken();

			if (PeekChar() == '\0')
				return NewToken(TokenType.Eof);

			char c = NextChar();
			switch (c)
			{
				case '{' :
					return NewToken(TokenType.OpenCurly);
				case '}':
					return NewToken(TokenType.CloseCurly);
				case '[':
					return NewToken(TokenType.OpenSquare);
				case ']':
					return NewToken(TokenType.CloseSquare);
				case '=':
					return NewToken(TokenType.Equal);
				case ',':
					return NewToken(TokenType.Comma);
				case ':':
					return NewToken(TokenType.Colon);
				case ';':
					return NewToken(TokenType.Semicolon);
				case '"':
				{
					string value = EatWhile(c2 => c2 != '"');
					NextChar(); //Swallow the end of the string constant
					return new StringToken(value, _path, TakePosition());
				}
				default :
					const string hlslDelimiter = "__hlsl__";
					if (c == hlslDelimiter[0] && PeekIdentifier(hlslDelimiter, 1))
					{
						EatIdentifier(hlslDelimiter, 1);

						string shaderCode = EatWhile(c2 => !IsEof && !PeekIdentifier(hlslDelimiter, 0));
						ShaderCodeToken codeToken = new ShaderCodeToken(shaderCode.Trim(), _path, TakePosition());

						EatIdentifier(hlslDelimiter, 0);

						return codeToken;
					}
					if (IsIdentifierChar(c))
					{
						string identifier = c + EatWhile(IsIdentifierChar);
						if (Keywords.IsKeyword(identifier))
							return new Token(Keywords.GetKeywordType(identifier), _path, TakePosition());
						return new IdentifierToken(identifier, _path, TakePosition());
					}
					ReportError(Resources.LexerUnexpectedCharacter, c);
					return ErrorToken();
			}
		}

		private bool PeekIdentifier(string identifier, int startIndex)
		{
			int index = 0;
			for (int i = startIndex; i < identifier.Length; ++i)
				if (PeekChar(index++) != identifier[i])
					return false;
			return true;
		}

		private void EatIdentifier(string identifier, int startIndex)
		{
			for (int i = startIndex; i < identifier.Length; ++i)
				NextChar();
		}

		private static bool IsIdentifierChar(char c)
		{
			if (!char.IsLetterOrDigit(c))
				return (c == '_');
			return true;
		}

		private string EatWhile(Func<char, bool> test)
		{
			_value.Length = 0;
			while (!IsEof && test(PeekChar()))
				_value.Append(NextChar());
			return _value.ToString();
		}

		private void ReportError(string message, params object[] args)
		{
			if (Error != null)
				Error(this, new ErrorEventArgs(string.Format(message, args), _buffer.Position));
		}

		private Token ErrorToken()
		{
			return NewToken(TokenType.Error);
		}

		public static bool IsLineSeparator(char ch)
		{
			switch (ch)
			{
				case '\u2028':
				case '\u2029':
				case '\n':
				case '\r':
					return true;
			}
			return false;
		}

		private static bool IsWhiteSpace(char c)
		{
			return char.IsWhiteSpace(c);
		}

		private void EatWhiteSpace()
		{
			while (IsWhiteSpace(PeekChar()))
				NextChar();
		}

		private char NextChar()
		{
			return _buffer.NextChar();
		}

		private char PeekChar(int index = 0)
		{
			return _buffer.PeekChar(index);
		}

		private void StartToken()
		{
			_position = _buffer.Position;
		}

		private Token NewToken(TokenType type)
		{
			return new Token(type, _path, TakePosition());
		}

		private BufferPosition TakePosition()
		{
			BufferPosition position = _position;
			ClearPosition();
			return position;
		}

		private void ClearPosition()
		{
			_position = new BufferPosition();
		}
	}
}