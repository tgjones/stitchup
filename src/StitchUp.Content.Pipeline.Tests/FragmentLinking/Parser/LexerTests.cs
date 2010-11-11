using System.Linq;
using NUnit.Framework;
using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;
using StitchUp.Content.Pipeline.FragmentLinking.Parser;

namespace StitchUp.Content.Pipeline.Tests.FragmentLinking.Parser
{
	[TestFixture]
	public class LexerTests
	{
		private static void TestTokenType(string value, TokenType expectedTokenType)
		{
			// Arrange.
			Lexer lexer = new Lexer(null, value);

			// Act.
			Token token = lexer.NextToken();

			// Assert.
			Assert.AreEqual(expectedTokenType, token.Type);
		}

		[Test]
		public void CanLexKeyword()
		{
			TestTokenType("fragment", TokenType.Fragment);
			TestTokenType("effect", TokenType.Effect);
		}

		[Test]
		public void CanLexPunctuation()
		{
			TestTokenType("[", TokenType.OpenSquare);
			TestTokenType("]", TokenType.CloseSquare);
			TestTokenType("{", TokenType.OpenCurly);
			TestTokenType("}", TokenType.CloseCurly);
			TestTokenType("=", TokenType.Equal);
			TestTokenType(":", TokenType.Colon);
			TestTokenType(",", TokenType.Comma);
		}

		[Test]
		public void CanLexString()
		{
			TestTokenType("\"this is a string\"", TokenType.String);
		}

		[Test]
		public void CanLexIdentifier()
		{
			TestTokenType("identifier", TokenType.Identifier);
		}

		[Test]
		public void CanLexEmptyFragment()
		{
			// Arrange.
			Lexer lexer = new Lexer(null, "fragment { }");

			// Act.
			var tokens = lexer.GetTokens();

			// Assert.
			Assert.AreEqual(3, tokens.Count());
			Assert.AreEqual(TokenType.Fragment, tokens[0].Type);
			Assert.AreEqual(TokenType.OpenCurly, tokens[1].Type);
			Assert.AreEqual(TokenType.CloseCurly, tokens[2].Type);
		}

		[Test]
		public void CanLexFragmentWithVertexParameters()
		{
			// Arrange.
			Lexer lexer = new Lexer(null, @"fragment MyFragment;

				[vertex]
				float3 normal : ""NORMAL"";
				float2 texCoord;");

			// Act.
			var tokens = lexer.GetTokens();

			// Assert.
			Assert.AreEqual(14, tokens.Count());
			Assert.AreEqual(TokenType.Fragment, tokens[0].Type);
			Assert.AreEqual(TokenType.Identifier, tokens[1].Type);
			Assert.AreEqual("MyFragment", ((IdentifierToken)tokens[1]).Identifier);
			Assert.AreEqual(TokenType.Semicolon, tokens[2].Type);
			Assert.AreEqual(TokenType.OpenSquare, tokens[3].Type);
			Assert.AreEqual(TokenType.Identifier, tokens[4].Type);
			Assert.AreEqual("vertex", ((IdentifierToken)tokens[4]).Identifier);
			Assert.AreEqual(TokenType.CloseSquare, tokens[5].Type);
			Assert.AreEqual(TokenType.Float3, tokens[6].Type);
			Assert.AreEqual(TokenType.Identifier, tokens[7].Type);
			Assert.AreEqual("normal", ((IdentifierToken)tokens[7]).Identifier);
			Assert.AreEqual(TokenType.Colon, tokens[8].Type);
			Assert.AreEqual(TokenType.String, tokens[9].Type);
			Assert.AreEqual("NORMAL", ((StringToken)tokens[9]).Value);
			Assert.AreEqual(TokenType.Semicolon, tokens[10].Type);
			Assert.AreEqual(TokenType.Float2, tokens[11].Type);
			Assert.AreEqual(TokenType.Identifier, tokens[12].Type);
			Assert.AreEqual("texCoord", ((IdentifierToken)tokens[12]).Identifier);
			Assert.AreEqual(TokenType.Semicolon, tokens[13].Type);
		}

		/*[Test]
		public void CanLexCodeBlock()
		{
			// Arrange.
			Lexer lexer = new Lexer(null, @"vs 2_0
				
				void main(INPUT input, inout OUTPUT output)
				{
				}");

			// Act.
			var tokens = lexer.GetTokens();

			// Assert.
			Assert.AreEqual(3, tokens.Count());
			Assert.AreEqual(TokenType.VertexShader, tokens[0].Type);
			Assert.AreEqual(TokenType.OpenCurly, tokens[1].Type);
			Assert.AreEqual(TokenType.CloseCurly, tokens[2].Type);
		}*/
	}
}