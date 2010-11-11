using System.IO;

namespace StitchUp.Content.Pipeline.FragmentLinking.Parser
{
	public class LineReader : StringReader
	{
		public LineReader(string s)
			: base(s)
		{ }
		bool _haveline = false;
		private string _currentline;
		public string PeekLine()
		{
			if (!_haveline)
			{
				_currentline = ReadLine();
				_haveline = true;
			}
			return _currentline;
		}
		public override string ReadLine()
		{
			if (_haveline)
			{
				_haveline = false;
				return _currentline;
			}
			else
				return base.ReadLine();
		}

		public override int Peek()
		{
			if (!_haveline || string.IsNullOrEmpty(_currentline))
				return base.Peek();
			return _currentline[0];
		}
	}
}