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

        // ReSharper disable InconsistentNaming
        [Description("float1x1")]
        Float1x1,

        [Description("float1x2")]
        Float1x2,

        [Description("float1x3")]
        Float1x3,

        [Description("float1x4")]
        Float1x4,

        [Description("float2x1")]
        Float2x1,

        [Description("float2x2")]
        Float2x2,

        [Description("float2x3")]
        Float2x3,

        [Description("float2x4")]
        Float2x4,

        [Description("float3x1")]
        Float3x1,

        [Description("float3x2")]
        Float3x2,

        [Description("float3x3")]
        Float3x3,

        [Description("float3x4")]
        Float3x4,

        [Description("float4x1")]
        Float4x1,

        [Description("float4x2")]
        Float4x2,

        [Description("float4x3")]
        Float4x3,

        [Description("float4x4")]
        Float4x4,
        // ReSharper restore InconsistentNaming

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