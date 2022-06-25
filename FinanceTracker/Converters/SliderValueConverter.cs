using System;
using System.Globalization;
using System.Windows.Data;

namespace FinanceTracker.Converters
{
	public class SliderValueConverter : IMultiValueConverter
	{
		public DateTime LowerLimit { get; private set; }
		public DateTime UpperLimit { get; private set; }

		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			LowerLimit = (DateTime)values[0];
			UpperLimit = (DateTime)values[1];
			DateTime value = (DateTime)values[2];

			return (value - LowerLimit).TotalMilliseconds;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			double valueMilliSeconds = (double)value;
			DateTime toReturn = LowerLimit + TimeSpan.FromMilliseconds(valueMilliSeconds);
			return new []{ Binding.DoNothing, Binding.DoNothing, toReturn };
		}
	}
}