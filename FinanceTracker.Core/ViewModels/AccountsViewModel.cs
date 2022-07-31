using CoreUtilities.Interfaces;
using FinanceTracker.Core.Interfaces;

namespace FinanceTracker.Core.ViewModels
{
	public class AccountsViewModel : ViewModelBase
	{
		public AccountsViewModel(IBankApiService bankApiService, IRegistryService registryService, string name, string defaultChildName, Guid parentBankGuid, bool holdsCreditCards) 
			: base(name, () => new AccountViewModel(bankApiService, registryService, defaultChildName, parentBankGuid, Guid.NewGuid()))
		{
			var existingAccounts = registryService.GetAllSettingsInPath($@"\Banks\{parentBankGuid}\Accounts");
			foreach (var kvp in existingAccounts)
			{
				var vm = new AccountViewModel(bankApiService, registryService, kvp.Value.ToString().Split('|').First(), parentBankGuid, Guid.Parse(kvp.Key));
				vm.OriginalName = kvp.Value.ToString().Split('|').Last();
				vm.IsCreditCard = false;
				if (vm.IsCreditCard == holdsCreditCards)
				{
					AddChild(vm);
				}
			}
		}
	}
}
