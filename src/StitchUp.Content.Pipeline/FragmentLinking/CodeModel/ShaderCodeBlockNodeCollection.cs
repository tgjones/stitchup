using System.Collections.Generic;
using System.Linq;

namespace StitchUp.Content.Pipeline.FragmentLinking.CodeModel
{
	public class ShaderCodeBlockNodeCollection : List<ShaderCodeBlockNode>
	{
		public ShaderCodeBlockNodeCollection(IEnumerable<ShaderCodeBlockNode> items)
			: base(items)
		{
			
		}

		public ShaderCodeBlockNodeCollection()
		{
			
		}

		public ShaderCodeBlockNode GetCodeBlock(ShaderModel targetShaderModel)
		{
			return this.FirstOrDefault(n => n.ShaderModel <= targetShaderModel);
		}
	}
}