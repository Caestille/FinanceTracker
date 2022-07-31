using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace FinanceTracker.Converters
{
	public class BankNameIconConverter : IValueConverter
	{
		List<KeyValuePair<string, string>> namesAndExtensions = new()
		{
			KeyValuePair.Create("Barclays", "svg"),
			KeyValuePair.Create("Halifax", "svg"),
			KeyValuePair.Create("Lloyds Tsb", "svg"),
			KeyValuePair.Create("Natwest", "svg"),
			KeyValuePair.Create("Rbs", "svg"),
			KeyValuePair.Create("Santander", "svg"),
			//KeyValuePair.Create("Vanguard", "png"),
			KeyValuePair.Create("Hsbc", "svg"),
		};

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var bankName = (string)value;
			KeyValuePair<string, string>? matchedBank = namesAndExtensions.FirstOrDefault(x => bankName?.IndexOf(x.Key, StringComparison.OrdinalIgnoreCase) != -1);
			if (matchedBank.Value.Key == null)
				return null;
			var uri = new Uri(@$"../Images/{matchedBank.Value.Key.Replace(" ", "")}.{matchedBank.Value.Value}", UriKind.Relative);
			return uri;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
