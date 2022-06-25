using System;
using System.Globalization;
using System.Windows.Data;

namespace FinanceTracker.Converters
{
	public class ValueHalverConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (double)value * 0.5;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (double)value * 2;
		}
	}
}