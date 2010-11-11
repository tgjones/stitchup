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
			ShaderModel shaderModel = ShaderModel.Version1_0;

			// Act.
			string description = shaderModel.GetDescription();

			// Assert.
			Assert.AreEqual("1_0", description);
		}
	}
}