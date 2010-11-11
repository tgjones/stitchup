using System;

namespace StitchUp.Content.Pipeline.FragmentLinking.Parser
{
	public class ErrorEventArgs : EventArgs
	{
		public string Message { get; private set; }
		public BufferPosition Position { get; private set; }

		public ErrorEventArgs(string message, BufferPosition position)
		{
			Message = message;
			Position = position;
		}
	}
}