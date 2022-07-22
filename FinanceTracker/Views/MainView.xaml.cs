using CoreUtilities.Controls;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;

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
			await System.Threading.Tasks.Task.Run(() => Thread.Sleep(300));
			BindingExpression binding = Blurrer.GetBindingExpression(BlurHost.BlurEnabledProperty);
			Binding parentBinding = binding.ParentBinding;
			Blurrer.BlurEnabled = true;
			Blurrer.DrawBlurredElementBackground();
			Blurrer.SetBinding(BlurHost.BlurEnabledProperty, parentBinding);
			this.Loaded -= MainView_Loaded;
		}
	}
}
