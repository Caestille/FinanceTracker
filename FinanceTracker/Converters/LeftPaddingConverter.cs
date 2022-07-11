using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FinanceTracker.Converters
{
	public class LeftPaddingConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return new Thickness((int)value + 1 * Double.Parse((string)parameter), 0, 0, 0);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
