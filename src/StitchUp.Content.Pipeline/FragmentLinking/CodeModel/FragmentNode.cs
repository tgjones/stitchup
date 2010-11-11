using System.Collections.Generic;
using System.Linq;

namespace StitchUp.Content.Pipeline.FragmentLinking.CodeModel
{
	public class FragmentNode : ParseNode
	{
		public string Name { get; set; }

		public ParameterBlockNode Interpolators { get; set; }
		public ParameterBlockNode Parameters { get; set; }
		public ParameterBlockNode VertexAttributes { get; set; }
		public ParameterBlockNode Textures { get; set; }

		public HeaderCodeBlockNode HeaderCode { get; set; }

		public ShaderCodeBlockNodeCollection VertexShaders { get; set; }
		public ShaderCodeBlockNodeCollection PixelShaders { get; set; }
	}
}