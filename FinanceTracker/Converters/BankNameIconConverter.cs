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
		List<string> availableBankNames = new()
		{
			"Barclays",
			"Halifax",
			"Lloyds Tsb",
			"Natwest",
			"Rbs",
			"Santander",
			"Vanguard",
			"Hsbc",
		};

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var bankName = (string)value;
			var matchedBank = availableBankNames.FirstOrDefault(x => bankName?.IndexOf(x, StringComparison.OrdinalIgnoreCase) != -1);
			if (matchedBank == null)
				return Binding.DoNothing;
			var uri = new Uri(@$"../Images/{matchedBank?.Replace(" ", "")}.png", UriKind.Relative);
			var bitmap = new BitmapImage(uri);
			return bitmap;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
