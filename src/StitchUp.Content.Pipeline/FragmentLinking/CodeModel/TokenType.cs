using System.ComponentModel;

namespace StitchUp.Content.Pipeline.FragmentLinking.CodeModel
{
	public enum TokenType
	{
		[Description("fragment")]
		Fragment,

        [Description("stitchedeffect")] // Everything after this and before "true" is a data type
		StitchedEffect,

		[Description("int")]
		Int,

		[Description("int2")]
		Int2,

		[Description("int3")]
		Int3,

		[Description("int4")]
		Int4,

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
		[Description("int1x1")]
		Int1x1,

		[Description("int1x2")]
		Int1x2,

		[Description("int1x3")]
		Int1x3,

		[Description("int1x4")]
		Int1x4,

		[Description("int2x1")]
		Int2x1,

		[Description("int2x2")]
		Int2x2,

		[Description("int2x3")]
		Int2x3,

		[Description("int2x4")]
		Int2x4,

		[Description("int3x1")]
		Int3x1,

		[Description("int3x2")]
		Int3x2,

		[Description("int3x3")]
		Int3x3,

		[Description("int3x4")]
		Int3x4,

		[Description("int4x1")]
		Int4x1,

		[Description("int4x2")]
		Int4x2,

		[Description("int4x3")]
		Int4x3,

		[Description("int4x4")]
		Int4x4,

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

		[Description("technique")]
		Technique,

		[Description("pass")]
		Pass,

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