using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace FinanceTracker.Converters
{
	public class CurrencyFormatterConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var val = $"£ {Math.Round((double)value, 2)}";
			var parts = val.Split('.');
			if (parts.Count() == 1)
			{
				return val + ".00";
			}
			else if (parts.Count() == 2)
			{
				var decimals = parts.Last();
				if (decimals.Length == 1)
				{
					decimals += "0";
				}

				return string.Join('.', new string[] { parts[0], decimals });
			}
			return string.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
