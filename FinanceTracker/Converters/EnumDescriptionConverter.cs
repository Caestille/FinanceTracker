using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace FinanceTracker.Converters
{
	public class EnumDescriptionConverter : IValueConverter
	{
		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Enum)
				return GetEnumDescription((Enum)value);

			return Binding.DoNothing;
		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Binding.DoNothing;
		}

		private string GetEnumDescription(Enum enumObj)
		{
			FieldInfo? fieldInfo = enumObj.GetType().GetField(enumObj.ToString());

			object[] attribArray = fieldInfo!.GetCustomAttributes(false);

			if (attribArray.Length == 0)
			{
				return enumObj.ToString();
			}

			DescriptionAttribute attrib = (attribArray[0] as DescriptionAttribute)!;
			return attrib.Description;
		}
	}
}