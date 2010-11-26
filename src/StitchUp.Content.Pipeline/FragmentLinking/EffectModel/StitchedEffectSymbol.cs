using System.Collections.Generic;
using System.Linq;
using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;

namespace StitchUp.Content.Pipeline.FragmentLinking.EffectModel
{
	public class StitchedEffectSymbol
	{
		public List<StitchedFragmentSymbol> StitchedFragments { get; set; }
		public List<TechniqueSymbol> Techniques { get; set; }

		public bool CanBeCompiledForShaderProfile(ShaderProfile shaderProfile)
		{
			foreach (StitchedFragmentSymbol stitchedFragment in StitchedFragments)
			{
				// There might not be any vertex or pixel shaders, and that's OK, since we'll create them as needed.
				// But if there IS a vertex or pixel shader, we should not automatically create one.
				if (stitchedFragment.FragmentNode.VertexShaders.Any()
				    && stitchedFragment.FragmentNode.VertexShaders.GetCodeBlock(shaderProfile) == null)
					return false;
				if (stitchedFragment.FragmentNode.PixelShaders.Any()
				    && stitchedFragment.FragmentNode.PixelShaders.GetCodeBlock(shaderProfile) == null)
					return false;
			}
			return true;
		}
	}
}