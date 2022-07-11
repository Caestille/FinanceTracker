using FinanceTracker.Core.ViewModels;

namespace FinanceTracker.Core.Messages
{
	public class ViewModelRequestShowMessage
	{
		public ViewModelBase ViewModel { get; private set; }

		public ViewModelRequestShowMessage(ViewModelBase viewModelToShow)
		{
			ViewModel = viewModelToShow;
		}
	}
}
