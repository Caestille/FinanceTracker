using FinanceTracker.Core.ViewModels;

namespace FinanceTracker.Core.Messages
{
	public class ViewModelRequestDeleteMessage
	{
		public ViewModelBase ViewModel { get; protected set; }

		public ViewModelRequestDeleteMessage(ViewModelBase viewModelToDelete)
		{
			ViewModel = viewModelToDelete;
		}
	}
}
