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

		public ShaderCodeBlockNode GetCodeBlock(ShaderProfile targetShaderProfile)
		{
			return this.FirstOrDefault(n => n.ShaderProfile <= targetShaderProfile);
		}
	}
}