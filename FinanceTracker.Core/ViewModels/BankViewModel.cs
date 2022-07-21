using Microsoft.Toolkit.Mvvm.Input;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using FinanceTracker.Core.Messages;

namespace FinanceTracker.Core.ViewModels
{
	public class BankViewModel : ViewModelBase
	{
		private const string defaultBankName = "Unnamed Bank";

		public ICommand EditNameCommand => new RelayCommand(EditName);
		public ICommand NameEditorKeyDownCommand => new RelayCommand<object>(NameEditorKeyDown); 

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

		public BankViewModel(string name) : base(name, new Func<ViewModelBase>(() => new AccountViewModel("Unnamed Account")))
		{

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
				}

				IsEditingName = false;
			}
		}

		protected override void BindMessages()
		{
			Messenger.Register<AccountViewModelRequestShowMessage>(this, (sender, message) => { VisibleAccount = message.ViewModel; });
			Messenger.Register<ViewModelRequestDeleteMessage>(this, (sender, message) => 
			{ 
				if (ChildViewModels.Contains(message.ViewModel))
				{
					ChildViewModels.Remove(message.ViewModel);
				}
			});
			base.BindMessages();
		}
	}
}
