using System;

namespace StitchUp.Content.Pipeline.FragmentLinking.CodeModel
{
	public static class DataTypeUtility
	{
		public static DataType FromString(string value)
		{
			switch (value)
			{
				case "bool":
					return DataType.Bool;
				case "1":
					return DataType.Float;
				case "2" :
					return DataType.Float2;
				case "3" :
					return DataType.Float3;
				case "4":
					return DataType.Float4;
				case "matrix" :
					return DataType.Matrix;
				case "texture2D":
					return DataType.Texture2D;
				default :
					throw new NotSupportedException(string.Format("'{0}' is not a valid value for a parameter data type.", value));
			}
		}

		public static string ToString(DataType dataType)
		{
			switch (dataType)
			{
				case DataType.Bool:
					return "bool";
				case DataType.Float:
					return "float";
				case DataType.Float2:
					return "float2";
				case DataType.Float3 :
					return "float3";
				case DataType.Float4:
					return "float4";
				case DataType.Matrix :
					return "matrix";
				case DataType.Texture2D:
					return "texture2D";
				default:
					throw new NotSupportedException();
			}
		}
	}
}