namespace StitchUp.Content.Pipeline.FragmentLinking.Parser
{
	public struct BufferPosition
	{
		public int Line { get; set; }
		public int Column { get; set; }
		public int Offset { get; set; }

		public BufferPosition(int line, int column, int offset)
			: this()
		{
			Line = line;
			Column = column;
			Offset = offset;
		}

		public override string ToString()
		{
			if (Line == 0 && Column == 0)
				return string.Empty;
			return string.Format("({0}, {1})", Line + 1, Column + 1);
		}

		public override bool Equals(object obj)
		{
			return ((obj is BufferPosition) && (this == ((BufferPosition)obj)));
		}

		public override int GetHashCode()
		{
			return ((Line * 128) + Column);
		}

		public static bool operator ==(BufferPosition a, BufferPosition b)
		{
			return (a.Line == b.Line && a.Column == b.Column && a.Offset == b.Offset);
		}

		public static bool operator !=(BufferPosition a, BufferPosition b)
		{
			return !(a == b);
		}
	}
}