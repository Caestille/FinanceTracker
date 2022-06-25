using System;
using System.Windows;
using System.Windows.Media;

namespace LogExplorer.Views.Controls
{
	public partial class CircularProgressBar
	{
		public CircularProgressBar()
		{
			InitializeComponent();
			Angle = (Percentage * 360) / 100;
			RenderArc();
		}

		public int Radius
		{
			get => (int)GetValue(RadiusProperty);
			set => SetValue(RadiusProperty, value);
		}

		public Brush SegmentColor
		{
			get => (Brush)GetValue(SegmentColorProperty);
			set => SetValue(SegmentColorProperty, value);
		}

		public int StrokeThickness
		{
			get => (int)GetValue(StrokeThicknessProperty);
			set => SetValue(StrokeThicknessProperty, value);
		}

		public double Percentage
		{
			get => (double)GetValue(PercentageProperty);
			set => SetValue(PercentageProperty, value);
		}

		public double Angle
		{
			get => (double)GetValue(AngleProperty);
			set => SetValue(AngleProperty, value);
		}

		// Using a DependencyProperty as the backing store for Percentage.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty PercentageProperty =
			DependencyProperty.Register("Percentage", typeof(double), typeof(CircularProgressBar), new PropertyMetadata(65d, OnPercentageChanged));

		// Using a DependencyProperty as the backing store for StrokeThickness.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty StrokeThicknessProperty =
			DependencyProperty.Register("StrokeThickness", typeof(int), typeof(CircularProgressBar), new PropertyMetadata(5, OnThicknessChanged));

		// Using a DependencyProperty as the backing store for SegmentColor.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty SegmentColorProperty =
			DependencyProperty.Register("SegmentColor", typeof(Brush), typeof(CircularProgressBar), new PropertyMetadata(new SolidColorBrush(Colors.Red), OnColorChanged));

		// Using a DependencyProperty as the backing store for Radius.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty RadiusProperty =
			DependencyProperty.Register("Radius", typeof(int), typeof(CircularProgressBar), new PropertyMetadata(25, OnPropertyChanged));

		// Using a DependencyProperty as the backing store for Angle.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty AngleProperty =
			DependencyProperty.Register("Angle", typeof(double), typeof(CircularProgressBar), new PropertyMetadata(120d, OnPropertyChanged));

		private static void OnColorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
		{
			CircularProgressBar? circle = sender as CircularProgressBar;
			circle?.set_Color((SolidColorBrush)args.NewValue);
		}

		private static void OnThicknessChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
		{
			CircularProgressBar? circle = sender as CircularProgressBar;
			circle?.set_tick((int)args.NewValue);
		}

		private static void OnPercentageChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
		{
			CircularProgressBar? circle = sender as CircularProgressBar;
			if (circle?.Percentage > 100) 
				circle.Percentage = 100;
			if (circle != null) 
				circle.Angle = (circle.Percentage * 360) / 100;
		}

		private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
		{
			CircularProgressBar? circle = sender as CircularProgressBar;
			circle?.RenderArc();
		}

		public void set_tick(int n)
		{
			PathRoot.StrokeThickness = n;
		}

		public void set_Color(SolidColorBrush n)
		{
			PathRoot.Stroke = n;
		}

		public void RenderArc()
		{
			Point startPoint = new Point(Radius, 0);
			Point endPoint = ComputeCartesianCoordinate(Angle, Radius);
			endPoint.X += Radius;
			endPoint.Y += Radius;

			PathRoot.Width = Radius * 2 + StrokeThickness;
			PathRoot.Height = Radius * 2 + StrokeThickness;
			PathRoot.Margin = new Thickness(StrokeThickness, StrokeThickness, 0, 0);

			bool largeArc = Angle > 180.0;

			Size outerArcSize = new Size(Radius, Radius);

			pathFigure.StartPoint = startPoint;

			if (Math.Abs(startPoint.X - Math.Round(endPoint.X)) < 0.00001 && Math.Abs(startPoint.Y - Math.Round(endPoint.Y)) < 0.00001)
				endPoint.X -= 0.01;

			arcSegment.Point = endPoint;
			arcSegment.Size = outerArcSize;
			arcSegment.IsLargeArc = largeArc;
		}

		private Point ComputeCartesianCoordinate(double angle, double radius)
		{
			// convert to radians
			double angleRad = (Math.PI / 180.0) * (angle - 90);

			double x = radius * Math.Cos(angleRad);
			double y = radius * Math.Sin(angleRad);

			return new Point(x, y);
		}
	}
}