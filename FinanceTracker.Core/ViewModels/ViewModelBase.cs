using CoreUtilities.HelperClasses;
using FinanceTracker.Core.Messages;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System.Windows.Input;

namespace FinanceTracker.Core.ViewModels
{
	public class ViewModelBase : ObservableRecipient
	{
		private Func<ViewModelBase> createChildFunc;

		private string name;
		public string Name
		{
			get => name;
			set => SetProperty(ref name, value);
		}

		private RangeObservableCollection<ViewModelBase> childViewModels = new();
		public RangeObservableCollection<ViewModelBase> ChildViewModels
		{
			get => childViewModels;
			set => SetProperty(ref childViewModels, value);
		}

		private bool isSelected;
		public bool IsSelected
		{
			get => isSelected;
			set => SetProperty(ref isSelected, value);
		}

		public bool SupportsAddingChildren => createChildFunc != null;

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

		public ViewModelBase(string name, Func<ViewModelBase> createChild = null)
		{
			Name = name;
			createChildFunc = createChild;

			BindCommands();
			BindMessages();
		}

		public ICommand SelectCommand => new RelayCommand(Select);
		public ICommand AddChildCommand => new RelayCommand(AddChild);

		protected virtual void BindCommands() { }

		protected virtual void BindMessages() 
		{
			Messenger.Register<ViewModelRequestShowMessage>(this, (sender, message) => 
			{
				if (message.ViewModel != this)
				{
					IsSelected = false;
				}
			});
		}

		private void Select()
		{
			if (ChildViewModels.Count != 0 || SupportsAddingChildren)
			{
				IsShowingChildren = !IsShowingChildren;
			}

			Messenger.Send(new ViewModelRequestShowMessage(this));
			IsSelected = true;
		}

		protected void AddChild() 
		{
			ChildViewModels.Add(createChildFunc());

			foreach (var vm in ChildViewModels)
			{
				vm.SetLevel(level + 1);
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
	}
}
