using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;

namespace StitchUp.SurfaceShaders.Content.Pipeline.Shaders
{
	public class ShaderNode : ParseNode
	{
		public ShaderFileType Type { get; set; }
		public string Name { get; set; }

		public string EffectCode { get; set; }

		public ParameterBlockNode Interpolators { get; set; }
		public ParameterBlockNode Parameters { get; set; }
		public ParameterBlockNode VertexAttributes { get; set; }
		public ParameterBlockNode Textures { get; set; }
		public ParameterBlockNode PixelOutputs { get; set; }

		public LightingModelBlockNode LightingModel { get; set; }

		public HeaderCodeBlockNode HeaderCode { get; set; }

		public ShaderCodeBlockNodeCollection VertexShaders { get; set; }
		public ShaderCodeBlockNodeCollection PixelShaders { get; set; }
	}

	public enum ShaderFileType
	{
		SurfaceShader,
		Effect
	}
}