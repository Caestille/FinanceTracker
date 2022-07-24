using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FinanceTracker.Views
{
	/// <summary>
	/// Interaction logic for BankView.xaml
	/// </summary>
	public partial class BankView : UserControl
	{
		public BankView()
		{
			InitializeComponent();
			this.SizeChanged += BankView_SizeChanged;
		}

		private void BankView_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			var relativePoint = (sender as Visual)?.TransformToAncestor(Application.Current.MainWindow)
				.Transform(new Point(0, 0));

			if (relativePoint == null)
				return;

			BlurHost.OffsetX = relativePoint.Value.X;
			BlurHost.DrawBlurredElementBackground();

			if (relativePoint.Value.X > 40)
			{
				ShadowGrid.Margin = new Thickness(-1, 0, 0, 0);
			}
			else
			{
				ShadowGrid.Margin = new Thickness(-35, 0, 0, 0);
			}
		}

		private void NameTextbox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var textbox = sender as TextBox;
			if (textbox != null && (textbox.Visibility == Visibility.Visible))
			{
				textbox.Width = 150;
				textbox.Focus();
				textbox.SelectAll();
			}
		}
	}
}
