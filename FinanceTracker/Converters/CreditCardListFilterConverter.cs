using CoreUtilities.HelperClasses;
using FinanceTracker.Core.ViewModels;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace FinanceTracker.Converters
{
	public class CreditCardListFilterConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var vms = (RangeObservableCollection<ViewModelBase>)value;
			var allow = bool.Parse((string)parameter);
			return vms.Where(x => (x as AccountViewModel).IsCreditCard == allow);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
