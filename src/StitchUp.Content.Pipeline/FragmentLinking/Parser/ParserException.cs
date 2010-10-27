using System;

namespace StitchUp.Content.Pipeline.Parser
{
	public class ParserException : Exception
	{
		public ParserException(string message)
			: base(message)
		{
			
		}
	}
}