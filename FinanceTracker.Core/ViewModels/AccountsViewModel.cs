using CoreUtilities.Interfaces;
using FinanceTracker.Core.Interfaces;

namespace FinanceTracker.Core.ViewModels
{
	public class AccountsViewModel : ViewModelBase
	{
		public AccountsViewModel(IBankApiService bankApiService, IRegistryService registryService, string name, string defaultChildName, Guid parentBankGuid, bool holdsCreditCards) 
			: base(name, () => new AccountViewModel(bankApiService, registryService, defaultChildName, parentBankGuid, Guid.NewGuid(), holdsCreditCards))
		{
			var existingAccounts = registryService.GetAllSettingsInPath($@"\Banks\{parentBankGuid}\Accounts");
			foreach (var kvp in existingAccounts)
			{
				var loadedName = kvp.Value.ToString().Split('|')[0];
				var originalName = kvp.Value.ToString().Split('|')[1];
				var isCreditCard = bool.Parse(kvp.Value.ToString().Split('|')[2]);
				if (isCreditCard == holdsCreditCards)
				{
					var vm = new AccountViewModel(bankApiService, registryService, loadedName, parentBankGuid, Guid.Parse(kvp.Key), isCreditCard);
					vm.OriginalName = originalName;
					AddChild(vm);
				}
			}
		}
	}
}
