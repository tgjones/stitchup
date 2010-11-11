using NUnit.Framework;
using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;
using StitchUp.Content.Pipeline.FragmentLinking.Parser;

namespace StitchUp.Content.Pipeline.Tests.FragmentLinking.Parser
{
	[TestFixture]
	public class ParserTests
	{
		[Test]
		public void CanParseFragmentWithParamBlock()
		{
			// Arrange.
			Lexer lexer = new Lexer(null, @"fragment basic_material;

				[params]
				float alpha;
				float3 color;");

			Pipeline.FragmentLinking.Parser.Parser parser = new Pipeline.FragmentLinking.Parser.Parser(null, lexer.GetTokens());

			// Act.
			FragmentNode fragment = parser.Parse();

			// Assert.
			Assert.AreEqual("basic_material", fragment.Name);
			Assert.AreEqual(1, fragment.Blocks.Count);
			Assert.AreEqual("params", fragment.Blocks[0].Name);
			Assert.AreEqual(2, fragment.Blocks[0].VariableDeclarations.Count);
			Assert.AreEqual(TokenType.Float, fragment.Blocks[0].VariableDeclarations[0].DataType);
			Assert.AreEqual("alpha", fragment.Blocks[0].VariableDeclarations[0].Name);
			Assert.AreEqual(TokenType.Float3, fragment.Blocks[0].VariableDeclarations[1].DataType);
			Assert.AreEqual("color", fragment.Blocks[0].VariableDeclarations[1].Name);
		}
	}
}