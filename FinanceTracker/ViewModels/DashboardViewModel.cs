using FinanceTracker.Core.ViewModels;

namespace FinanceTracker.ViewModels
{
	public class DashboardViewModel : ViewModelBase
	{
		public DashboardViewModel() : base("Dashboard") { }

		protected override void BindCommands() { }

		protected override void BindMessages() { base.BindMessages(); }
	}
}
