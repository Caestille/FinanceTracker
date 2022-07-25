using FinanceTracker.Core.Models;
using CoreUtilities.HelperClasses;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;
using FinanceTracker.Core.Messages;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace FinanceTracker.Core.ViewModels
{
	public class AccountViewModel : ViewModelBase
	{
		private const string defaultBankName = "Unnamed Account";

		public ICommand SetVisibleAccountCommand => new RelayCommand(SetVisibleAccount);
		public ICommand RequestCloseCommand => new RelayCommand(RequestClose);
		public ICommand EditNameCommand => new RelayCommand(EditName);
		public ICommand NameEditorKeyDownCommand => new RelayCommand<object>(NameEditorKeyDown);

		private RangeObservableCollection<TransactionModel> transactions = new();
		public RangeObservableCollection<TransactionModel> Transactions
		{
			get => transactions;
			set => SetProperty(ref transactions, value);
		}

		private bool isEditingName;
		public bool IsEditingName
		{
			get => isEditingName;
			set => SetProperty(ref isEditingName, value);
		}

		private string? temporaryName;
		public string? TemporaryName
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

		public AccountViewModel(string name) : base(name) { SupportsDeleting = true; }

		private void EditName()
		{
			if (!IsEditingName)
			{
				TemporaryName = Name == defaultBankName ? string.Empty : Name;
				IsEditingName = true;
			}
			else
			{
				if (TemporaryName != null)
					Name = TemporaryName;
				IsEditingName = false;
			}
		}

		private void NameEditorKeyDown(object? args)
		{
			if (args != null && args is KeyEventArgs e && (e.Key == Key.Enter || e.Key == Key.Escape))
			{
				if (e.Key == Key.Enter && TemporaryName != null)
				{
					Name = TemporaryName;
				}

				IsEditingName = false;
			}
		}

		private void SetVisibleAccount()
		{
			Messenger.Send(new AccountViewModelRequestShowMessage(this));
		}

		private void RequestClose()
		{
			Messenger.Send(new AccountViewModelRequestShowMessage(null));
		}

		protected override void Select()
		{
			var parent = BankData.First(x => x.ChildViewModels.Any(y => y.Name == this.Name));
			Messenger.Send(new ViewModelRequestShowMessage(parent));
			parent.VisibleAccount = this;
		}
	}
}
