using FinanceTracker.Core.ViewModels;

namespace FinanceTracker.Core.Messages
{
	public class BanksUpdatedMessage
	{
		public IEnumerable<BankViewModel> Banks { get; private set; }
		public ViewModelBase Sender { get; private set; }

		public BanksUpdatedMessage(ViewModelBase sender, IEnumerable<BankViewModel> banks)
		{
			Sender = sender;
			Banks = banks;
		}
	}
}
