using System;
using System.Windows;

namespace LogExplorer.Views.Controls
{
	public partial class RangeSlider
	{
		private const int borderPadding = 11;

		public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(0d));
		public double Minimum
		{
			get => (double)GetValue(MinimumProperty);
			set => SetValue(MinimumProperty, value);
		}

		public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(1d));
		public double Maximum
		{
			get => (double)GetValue(MaximumProperty);
			set => SetValue(MaximumProperty, value);
		}

		public static readonly DependencyProperty SliderSeparationProperty = DependencyProperty.Register("SliderSeparation", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(1d));
		public double SliderSeparation
		{
			get => (double)GetValue(SliderSeparationProperty);
			set => SetValue(SliderSeparationProperty, value);
		}

		public static readonly DependencyProperty LowerValueProperty = DependencyProperty.Register("LowerValue", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(0d));
		public double LowerValue
		{
			get => (double)GetValue(LowerValueProperty);
			set
			{
				if (value > Maximum - SliderSeparation)
				{
					value = Maximum - SliderSeparation;
				}
				double oldValue = LowerValue;
				SetValue(LowerValueProperty, value);
				LowerValueChanged?.Invoke(this, new RoutedPropertyChangedEventArgs<double>(oldValue, value));
				if (value > UpperValue - SliderSeparation)
				{
					UpperValue = Math.Min(Maximum, LowerValue + SliderSeparation);
				}
			}
		}

		public static readonly DependencyProperty UpperValueProperty = DependencyProperty.Register("UpperValue", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(0d));
		public double UpperValue
		{
			get => (double)GetValue(UpperValueProperty);
			set
			{
				if (value < Minimum + SliderSeparation)
				{
					value = Minimum + SliderSeparation;
				}
				double oldValue = UpperValue;
				SetValue(UpperValueProperty, value);
				UpperValueChanged?.Invoke(this, new RoutedPropertyChangedEventArgs<double>(oldValue, value));
				if (value < LowerValue + SliderSeparation)
				{
					LowerValue = Math.Max(Minimum, UpperValue - SliderSeparation);
				}
			}
		}

		public event EventHandler<RoutedPropertyChangedEventArgs<double>>? LowerValueChanged;
		public event EventHandler<RoutedPropertyChangedEventArgs<double>>? UpperValueChanged;

		public RangeSlider()
		{
			InitializeComponent();

			UpperSlider.ValueChanged += UpperSlider_ValueChanged;
			LowerSlider.ValueChanged += LowerSlider_ValueChanged;
		}

		private void LowerSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			LowerValue = e.NewValue;
			LowerSlider.Value = LowerValue;
			AdjustSliderRangeBorderValues();
		}

		private void UpperSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			UpperValue = e.NewValue;
			UpperSlider.Value = UpperValue;
			AdjustSliderRangeBorderValues();
		}

		private void AdjustSliderRangeBorderValues()
		{
			double width = BaseGrid.ActualWidth - borderPadding;
			double range = Maximum - Minimum;
			InnerBorder.Margin = OuterBorder.Margin = new Thickness(LowerValue / range * width + 10, 0, width - UpperValue / range * width + 10, 0);
		}
	}
}