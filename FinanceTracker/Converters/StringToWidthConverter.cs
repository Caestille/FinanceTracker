using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace FinanceTracker.Converters
{
	public class StringToWidthConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			string text = (string)values[0];
			double fontSize = (double)values[1];
			FontFamily fontFamily = (FontFamily)values[2];
			FontStyle fontStyle = (FontStyle)values[3];
			FontWeight fontWeight = (FontWeight)values[4];
			FontStretch fontStretch = (FontStretch)values[5];

			double width = MeasureString(text, fontSize, fontFamily, fontStyle, fontWeight, fontStretch);

			return width + 30;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			return new[] { Binding.DoNothing, Binding.DoNothing, Binding.DoNothing, Binding.DoNothing, Binding.DoNothing, Binding.DoNothing };
		}

		private double MeasureString(string? text, double fontSize, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch)
		{
			if (text != null)
			{
#pragma warning disable CS0618
				var formattedText = new FormattedText(
#pragma warning restore CS0618
					text,
					CultureInfo.CurrentCulture,
					FlowDirection.LeftToRight,
					new Typeface(fontFamily, fontStyle, fontWeight, fontStretch),
					fontSize,
					Brushes.Black);

				return formattedText.Width;
			}

			return 0;
		}
	}
}