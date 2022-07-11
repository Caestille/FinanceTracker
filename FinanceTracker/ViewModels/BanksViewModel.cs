using FinanceTracker.Core.ViewModels;

namespace FinanceTracker.ViewModels
{
	public class BanksViewModel : ViewModelBase
	{
		public BanksViewModel() : base("Banks") 
		{
			SupportsAddingChildren = true;
		}

		protected override void BindCommands() { }

		protected override void BindMessages() { base.BindMessages(); }
	}
}
