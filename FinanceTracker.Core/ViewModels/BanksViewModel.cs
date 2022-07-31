using CoreUtilities.HelperClasses;
using CoreUtilities.Interfaces;
using FinanceTracker.Core.Interfaces;
using FinanceTracker.Core.Messages;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace FinanceTracker.Core.ViewModels
{
	public class BanksViewModel : ViewModelBase
	{
		public override RangeObservableCollection<BankViewModel> BankData 
		{ 
			get => new RangeObservableCollection<BankViewModel>(ChildViewModels.Select(x => (BankViewModel)x)); 
			set
			{
				ChildViewModels.Clear();
				ChildViewModels.AddRange(value);
			}
		}

		public BanksViewModel(IBankApiService bankApiService, IRegistryService registryService)
			: base("Banks", new Func<ViewModelBase>(
				() => new BankViewModel(bankApiService, registryService, "Unnamed Bank", Guid.NewGuid()))) 
		{
			var existingBanks = registryService.GetAllSettingsInPath(@"\Banks");
			foreach (var kvp in existingBanks)
			{
				ChildViewModels.Add(new BankViewModel(bankApiService, registryService, kvp.Value.ToString(), Guid.Parse(kvp.Key)));
			}
			NotifyBanksChanged();

			if (ChildViewModels.Any())
			{
				IsShowingChildren = true;
			}
		}

		public override void AddChild(ViewModelBase vmToAdd = null, string name = "")
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
