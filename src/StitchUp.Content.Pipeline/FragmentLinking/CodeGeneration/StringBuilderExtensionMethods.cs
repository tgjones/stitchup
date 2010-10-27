using System.Text;

namespace StitchUp.Content.Pipeline.FragmentLinking.CodeGeneration
{
	public static class StringBuilderExtensionMethods
	{
		public static void AppendLineFormat(this StringBuilder sb, string format, params object[] args)
		{
			sb.AppendFormat(format, args);
			sb.AppendLine();
		}
	}
}