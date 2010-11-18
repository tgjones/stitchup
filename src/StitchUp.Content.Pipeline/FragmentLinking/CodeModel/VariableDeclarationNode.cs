namespace StitchUp.Content.Pipeline.FragmentLinking.CodeModel
{
	public class VariableDeclarationNode : ParseNode
	{
		public DataType DataType { get; set; }
		public string Name { get; set; }
		public string Semantic { get; set; }
		public string InitialValue { get; set; }
	}
}