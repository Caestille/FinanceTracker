using Microsoft.Toolkit.Mvvm.Input;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using FinanceTracker.Core.Messages;
using FinanceTracker.Core.Interfaces;
using CoreUtilities.Interfaces;
using FinanceTracker.Core.Models;
using System.Windows;
using CoreUtilities.HelperClasses;
using System.Collections.ObjectModel;

namespace FinanceTracker.Core.ViewModels
{
	public class BankViewModel : ViewModelBase
	{
		private IBankApiService truelayerService;
		private IRegistryService registryService;

		private const string defaultBankName = "Unnamed Bank";
		private Guid bankGuid;

		public ICommand EditNameCommand => new RelayCommand(EditName);
		public ICommand NameEditorKeyDownCommand => new RelayCommand<object>(NameEditorKeyDown);
		public ICommand CancelBankLinkCommand => new RelayCommand(CancelBankLink);
		public ICommand LinkBankCommand => new AsyncRelayCommand(async () => { await LinkBank(); });
		public ICommand UnlinkBankCommand => new AsyncRelayCommand(async () => { await UnlinkBank(); });
		public ICommand DownloadDataCommand => new AsyncRelayCommand(async () => { await DownloadData(); });

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

		public override bool SupportsAddingChildren => false;

		private ViewModelBase visibleAccount;
		public ViewModelBase VisibleAccount
		{
			get => visibleAccount;
			set => SetProperty(ref visibleAccount, value);
		}

		private BankLinkStatus linkStatus;
		public BankLinkStatus LinkStatus
		{
			get => linkStatus;
			set => SetProperty(ref linkStatus, value);
		}

		private bool isDownloadingData;
		public bool IsDownloadingData
		{
			get => isDownloadingData;
			set => SetProperty(ref isDownloadingData, value);
		}

		private CancellationTokenSource cancellationTokenSource;
		public CancellationTokenSource CancellationTokenSource
		{
			get => cancellationTokenSource;
			set => SetProperty(ref cancellationTokenSource, value);
		}

		private AccountsViewModel accountsViewModel;
		public AccountsViewModel AccountsViewModel
		{
			get => accountsViewModel;
			set => SetProperty(ref accountsViewModel, value);
		}

		private AccountsViewModel creditCardsViewModel;
		public AccountsViewModel CreditCardsViewModel
		{
			get => creditCardsViewModel;
			set => SetProperty(ref creditCardsViewModel, value);
		}


		public BankViewModel(IBankApiService bankApiService, IRegistryService registryService, string name, Guid bankGuid) : base(name, new Func<ViewModelBase>(() => new AccountViewModel(bankApiService, registryService, "Unnamed Account", bankGuid, Guid.NewGuid())))
		{
			SupportsDeleting = true;

			this.registryService = registryService;
			truelayerService = bankApiService;
			truelayerService.NewBankLinkStatusForGuid += TruelayerService_NewBankLinkStatusForGuid;
			this.bankGuid = bankGuid;
			registryService.SetSetting(bankGuid.ToString(), Name, @"\Banks");

			AccountsViewModel = new AccountsViewModel(bankApiService, registryService, "Accounts", "Unnamed Account", bankGuid, false);
			CreditCardsViewModel = new AccountsViewModel(bankApiService, registryService, "Credit Cards", "Unnamed Credit Card", bankGuid, true);

			AddChild(AccountsViewModel);
			AddChild(CreditCardsViewModel);

			truelayerService.TryReloadBankLinkDetails(bankGuid);

			Application.Current.Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
		}

		protected override void BindMessages()
		{
			Messenger.Register<AccountViewModelRequestShowMessage>(this, (sender, message) => { VisibleAccount = message.ViewModel; });
			base.BindMessages();
		}

		protected override void RequestDelete()
		{
			truelayerService.DeleteLink(bankGuid);
			registryService.DeleteSetting(bankGuid.ToString(), @"\Banks");
			base.RequestDelete();
		}

		protected override void OnViewModelDelete(ViewModelBase viewModel)
		{
			if (VisibleAccount == viewModel)
			{
				VisibleAccount = null;
			}
			base.OnViewModelDelete(viewModel);
		}

		private void EditName()
        {
			if (!IsEditingName)
			{
				TemporaryName = Name == defaultBankName ? string.Empty : Name;
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
					registryService.SetSetting(bankGuid.ToString(), Name, @"\Banks");
				}

				IsEditingName = false;
			}
		}

		private async Task LinkBank()
		{
			if (LinkStatus != BankLinkStatus.NotLinked)
				return;

			CancellationTokenSource = new CancellationTokenSource();
			var result = await truelayerService.LinkBank(bankGuid, cancellationTokenSource.Token);
			await Task.Delay(2000);
		}

		private async Task UnlinkBank()
		{
			await truelayerService.DeleteLink(bankGuid);
		}

		private async Task DownloadData()
		{
			IsDownloadingData = true;
			var results = new List<TransactionModel>();
			var accounts = await truelayerService.GetAccounts(bankGuid);
			await Parallel.ForEachAsync(accounts, async (account, token) =>
			{
				try
				{
					var name = account.DisplayName;
					var childrenContainsAccount = ChildViewModels.Any(x => (x as AccountViewModel).OriginalName == name);
					AccountViewModel? accountVm = Application.Current.Dispatcher.Invoke(() =>
						childrenContainsAccount 
							? ChildViewModels.First(x => (x as AccountViewModel).OriginalName == name) as AccountViewModel
							: new AccountViewModel(truelayerService, registryService, name, bankGuid, Guid.NewGuid()));
					if (!childrenContainsAccount && accountVm != null)
					{
						Application.Current.Dispatcher.Invoke(() => AddChild(accountVm));
					}
					accountVm?.DownloadTransactions();
				}
				catch { }
			});
			IsDownloadingData = false;
		}

		private void CancelBankLink()
		{
			CancellationTokenSource.Cancel();
		}

		private void TruelayerService_NewBankLinkStatusForGuid(object? sender, (Guid, BankLinkStatus) e)
		{
			if (e.Item1 == bankGuid)
			{
				LinkStatus = e.Item2;
			}
		}

		private void Dispatcher_ShutdownStarted(object? sender, EventArgs e)
		{
			truelayerService.Dispose();
		}
	}
}
