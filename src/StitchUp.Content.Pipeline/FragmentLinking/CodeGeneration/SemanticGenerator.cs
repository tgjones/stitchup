using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;

namespace StitchUp.Content.Pipeline.FragmentLinking.CodeGeneration
{
	public class SemanticGenerator
	{
		private readonly string _semanticPrefix;
		private int _currentIndex;

		public SemanticGenerator(string semanticPrefix, int startIndex = 0)
		{
			_semanticPrefix = semanticPrefix;
			_currentIndex = startIndex;
		}

		public string GetNextSemantic(VariableDeclarationNode variable)
		{
			if (!string.IsNullOrEmpty(variable.Semantic))
				return variable.Semantic;
			return _semanticPrefix + _currentIndex++;
		}
	}
}