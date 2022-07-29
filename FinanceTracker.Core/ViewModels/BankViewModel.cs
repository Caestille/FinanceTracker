using Microsoft.Toolkit.Mvvm.Input;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using FinanceTracker.Core.Messages;
using FinanceTracker.Core.Interfaces;
using CoreUtilities.Interfaces;
using FinanceTracker.Core.Models;

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
		public ICommand CancelTaskCommand => new RelayCommand(CancelTask);
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

		private CancellationTokenSource cancellationTokenSource;
		public CancellationTokenSource CancellationTokenSource
		{
			get => cancellationTokenSource;
			set => SetProperty(ref cancellationTokenSource, value);
		}

		public BankViewModel(IBankApiService bankApiService, IRegistryService registryService, string name, Guid? guid = null) : base(name, new Func<ViewModelBase>(() => new AccountViewModel("Unnamed Account")))
		{
			SupportsDeleting = true;

			this.registryService = registryService;
			truelayerService = bankApiService;
			truelayerService.NewBankLinkStatusForGuid += TruelayerService_NewBankLinkStatusForGuid;
			bankGuid = guid ?? Guid.NewGuid();
			registryService.SetSetting(bankGuid.ToString(), Name, @"\Banks");
			if (guid != null)
			{
				truelayerService.ReloadBankLinkDetails(guid.Value);
			}
		}

		private void TruelayerService_NewBankLinkStatusForGuid(object? sender, (Guid, BankLinkStatus) e)
		{
			if (e.Item1 == bankGuid)
			{
				LinkStatus = e.Item2;
			}
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
			var results = new List<TransactionModel>();
			var accounts = await truelayerService.GetAccounts(bankGuid);
			foreach (var account in accounts)
			{
				results.AddRange(await truelayerService.GetTransactions(account));
			}
		}

		private void CancelTask()
		{
			CancellationTokenSource.Cancel();
		}
	}
}
