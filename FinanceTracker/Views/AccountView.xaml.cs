using System.Windows;
using System.Windows.Controls;

namespace FinanceTracker.Views
{
	/// <summary>
	/// Interaction logic for AccountView.xaml
	/// </summary>
	public partial class AccountView : UserControl
	{
		public AccountView()
		{
			InitializeComponent();
			NameTextbox.IsVisibleChanged += NameTextbox_IsVisibleChanged;
		}

		private void NameTextbox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (NameTextbox.Visibility == Visibility.Visible)
			{
				NameTextbox.Focus();
				NameTextbox.SelectAll();
			}
		}
	}
}
