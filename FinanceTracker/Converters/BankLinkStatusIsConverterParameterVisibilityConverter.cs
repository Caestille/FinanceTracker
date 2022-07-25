using FinanceTracker.Core.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FinanceTracker.Converters
{
	public class BankLinkStatusIsConverterParameterVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var match = (BankLinkStatus)value == (BankLinkStatus)parameter;
			return match ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
