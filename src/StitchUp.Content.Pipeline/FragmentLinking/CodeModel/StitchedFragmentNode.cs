namespace StitchUp.Content.Pipeline.FragmentLinking.CodeModel
{
	public class StitchedFragmentNode
	{
		public string UniqueName { get; private set; }
		public FragmentNode FragmentNode { get; private set; }

		public StitchedFragmentNode(string uniqueName, FragmentNode fragmentNode)
		{
			UniqueName = uniqueName;
			FragmentNode = fragmentNode;
		}
	}
}