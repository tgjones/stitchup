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
            TestTokenType("bool", TokenType.Bool);
            TestTokenType("float", TokenType.Float);
            TestTokenType("float2", TokenType.Float2);
            TestTokenType("float3", TokenType.Float3);
            TestTokenType("float4", TokenType.Float4);
			TestTokenType("int", TokenType.Int);
			TestTokenType("int2", TokenType.Int2);
			TestTokenType("int3", TokenType.Int3);
			TestTokenType("int4", TokenType.Int4);
            TestTokenType("matrix", TokenType.Matrix);
            TestTokenType("float1x1", TokenType.Float1x1);
            TestTokenType("float1x2", TokenType.Float1x2);
            TestTokenType("float1x3", TokenType.Float1x3);
            TestTokenType("float1x4", TokenType.Float1x4);
            TestTokenType("float2x1", TokenType.Float2x1);
            TestTokenType("float2x2", TokenType.Float2x2);
            TestTokenType("float2x3", TokenType.Float2x3);
            TestTokenType("float2x4", TokenType.Float2x4);
            TestTokenType("float3x1", TokenType.Float3x1);
            TestTokenType("float3x2", TokenType.Float3x2);
            TestTokenType("float3x3", TokenType.Float3x3);
            TestTokenType("float3x4", TokenType.Float3x4);
            TestTokenType("float4x1", TokenType.Float4x1);
            TestTokenType("float4x2", TokenType.Float4x2);
            TestTokenType("float4x3", TokenType.Float4x3);
            TestTokenType("float4x4", TokenType.Float4x4);
			TestTokenType("int1x1", TokenType.Int1x1);
			TestTokenType("int1x2", TokenType.Int1x2);
			TestTokenType("int1x3", TokenType.Int1x3);
			TestTokenType("int1x4", TokenType.Int1x4);
			TestTokenType("int2x1", TokenType.Int2x1);
			TestTokenType("int2x2", TokenType.Int2x2);
			TestTokenType("int2x3", TokenType.Int2x3);
			TestTokenType("int2x4", TokenType.Int2x4);
			TestTokenType("int3x1", TokenType.Int3x1);
			TestTokenType("int3x2", TokenType.Int3x2);
			TestTokenType("int3x3", TokenType.Int3x3);
			TestTokenType("int3x4", TokenType.Int3x4);
			TestTokenType("int4x1", TokenType.Int4x1);
			TestTokenType("int4x2", TokenType.Int4x2);
			TestTokenType("int4x3", TokenType.Int4x3);
			TestTokenType("int4x4", TokenType.Int4x4);
            TestTokenType("texture1D", TokenType.Texture1D);
            TestTokenType("texture2D", TokenType.Texture2D);
            TestTokenType("texture3D", TokenType.Texture3D);
            TestTokenType("textureCube", TokenType.TextureCube);
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
			TestTokenType("\"this is a string\"", TokenType.Literal);
		}

		[Test]
		public void CanLexIdentifier()
		{
			TestTokenType("identifier", TokenType.Identifier);
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
			Assert.AreEqual(15, tokens.Count());
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
			Assert.AreEqual(TokenType.Literal, tokens[9].Type);
			Assert.AreEqual("NORMAL", ((StringToken)tokens[9]).Value);
			Assert.AreEqual(TokenType.Semicolon, tokens[10].Type);
			Assert.AreEqual(TokenType.Float2, tokens[11].Type);
			Assert.AreEqual(TokenType.Identifier, tokens[12].Type);
			Assert.AreEqual("texCoord", ((IdentifierToken)tokens[12]).Identifier);
			Assert.AreEqual(TokenType.Semicolon, tokens[13].Type);
			Assert.AreEqual(TokenType.Eof, tokens[14].Type);
		}

		[Test]
		public void CanLexDeclarationWithSemantic()
		{
			// Arrange.
			Lexer lexer = new Lexer(null, @"float3 normal : ""NORMAL"";");

			// Act.
			var tokens = lexer.GetTokens();

			// Assert.
			Assert.AreEqual(6, tokens.Count());
			Assert.AreEqual(TokenType.Float3, tokens[0].Type);
			Assert.AreEqual(TokenType.Identifier, tokens[1].Type);
			Assert.AreEqual("normal", ((IdentifierToken)tokens[1]).Identifier);
			Assert.AreEqual(TokenType.Colon, tokens[2].Type);
			Assert.AreEqual(TokenType.Literal, tokens[3].Type);
			Assert.AreEqual("NORMAL", ((StringToken)tokens[3]).Value);
			Assert.AreEqual(TokenType.Semicolon, tokens[4].Type);
			Assert.AreEqual(TokenType.Eof, tokens[5].Type);
		}

		[Test]
		public void CanLexDeclarationWithInitialValue1()
		{
			// Arrange.
			Lexer lexer = new Lexer(null, @"float3 normal = float3(0, 1, 0);");

			// Act.
			var tokens = lexer.GetTokens();

			// Assert.
			Assert.AreEqual(13, tokens.Count());
			Assert.AreEqual(TokenType.Float3, tokens[0].Type);
			Assert.AreEqual(TokenType.Identifier, tokens[1].Type);
			Assert.AreEqual("normal", ((IdentifierToken)tokens[1]).Identifier);
			Assert.AreEqual(TokenType.Equal, tokens[2].Type);
			Assert.AreEqual(TokenType.Float3, tokens[3].Type);
			Assert.AreEqual(TokenType.OpenParen, tokens[4].Type);
			Assert.AreEqual(TokenType.Literal, tokens[5].Type);
			Assert.AreEqual(0, ((IntToken)tokens[5]).Value);
			Assert.AreEqual(TokenType.Comma, tokens[6].Type);
			Assert.AreEqual(TokenType.Literal, tokens[7].Type);
			Assert.AreEqual(1, ((IntToken)tokens[7]).Value);
			Assert.AreEqual(TokenType.Comma, tokens[8].Type);
			Assert.AreEqual(TokenType.Literal, tokens[9].Type);
			Assert.AreEqual(0, ((IntToken)tokens[9]).Value);
			Assert.AreEqual(TokenType.CloseParen, tokens[10].Type);
			Assert.AreEqual(TokenType.Semicolon, tokens[11].Type);
			Assert.AreEqual(TokenType.Eof, tokens[12].Type);
		}

		[Test]
		public void CanLexDeclarationWithInitialValue2()
		{
			// Arrange.
			Lexer lexer = new Lexer(null, @"float3 normal = {0, -1f, 0.0f};");

			// Act.
			var tokens = lexer.GetTokens();

			// Assert.
			Assert.AreEqual(13, tokens.Count());
			Assert.AreEqual(TokenType.Float3, tokens[0].Type);
			Assert.AreEqual(TokenType.Identifier, tokens[1].Type);
			Assert.AreEqual("normal", ((IdentifierToken)tokens[1]).Identifier);
			Assert.AreEqual(TokenType.Equal, tokens[2].Type);
			Assert.AreEqual(TokenType.OpenCurly, tokens[3].Type);
			Assert.AreEqual(TokenType.Literal, tokens[4].Type);
			Assert.AreEqual(0, ((IntToken)tokens[4]).Value);
			Assert.AreEqual(TokenType.Comma, tokens[5].Type);
			Assert.AreEqual(TokenType.Minus, tokens[6].Type);
			Assert.AreEqual(TokenType.Literal, tokens[7].Type);
			Assert.AreEqual(1f, ((FloatToken)tokens[7]).Value);
			Assert.AreEqual(TokenType.Comma, tokens[8].Type);
			Assert.AreEqual(TokenType.Literal, tokens[9].Type);
			Assert.AreEqual(0.0f, ((FloatToken)tokens[9]).Value);
			Assert.AreEqual(TokenType.CloseCurly, tokens[10].Type);
			Assert.AreEqual(TokenType.Semicolon, tokens[11].Type);
			Assert.AreEqual(TokenType.Eof, tokens[12].Type);
		}

		[Test]
		public void CanLexDeclarationWithSemanticAndInitialValue1()
		{
			// Arrange.
			Lexer lexer = new Lexer(null, @"float3 normal : ""NORMAL"" = float3(0, 1, 0);");

			// Act.
			var tokens = lexer.GetTokens();

			// Assert.
			Assert.AreEqual(15, tokens.Count());
			Assert.AreEqual(TokenType.Float3, tokens[0].Type);
			Assert.AreEqual(TokenType.Identifier, tokens[1].Type);
			Assert.AreEqual("normal", ((IdentifierToken)tokens[1]).Identifier);
			Assert.AreEqual(TokenType.Colon, tokens[2].Type);
			Assert.AreEqual(TokenType.Literal, tokens[3].Type);
			Assert.AreEqual("NORMAL", ((StringToken)tokens[3]).Value);
			Assert.AreEqual(TokenType.Equal, tokens[4].Type);
			Assert.AreEqual(TokenType.Float3, tokens[5].Type);
			Assert.AreEqual(TokenType.OpenParen, tokens[6].Type);
			Assert.AreEqual(TokenType.Literal, tokens[7].Type);
			Assert.AreEqual(0, ((IntToken)tokens[7]).Value);
			Assert.AreEqual(TokenType.Comma, tokens[8].Type);
			Assert.AreEqual(TokenType.Literal, tokens[9].Type);
			Assert.AreEqual(1, ((IntToken)tokens[9]).Value);
			Assert.AreEqual(TokenType.Comma, tokens[10].Type);
			Assert.AreEqual(TokenType.Literal, tokens[11].Type);
			Assert.AreEqual(0, ((IntToken)tokens[11]).Value);
			Assert.AreEqual(TokenType.CloseParen, tokens[12].Type);
			Assert.AreEqual(TokenType.Semicolon, tokens[13].Type);
			Assert.AreEqual(TokenType.Eof, tokens[14].Type);
		}

		[Test]
		public void CanLexCodeBlock()
		{
			// Arrange.
			Lexer lexer = new Lexer(null, @"[vs 2_0]
				__hlsl__
				void main(INPUT input, inout OUTPUT output)
				{
				}
				__hlsl__");

			// Act.
			var tokens = lexer.GetTokens();

			// Assert.
			Assert.AreEqual(6, tokens.Count());
			Assert.AreEqual(TokenType.OpenSquare, tokens[0].Type);
			Assert.AreEqual(TokenType.Identifier, tokens[1].Type);
			Assert.AreEqual("vs", ((IdentifierToken)tokens[1]).Identifier);
			Assert.AreEqual(TokenType.Identifier, tokens[2].Type);
			Assert.AreEqual("2_0", ((IdentifierToken)tokens[2]).Identifier);
			Assert.AreEqual(TokenType.CloseSquare, tokens[3].Type);
			Assert.AreEqual(TokenType.ShaderCode, tokens[4].Type);
			Assert.AreEqual("void main(INPUT input, inout OUTPUT output)\r\t\t\t\t{\r\t\t\t\t}", ((ShaderCodeToken)tokens[4]).ShaderCode);
			Assert.AreEqual(TokenType.Eof, tokens[5].Type);
		}

		[Test]
		public void CanLexComment()
		{
			// Arrange.
			Lexer lexer = new Lexer(null, @"[params] // this is a comment
				// This is a commented line float3 = { }
				[interpolators]");

			// Act.
			var tokens = lexer.GetTokens();

			// Assert.
			Assert.AreEqual(7, tokens.Count());
			Assert.AreEqual(TokenType.OpenSquare, tokens[0].Type);
			Assert.AreEqual(TokenType.Identifier, tokens[1].Type);
			Assert.AreEqual("params", ((IdentifierToken)tokens[1]).Identifier);
			Assert.AreEqual(TokenType.CloseSquare, tokens[2].Type);
			Assert.AreEqual(TokenType.OpenSquare, tokens[3].Type);
			Assert.AreEqual(TokenType.Identifier, tokens[4].Type);
			Assert.AreEqual("interpolators", ((IdentifierToken)tokens[4]).Identifier);
			Assert.AreEqual(TokenType.CloseSquare, tokens[5].Type);
			Assert.AreEqual(TokenType.Eof, tokens[6].Type);
		}

        [Test]
        public void BadlyFormattedCommentResultsInErrorToken()
        {
            // Arrange.
            Lexer lexer = new Lexer(null, @"/ this is a bad comment");

            // Act.
            var tokens = lexer.GetTokens();

            // Assert.
            Assert.AreEqual(TokenType.Error, tokens[0].Type);
        }
	}
}