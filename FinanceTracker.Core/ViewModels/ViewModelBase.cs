using CoreUtilities.HelperClasses;
using FinanceTracker.Core.Messages;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace FinanceTracker.Core.ViewModels
{
	public class ViewModelBase : ObservableRecipient
	{
		private Func<ViewModelBase> createChildFunc;

		public ICommand SelectCommand => new RelayCommand(Select);
		public ICommand AddChildCommand => new RelayCommand(() => AddChild());
		public ICommand RequestDeleteCommand => new RelayCommand(RequestDelete);

		private string name;
		public string Name
		{
			get => name;
			set => SetProperty(ref name, value.TrimStart());
		}

		private RangeObservableCollection<ViewModelBase> childViewModels = new();
		public RangeObservableCollection<ViewModelBase> ChildViewModels
		{
			get => childViewModels;
			set => SetProperty(ref childViewModels, value);
		}

		private RangeObservableCollection<BankViewModel> bankData = new();
		public virtual RangeObservableCollection<BankViewModel> BankData
		{
			get => bankData;
			set => SetProperty(ref bankData, value);
		}

		private bool isSelected;
		public bool IsSelected
		{
			get => isSelected;
			set => SetProperty(ref isSelected, value);
		}

		public virtual bool SupportsAddingChildren => createChildFunc != null;

		private bool supportsDeleting;
		public bool SupportsDeleting
		{
			get => supportsDeleting;
			set => SetProperty(ref supportsDeleting, value);
		}

		private bool isShowingChildren;
		public bool IsShowingChildren
		{
			get => isShowingChildren;
			set => SetProperty(ref isShowingChildren, value);
		}

		private int level = 0;
		public int Level
		{
			get => level;
			set => SetProperty(ref level, value);
		}

		protected IMessenger BaseMessenger => Messenger;

		public ViewModelBase(string name, Func<ViewModelBase> createChild = null)
		{
			Name = name;
			createChildFunc = createChild;

			BindMessages();

			Messenger.Send(new RequestSyncBankDataMessage());
		}

		protected virtual void BindMessages() 
		{
			Messenger.Register<ViewModelRequestShowMessage>(this, (sender, message) => 
			{
				OnViewModelRequestShow(message.ViewModel);
			});

			Messenger.Register<BanksUpdatedMessage>(this, (sender, message) =>
			{
				BankData = new RangeObservableCollection<BankViewModel>(message.Banks);
			});

			Messenger.Register<ViewModelRequestDeleteMessage>(this, (sender, message) =>
			{
				OnViewModelDelete(message.ViewModel);
			});
		}

		protected virtual void OnViewModelRequestShow(ViewModelBase viewModel)
		{
			if (viewModel != this)
			{
				IsSelected = false;
			}
		}

		protected virtual void OnViewModelDelete(ViewModelBase viewModel)
		{
			if (ChildViewModels.Contains(viewModel))
			{
				ChildViewModels.Remove(viewModel);

				if (IsShowingChildren && !ChildViewModels.Any())
				{
					IsShowingChildren = false;
				}

				OnPropertyChanged(nameof(ChildViewModels));
			}
		}

		protected virtual void Select()
		{
			if (ChildViewModels.Count != 0 && SupportsAddingChildren)
			{
				IsShowingChildren = !IsShowingChildren;
			}
			else if (!SupportsAddingChildren)
			{
				Messenger.Send(new ViewModelRequestShowMessage(this));
			}
		}

		protected virtual void AddChild(ViewModelBase viewModelToAdd = null, string name = "") 
		{
			var viewModel = viewModelToAdd ?? createChildFunc();
			if (name != string.Empty)
			{
				viewModel.Name = name;
			}

			ChildViewModels.Add(viewModel);

			foreach (var vm in ChildViewModels)
			{
				vm.SetLevel(level + 1);
			}

			if (SupportsAddingChildren)
			{
				IsShowingChildren = true;
				ChildViewModels.Last().Select();
			}

			OnPropertyChanged(nameof(ChildViewModels));
		}

		protected void SetLevel(int level)
		{
			Level = level;
			foreach (var vm in ChildViewModels)
			{
				vm.SetLevel(level + 1);
			}
		}

		protected void NotifyBanksChanged()
		{
			Messenger.Send(new BanksUpdatedMessage(this, BankData));
		}

		protected void GetChildren(ref RangeObservableCollection<ViewModelBase> result, bool recurse)
		{
			result.AddRange(ChildViewModels);

			if (!recurse)
				return;

			foreach (var childVm in ChildViewModels)
			{
				childVm.GetChildren(ref result, true);
			}
		}

		protected virtual void RequestDelete()
		{
			Messenger.Send(new ViewModelRequestDeleteMessage(this));
		}
	}
}
