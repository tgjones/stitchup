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
			Assert.AreEqual(DataType.Float, fragment.Parameters.VariableDeclarations[0].DataType);
			Assert.AreEqual("alpha", fragment.Parameters.VariableDeclarations[0].Name);
			Assert.AreEqual("ALPHA", fragment.Parameters.VariableDeclarations[0].Semantic);
			Assert.AreEqual(DataType.Float3, fragment.Parameters.VariableDeclarations[1].DataType);
			Assert.AreEqual("color", fragment.Parameters.VariableDeclarations[1].Name);
		}
	}
}