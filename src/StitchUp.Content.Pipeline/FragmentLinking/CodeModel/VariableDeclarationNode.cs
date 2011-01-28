namespace StitchUp.Content.Pipeline.FragmentLinking.CodeModel
{
	public class VariableDeclarationNode : ParseNode
	{
		public TokenType DataType { get; set; }
		public string Name { get; set; }
		public bool IsArray { get; set; }
		public string ArraySize { get; set; }
		public string Semantic { get; set; }
		public string InitialValue { get; set; }
	}
}