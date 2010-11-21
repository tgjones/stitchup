using NUnit.Framework;
using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;
using StitchUp.Content.Pipeline.FragmentLinking.Parser;

namespace StitchUp.Content.Pipeline.Tests.FragmentLinking.Parser
{
	[TestFixture]
	public class FragmentParserTests
	{
		[Test]
		public void CanParseFragmentWithParamBlock()
		{
			// Arrange.
			Lexer lexer = new Lexer(null, @"fragment basic_material;

				[params]
				float alpha : ALPHA;
				float3 color;");

			FragmentParser parser = new FragmentParser(null, lexer.GetTokens());

			// Act.
			FragmentNode fragment = parser.Parse();

			// Assert.
			Assert.AreEqual("basic_material", fragment.Name);
			Assert.IsNotNull(fragment.Parameters);
			Assert.AreEqual(2, fragment.Parameters.VariableDeclarations.Count);
			Assert.AreEqual(TokenType.Float, fragment.Parameters.VariableDeclarations[0].DataType);
			Assert.AreEqual("alpha", fragment.Parameters.VariableDeclarations[0].Name);
			Assert.AreEqual("ALPHA", fragment.Parameters.VariableDeclarations[0].Semantic);
            Assert.AreEqual(TokenType.Float3, fragment.Parameters.VariableDeclarations[1].DataType);
			Assert.AreEqual("color", fragment.Parameters.VariableDeclarations[1].Name);
		}

		[Test]
		public void CanParseFragmentWithParamBlockWithInitialValues()
		{
			// Arrange.
			Lexer lexer = new Lexer(null, @"fragment basic_material;

				[params]
				float alpha : ALPHA = 1.0f;
				float3 dir = float3(1.0f, -1.1f, 0.0);
				bool enable = false;");

			FragmentParser parser = new FragmentParser(null, lexer.GetTokens());

			// Act.
			FragmentNode fragment = parser.Parse();

			// Assert.
			Assert.AreEqual("basic_material", fragment.Name);
			Assert.IsNotNull(fragment.Parameters);
			Assert.AreEqual(3, fragment.Parameters.VariableDeclarations.Count);
            Assert.AreEqual(TokenType.Float, fragment.Parameters.VariableDeclarations[0].DataType);
			Assert.AreEqual("alpha", fragment.Parameters.VariableDeclarations[0].Name);
			Assert.AreEqual("ALPHA", fragment.Parameters.VariableDeclarations[0].Semantic);
			Assert.AreEqual("1", fragment.Parameters.VariableDeclarations[0].InitialValue);
            Assert.AreEqual(TokenType.Float3, fragment.Parameters.VariableDeclarations[1].DataType);
			Assert.AreEqual("dir", fragment.Parameters.VariableDeclarations[1].Name);
			Assert.AreEqual("float3(1,-1.1,0)", fragment.Parameters.VariableDeclarations[1].InitialValue);
            Assert.AreEqual(TokenType.Bool, fragment.Parameters.VariableDeclarations[2].DataType);
			Assert.AreEqual("enable", fragment.Parameters.VariableDeclarations[2].Name);
			Assert.AreEqual("false", fragment.Parameters.VariableDeclarations[2].InitialValue);
		}
	}
}