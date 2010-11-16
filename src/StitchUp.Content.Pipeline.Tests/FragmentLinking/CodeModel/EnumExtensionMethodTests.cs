using NUnit.Framework;
using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;

namespace StitchUp.Content.Pipeline.Tests.FragmentLinking.CodeModel
{
	[TestFixture]
	public class EnumExtensionMethodTests
	{
		[Test]
		public void CanGetDescription()
		{
			// Arrange.
			const ShaderModel shaderModel = ShaderModel.Version2_0;

			// Act.
			string description = shaderModel.GetDescription();

			// Assert.
			Assert.AreEqual("2_0", description);
		}
	}
}