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
			const ShaderProfile shaderProfile = ShaderProfile.Version2_0;

			// Act.
			string description = shaderProfile.GetDescription();

			// Assert.
			Assert.AreEqual("2_0", description);
		}
	}
}