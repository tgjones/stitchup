using StitchUp.Content.Pipeline.FragmentLinking.Parser;

namespace StitchUp.Content.Pipeline.FragmentLinking.CodeModel
{
	public class StringToken : Token
	{
		public string Value { get; private set; }

		public StringToken(string value, string sourcePath, BufferPosition position) : base(TokenType.String, sourcePath, position)
		{
			Value = value;
		}
	}
}