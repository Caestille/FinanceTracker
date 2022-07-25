using FinanceTracker.Core.ViewModels;

namespace FinanceTracker.Core.Messages
{
	public class AccountViewModelRequestShowMessage : ViewModelRequestShowMessage
	{
		public AccountViewModelRequestShowMessage(ViewModelBase viewModelToShow) : base(viewModelToShow)
		{
			ViewModel = viewModelToShow;
		}
	}
}
