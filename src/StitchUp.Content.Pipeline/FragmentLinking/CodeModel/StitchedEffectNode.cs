using System.Collections.Generic;
using System.Linq;

namespace StitchUp.Content.Pipeline.FragmentLinking.CodeModel
{
	public class StitchedEffectNode
	{
		public List<StitchedFragmentNode> StitchedFragments { get; private set; }

		public StitchedEffectNode(List<StitchedFragmentNode> stitchedFragments)
		{
			StitchedFragments = stitchedFragments;
		}

		public bool CanBeCompiledForShaderModel(ShaderModel shaderModel)
		{
			foreach (StitchedFragmentNode stitchedFragment in StitchedFragments)
			{
				// There might not be any vertex or pixel shaders, and that's OK, since we'll create them as needed.
				// But if there IS a vertex or pixel shader, we should not automatically create one.
				if (stitchedFragment.FragmentNode.VertexShaders.Any()
					&& stitchedFragment.FragmentNode.VertexShaders.GetCodeBlock(shaderModel) == null)
					return false;
				if (stitchedFragment.FragmentNode.PixelShaders.Any()
					&& stitchedFragment.FragmentNode.PixelShaders.GetCodeBlock(shaderModel) == null)
					return false;
			}
			return true;
		}
	}
}