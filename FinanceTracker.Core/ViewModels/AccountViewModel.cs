using FinanceTracker.Core.Models;
using CoreUtilities.HelperClasses;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace FinanceTracker.Core.ViewModels
{
	public class AccountViewModel : ObservableObject
	{
		private string name;
		public string Name
		{
			get => name;
			set => SetProperty(ref name, value);
		}

		private RangeObservableCollection<TransactionModel> transactions = new();
		public RangeObservableCollection<TransactionModel> Transactions
		{
			get => transactions;
			set => SetProperty(ref transactions, value);
		}
	}
}
