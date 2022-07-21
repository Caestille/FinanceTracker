using System.Threading;
using System.Windows.Controls;

namespace FinanceTracker.Views
{
	/// <summary>
	/// Interaction logic for MainView.xaml
	/// </summary>
	public partial class MainView : UserControl
	{
		public MainView()
		{
			InitializeComponent();
			this.Loaded += MainView_Loaded;
		}

		private async void MainView_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
			await System.Threading.Tasks.Task.Run(() => Thread.Sleep(1000));
			BlurHost.DrawBlurredElementBackground();
			this.Loaded -= MainView_Loaded;
		}
	}
}
