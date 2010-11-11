using System.ComponentModel;

namespace StitchUp.Content.Pipeline.FragmentLinking.CodeModel
{
	public enum TokenType
	{
		[Description("fragment")]
		Fragment,

		[Description("effect")]
		Effect,

		[Description("float")]
		Float,

		[Description("float2")]
		Float2,

		[Description("float3")]
		Float3,

		[Description("float4")]
		Float4,

		[Description("matrix")]
		Matrix,

		[Description("bool")]
		Bool,

		[Description("texture2D")]
		Texture2D,

		[Description("Identifier")]
		Identifier,

		[Description("EOF")]
		Eof,

		[Description("[")]
		OpenSquare,

		[Description("]")]
		CloseSquare,

		[Description("{")]
		OpenCurly,

		[Description("}")]
		CloseCurly,

		[Description("=")]
		Equal,

		[Description(",")]
		Comma,

		[Description(":")]
		Colon,

		[Description("String")]
		String,

		[Description("Error")]
		Error,

		[Description(";")]
		Semicolon
	}
}