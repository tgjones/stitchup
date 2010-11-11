using StitchUp.Content.Pipeline.FragmentLinking.Parser;

namespace StitchUp.Content.Pipeline.FragmentLinking.CodeModel
{
	public class IdentifierToken : Token
	{
		public string Identifier { get; private set; }

		public IdentifierToken(string identifier, string sourcePath, BufferPosition position)
			: base(TokenType.Identifier, sourcePath, position)
		{
			Identifier = identifier;
		}
	}
}