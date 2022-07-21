using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FinanceTracker.Converters
{
	public class MultiBoolAndToVisibilityConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			bool allow = true;
			foreach (object value in values)
			{
				if (value is bool castValue)
				{
					allow &= castValue;
				}
			}
			return allow ? Visibility.Visible : Visibility.Collapsed;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			return new[] { Binding.DoNothing };
		}
	}
}