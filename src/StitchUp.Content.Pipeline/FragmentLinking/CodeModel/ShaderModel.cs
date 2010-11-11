using System.ComponentModel;

namespace StitchUp.Content.Pipeline.FragmentLinking.CodeModel
{
	public enum ShaderModel
	{
		// ReSharper disable InconsistentNaming
		[Description("1_0")]
		Version1_0,

		[Description("1_1")]
		Version1_1,

		[Description("2_0")]
		Version2_0,

		[Description("3_0")]
		Version3_0
		// ReSharper restore InconsistentNaming
	}
}