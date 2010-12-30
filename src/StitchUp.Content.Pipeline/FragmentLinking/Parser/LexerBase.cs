using System;
using System.Collections.Generic;
using System.Text;
using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;
using StitchUp.Content.Pipeline.Properties;

namespace StitchUp.Content.Pipeline.FragmentLinking.Parser
{
	public abstract class LexerBase
	{
		public const string HlslDelimiter = "__hlsl__";

		public event ErrorEventHandler Error;

		private readonly string _path;
		private readonly TextBuffer _buffer;
		private BufferPosition _position;

		// Temporary variables used while lexing.
		private readonly StringBuilder _value;

		protected string Path
		{
			get { return _path; }
		}

		protected StringBuilder Value
		{
			get { return _value; }
		}

		public bool IsEof
		{
			get { return _buffer.IsEof; }
		}

		protected LexerBase(string path, string text)
		{
			_path = path;
			_buffer = new TextBuffer(text);
			_value = new StringBuilder();
		}

		protected bool PeekIdentifier(string identifier, int startIndex)
		{
			int index = 0;
			for (int i = startIndex; i < identifier.Length; ++i)
				if (PeekChar(index++) != identifier[i])
					return false;
			return true;
		}

		protected void EatIdentifier(string identifier, int startIndex)
		{
			for (int i = startIndex; i < identifier.Length; ++i)
				NextChar();
		}

		protected static bool IsIdentifierChar(char c)
		{
			if (!char.IsLetterOrDigit(c))
				return (c == '_');
			return true;
		}

		protected string EatWhile(Func<char, bool> test)
		{
			_value.Length = 0;
			while (!IsEof && test(PeekChar()))
				_value.Append(NextChar());
			return _value.ToString();
		}

		protected void ReportError(string message, params object[] args)
		{
			if (Error != null)
				Error(this, new ErrorEventArgs(string.Format(message, args), _buffer.Position));
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

		protected void EatWhiteSpace()
		{
			while (IsWhiteSpace(PeekChar()))
				NextChar();
		}

		protected char NextChar()
		{
			return _buffer.NextChar();
		}

		protected char PeekChar(int index = 0)
		{
			return _buffer.PeekChar(index);
		}

		protected void StartToken()
		{
			_position = _buffer.Position;
		}

		protected BufferPosition TakePosition()
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