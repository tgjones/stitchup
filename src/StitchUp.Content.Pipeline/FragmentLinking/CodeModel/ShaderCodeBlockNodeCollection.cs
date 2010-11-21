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
            // First attempt to get a shader matching this specific profile.
            ShaderCodeBlockNode result = this.FirstOrDefault(n => n.ShaderProfile != null && n.ShaderProfile.Value <= targetShaderProfile);
            if (result != null)
                return result;

            // Otherwise get the first shader with an unspecified profile.
            return this.FirstOrDefault(n => n.ShaderProfile == null);
		}
	}
}