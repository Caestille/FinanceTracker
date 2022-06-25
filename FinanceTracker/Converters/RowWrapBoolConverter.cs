using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace FinanceTracker.Converters
{
	public class RowWrapBoolConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if ((bool)value)
				return new DataGridLength(1, DataGridLengthUnitType.Star);
			
			return new DataGridLength(1, DataGridLengthUnitType.Auto);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Binding.DoNothing;
		}
	}
}