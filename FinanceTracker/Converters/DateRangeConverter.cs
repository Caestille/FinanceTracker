using System;
using System.Globalization;
using System.Windows.Data;

namespace FinanceTracker.Converters
{
	public class DateRangeConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			DateTime date1 = (DateTime)values[0];
			DateTime date2 = (DateTime)values[1];

			return $"{date1} - {date2}";
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			return new[] { Binding.DoNothing, Binding.DoNothing };
		}
	}
}