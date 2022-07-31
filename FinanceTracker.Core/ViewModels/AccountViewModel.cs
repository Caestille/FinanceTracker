using FinanceTracker.Core.Models;
using CoreUtilities.HelperClasses;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;
using FinanceTracker.Core.Messages;
using Microsoft.Toolkit.Mvvm.Messaging;
using FinanceTracker.Core.Interfaces;
using CoreUtilities.Interfaces;

namespace FinanceTracker.Core.ViewModels
{
	public class AccountViewModel : ViewModelBase
	{
		private IBankApiService truelayerService;
		private IRegistryService registryService;

		private const string defaultAccountName = "Unnamed Account";
		private Guid accountGuid;
		private Guid parentBankGuid;

		public ICommand SetVisibleAccountCommand => new RelayCommand(Select);
		public ICommand RequestCloseCommand => new RelayCommand(RequestClose);
		public ICommand EditNameCommand => new RelayCommand(EditName);
		public ICommand NameEditorKeyDownCommand => new RelayCommand<object>(NameEditorKeyDown);

		private RangeObservableCollection<TransactionModel> transactions = new();
		public RangeObservableCollection<TransactionModel> Transactions
		{
			get => transactions;
			set => SetProperty(ref transactions, value);
		}

		private bool isCreditCard;
		public bool IsCreditCard
		{
			get => isCreditCard;
			set => SetProperty(ref isCreditCard, value);
		}

		private bool isEditingName;
		public bool IsEditingName
		{
			get => isEditingName;
			set => SetProperty(ref isEditingName, value);
		}

		private string temporaryName;
		public string TemporaryName
		{
			get => temporaryName;
			set => SetProperty(ref temporaryName, value);
		}

		private double total;
		public double Total
		{
			get => total;
			set => SetProperty(ref total, value);
		}

		private string originalName;
		public string OriginalName
		{
			get => originalName;
			set
			{
				originalName = value;
				registryService.SetSetting(accountGuid.ToString(), $"{Name}|{OriginalName}", $@"\Banks\{parentBankGuid}\Accounts");
			}
		}

		public AccountViewModel(IBankApiService bankApiService, IRegistryService registryService, string name, Guid parentBankGuid, Guid accountGuid) : base(name)
		{
			this.registryService = registryService;
			truelayerService = bankApiService;

			this.accountGuid = accountGuid;
			this.parentBankGuid = parentBankGuid;
			OriginalName = name;

			SupportsDeleting = true; 
		}

		public async Task DownloadTransactions()
		{
			var what = await truelayerService.GetTransactions(parentBankGuid, OriginalName);
			Total = what.First().RunningBalance.Amount;
		}

		protected override void Select()
		{
			Messenger.Send(new AccountViewModelRequestShowMessage(this));
			//var parent = BankData.First(x => x.ChildViewModels.Any(y => y.Name == this.Name));
			//Messenger.Send(new ViewModelRequestShowMessage(parent));
			this.IsSelected = true;
		}

		protected override void RequestDelete()
		{
			registryService.DeleteSetting(accountGuid.ToString(), $@"\Banks\{parentBankGuid}\Accounts");
			base.RequestDelete();
		}

		private void EditName()
		{
			if (!IsEditingName)
			{
				TemporaryName = Name == defaultAccountName ? string.Empty : Name;
				IsEditingName = true;
			}
			else
			{
				Name = TemporaryName;
				IsEditingName = false;
			}
		}

		private void NameEditorKeyDown(object args)
		{
			if (args is KeyEventArgs e && (e.Key == Key.Enter || e.Key == Key.Escape))
			{
				if (e.Key == Key.Enter)
				{
					Name = TemporaryName;
					registryService.SetSetting(accountGuid.ToString(), $"{Name}|{OriginalName}", $@"\Banks\{parentBankGuid}\Accounts");
				}

				IsEditingName = false;
			}
		}

		private void RequestClose()
		{
			Messenger.Send(new AccountViewModelRequestShowMessage(null));
		}
	}
}
