using System.ComponentModel;

namespace StitchUp.Content.Pipeline.FragmentLinking.CodeModel
{
	public enum TokenType
	{
		[Description("fragment")]
		Fragment,

        [Description("effect")] // Everything after this and before "true" is a data type
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

        [Description("Texture1D")]
        Texture1D,

		[Description("Texture2D")]
		Texture2D,

        [Description("Texture3D")]
        Texture3D,

        [Description("TextureCube")]
        TextureCube,

        [Description("true")] // Everything before this and before "effect" is a data type
		True,

		[Description("false")]
		False,

		[Description("Identifier")] // Everything before this is a keyword.
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

		[Description("(")]
		OpenParen,

		[Description(")")]
		CloseParen,

		[Description("=")]
		Equal,

		[Description("-")]
		Minus,

		[Description(",")]
		Comma,

		[Description(":")]
		Colon,

		[Description("Error")]
		Error,

		[Description(";")]
		Semicolon,

		[Description("Shader Code")]
		ShaderCode,

		[Description("Literal")]
		Literal
	}
}