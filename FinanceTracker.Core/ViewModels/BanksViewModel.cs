using CoreUtilities.HelperClasses;
using FinanceTracker.Core.Messages;
using FinanceTracker.Core.ViewModels;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace FinanceTracker.Core.ViewModels
{
	public class BanksViewModel : ViewModelBase
	{
		public BanksViewModel() : base("Banks", new Func<ViewModelBase>(() => new BankViewModel("Unnamed Bank"))) { }

		public override RangeObservableCollection<BankViewModel> BankData 
		{ 
			get => new RangeObservableCollection<BankViewModel>(ChildViewModels.Select(x => (BankViewModel)x)); 
			set
			{
				ChildViewModels.Clear();
				ChildViewModels.AddRange(value);
			}
		}

		protected override void AddChild()
		{
			base.AddChild();
			NotifyBanksChanged();
		}

		protected override void BindMessages()
		{
			Messenger.Register<RequestSyncBankDataMessage>(this, (sender, message) =>
			{
				NotifyBanksChanged();
			});
			base.BindMessages();
		}
	}
}
