using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;

namespace StitchUp.Content.Pipeline.FragmentLinking.EffectModel
{
	public class StitchedFragmentSymbol
	{
		public string UniqueName { get; private set; }
		public FragmentNode FragmentNode { get; private set; }

		public StitchedFragmentSymbol(string uniqueName, FragmentNode fragmentNode)
		{
			UniqueName = uniqueName;
			FragmentNode = fragmentNode;
		}
	}
}