using System;

namespace StitchUp.Content.Pipeline.Graphics
{
	public static class FragmentParameterDataTypeUtility
	{
		public static FragmentParameterDataType FromString(string value)
		{
			switch (value)
			{
				case "bool":
					return FragmentParameterDataType.Bool;
				case "1":
					return FragmentParameterDataType.Float;
				case "2" :
					return FragmentParameterDataType.Float2;
				case "3" :
					return FragmentParameterDataType.Float3;
				case "4":
					return FragmentParameterDataType.Float4;
				case "matrix" :
					return FragmentParameterDataType.Matrix;
				case "texture2D":
					return FragmentParameterDataType.Texture2D;
				default :
					throw new NotSupportedException(string.Format("'{0}' is not a valid value for a parameter data type.", value));
			}
		}

		public static string ToString(FragmentParameterDataType dataType)
		{
			switch (dataType)
			{
				case FragmentParameterDataType.Bool:
					return "bool";
				case FragmentParameterDataType.Float:
					return "float";
				case FragmentParameterDataType.Float2:
					return "float2";
				case FragmentParameterDataType.Float3 :
					return "float3";
				case FragmentParameterDataType.Float4:
					return "float4";
				case FragmentParameterDataType.Matrix :
					return "matrix";
				case FragmentParameterDataType.Texture2D:
					return "texture2D";
				default:
					throw new NotSupportedException();
			}
		}
	}
}