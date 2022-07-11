using CoreUtilities.HelperClasses;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace FinanceTracker.Core.ViewModels
{
	public class BankViewModel : ObservableObject
	{
		private string name;
		public string Name
		{
			get => name;
			set => SetProperty(ref name, value);
		}

		private RangeObservableCollection<AccountViewModel> accounts = new();
		public RangeObservableCollection<AccountViewModel> Accounts
		{
			get => accounts;
			set => SetProperty(ref accounts, value);
		}
	}
}
