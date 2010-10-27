using System;

namespace StitchUp.Content.Pipeline.Graphics
{
	public static class FragmentParameterDataTypeUtility
	{
		public static FragmentParameterDataType FromString(string value)
		{
			switch (value)
			{
				case "2" :
					return FragmentParameterDataType.Float2;
				case "3" :
					return FragmentParameterDataType.Float3;
				case "matrix" :
					return FragmentParameterDataType.Matrix;
				default :
					throw new NotSupportedException();
			}
		}

		public static string ToString(FragmentParameterDataType dataType)
		{
			switch (dataType)
			{
				case FragmentParameterDataType.Float2:
					return "float2";
				case FragmentParameterDataType.Float3 :
					return "float3";
				case FragmentParameterDataType.Matrix :
					return "matrix";
				default:
					throw new NotSupportedException();
			}
		}
	}
}