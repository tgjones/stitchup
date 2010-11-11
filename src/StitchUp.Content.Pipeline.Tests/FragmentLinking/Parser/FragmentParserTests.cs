using System.Linq;
using NUnit.Framework;
using StitchUp.Content.Pipeline.FragmentLinking.Parser;
using StitchUp.Content.Pipeline.Graphics;

namespace StitchUp.Content.Pipeline.Tests.FragmentLinking.Parser
{
	[TestFixture]
	public class FragmentParserTests
	{
		[Test]
		public void CanParseInterfaceBlock()
		{
			// Arrange.
			const string interfaceBlock = @"
				interface()
				{
					$name = base_texture
					$textures = color_map
					$params = [ world, camera_position, ambient_light_diffuse_color ]
					$vertex = uv
					$interpolators = uv
					$uv = 2
				}";

			// Act.
			FragmentParser parser = new FragmentParser(interfaceBlock);
			parser.Parse();

			// Assert.
			Assert.AreEqual("base_texture", parser.Fragment.InterfaceName);

			Assert.AreEqual(1, parser.Fragment.InterfaceTextures.Count);
			Assert.AreEqual("color_map", parser.Fragment.InterfaceTextures[0]);

			Assert.AreEqual(1, parser.Fragment.InterfaceVertex.Count);
			Assert.AreEqual("uv", parser.Fragment.InterfaceVertex[0]);

			Assert.AreEqual(1, parser.Fragment.InterfaceInterpolators.Count);
			Assert.AreEqual("uv", parser.Fragment.InterfaceInterpolators[0]);

			Assert.AreEqual(1, parser.Fragment.InterfaceParameterMetadata.Count);
			Assert.AreEqual("uv", parser.Fragment.InterfaceParameterMetadata.Keys.ElementAt(0));
			Assert.AreEqual(FragmentParameterDataType.Float2, parser.Fragment.InterfaceParameterMetadata["uv"].DataType);
		}

		[Test]
		public void CanParseFragmentWithPixelShader()
		{
			// Arrange.
			const string interfaceBlock = @"
				interface()
				{
					$name = base_texture
					$textures = color_map
					$vertex = uv
					$interpolators = uv
					$uv = 2
				}

				ps 2_0

				void main(INPUT input, inout OUTPUT output)
				{
					output.color = tex2D(color_map, input.uv);
				}
";

			// Act.
			FragmentParser parser = new FragmentParser(interfaceBlock);
			parser.Parse();

			// Assert.
			Assert.AreEqual(1, parser.Fragment.CodeBlocks.Count);
			Assert.AreEqual(FragmentCodeShaderType.PixelShader, parser.Fragment.CodeBlocks[0].ShaderType);
			Assert.AreEqual("2_0", parser.Fragment.CodeBlocks[0].Version);
			Assert.AreEqual(@"void main(INPUT input, inout OUTPUT output)
				{
					output.color = tex2D(color_map, input.uv);
				}", parser.Fragment.CodeBlocks[0].Code);
		}

		[Test]
		public void CanParseFragmentWithVertexShader()
		{
			// Arrange.
			const string interfaceBlock = @"
				interface()
				{
					$name = transform_position

					$params = wvp
					$wvp = [ matrix, semantic=""WORLDVIEWPROJECTION"" ]

					$vertex = position
					$position = [ 3, semantic=""POSITION"" ]
				}

				vs 1_1

				void main(INPUT input, inout OUTPUT output)
				{
					output.position = mul(float4(input.position, 1), wvp);
				}
";

			// Act.
			FragmentParser parser = new FragmentParser(interfaceBlock);
			parser.Parse();

			// Assert.
			Assert.AreEqual(2, parser.Fragment.InterfaceParameterMetadata.Count);
			Assert.AreEqual(FragmentParameterDataType.Matrix, parser.Fragment.InterfaceParameterMetadata["wvp"].DataType);
			Assert.AreEqual("WORLDVIEWPROJECTION", parser.Fragment.InterfaceParameterMetadata["wvp"].Semantic);
			Assert.AreEqual(FragmentParameterDataType.Float3, parser.Fragment.InterfaceParameterMetadata["position"].DataType);
			Assert.AreEqual("POSITION", parser.Fragment.InterfaceParameterMetadata["position"].Semantic);
			Assert.AreEqual(1, parser.Fragment.CodeBlocks.Count);
			Assert.AreEqual(FragmentCodeShaderType.VertexShader, parser.Fragment.CodeBlocks[0].ShaderType);
			Assert.AreEqual("1_1", parser.Fragment.CodeBlocks[0].Version);
			Assert.AreEqual(@"void main(INPUT input, inout OUTPUT output)
				{
					output.position = mul(float4(input.position, 1), wvp);
				}", parser.Fragment.CodeBlocks[0].Code);
		}
	}
}