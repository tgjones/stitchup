using System.IO;
using System.Text;

namespace StitchUp.Content.Pipeline.FragmentLinking.Generator
{
	public class ScriptTextWriter : TextWriter
	{
		private readonly TextWriter _writer;

		public ScriptTextWriter(TextWriter writer)
		{
			_writer = writer;
		}

		public override Encoding Encoding
		{
			get { return _writer.Encoding; }
		}

		public override void WriteLine()
		{
			_writer.WriteLine();
		}

		public override void Write(string value)
		{
			_writer.Write(value);
		}

		public override void Write(string format, object arg0)
		{
			_writer.Write(format, arg0);
		}

		public override void WriteLine(string value)
		{
			_writer.WriteLine(value);
		}

		public override void WriteLine(string format, object arg0)
		{
			_writer.WriteLine(format, arg0);
		}
	}
}