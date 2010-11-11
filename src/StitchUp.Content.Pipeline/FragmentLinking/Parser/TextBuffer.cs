using System;

namespace StitchUp.Content.Pipeline.FragmentLinking.Parser
{
	public class TextBuffer
	{
		private readonly string _text;
		private int _offset;

		public int Column { get; private set; }
		public int Line { get; private set; }

		public bool IsEof
		{
			get { return (RemainingLength == 0); }
		}

		public BufferPosition Position
		{
			get { return new BufferPosition(Line, Column, _offset); }
		}

		public int RemainingLength
		{
			get { return (_text.Length - _offset); }
		}

		public TextBuffer(string text)
		{
			_text = text;
		}

		public char NextChar()
		{
			char ch = PeekChar();
			_offset++;
			Column++;

			if (Lexer.IsLineSeparator(ch))
			{
				Column = 0;
				Line++;
				if (ch == '\r' && _offset < _text.Length && PeekChar() == '\n')
					_offset++;
			}

			return ch;
		}

		public char PeekChar()
		{
			return PeekChar(0);
		}

		public char PeekChar(int index)
		{
			if (_offset + index >= _text.Length)
				return '\0';

			return _text[_offset + index];
		}
	}
}