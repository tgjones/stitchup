using System;
using System.ComponentModel;
using System.Reflection;

namespace StitchUp.Content.Pipeline.FragmentLinking.CodeModel
{
	public static class EnumExtensionMethods
	{
		public static string GetDescription(this Enum value)
		{
			FieldInfo fi = value.GetType().GetField(value.ToString());
			object[] descriptionAttrs = fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
			if (descriptionAttrs.Length > 0)
			{
				DescriptionAttribute description = (DescriptionAttribute)descriptionAttrs[0];
				return description.Description;
			}
			return value.ToString();
		}
	}
}