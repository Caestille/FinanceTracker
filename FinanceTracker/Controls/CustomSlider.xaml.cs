using System;
using System.Diagnostics;
using System.Windows;

namespace LogExplorer.Views.Controls
{
	public partial class CustomSlider
	{
		public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(double), typeof(CustomSlider), new UIPropertyMetadata(0d));
		public double Minimum
		{
			get => (double)GetValue(MinimumProperty);
			set => SetValue(MinimumProperty, value);
		}

		public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(double), typeof(CustomSlider), new UIPropertyMetadata(1d));
		public double Maximum
		{
			get => (double)GetValue(MaximumProperty);
			set => SetValue(MaximumProperty, value);
		}

		public static readonly DependencyProperty LowerValueProperty = DependencyProperty.Register("LowerValue", typeof(double), typeof(CustomSlider), new UIPropertyMetadata(0d));
		public double LowerValue
		{
			get => (double)GetValue(LowerValueProperty);
			set
			{
				double oldValue = LowerValue;
				SetValue(LowerValueProperty, value);
				ValueChanged?.Invoke(this, new RoutedPropertyChangedEventArgs<double>(oldValue, value));
			}
		}

		public event EventHandler<RoutedPropertyChangedEventArgs<double>>? ValueChanged;

		public CustomSlider()
		{
			InitializeComponent();

			Slider.ValueChanged += Slider_ValueChanged;
		}

		private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			AdjustSliderRangeBorderValues();
		}

		private void AdjustSliderRangeBorderValues()
		{
			double width = BaseGrid.ActualWidth;
			double range = Maximum - Minimum;
			double rightPadding = (width - LowerValue / range * width);
			double factor = ((width - rightPadding) / width);
			rightPadding += (width * 0.1 * factor);
			if (double.IsNaN(rightPadding))
				rightPadding = 0;
			InnerBorder.Margin = OuterBorder.Margin = new Thickness(0, 0, rightPadding, 0);
		}
	}
}