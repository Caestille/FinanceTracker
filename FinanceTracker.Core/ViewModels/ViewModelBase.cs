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

		private bool supportsAddingChildren;
		public bool SupportsAddingChildren
		{
			get => supportsAddingChildren;
			set => SetProperty(ref supportsAddingChildren, value);
		}

		private bool showChildren;
		public bool ShowChildren
		{
			get => showChildren;
			set => SetProperty(ref showChildren, value);
		}

		private int level = 0;
		public int Level
		{
			get => level;
			set => SetProperty(ref level, value);
		}

		public ViewModelBase(string name)
		{
			Name = name;

			BindCommands();
			BindMessages();

			AddLevelsToChildren();
		}

		public ICommand SelectCommand => new RelayCommand(Select);

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
			if (ChildViewModels.Count != 0)
			{
				ShowChildren = !ShowChildren;
			}
			else
			{
				Messenger.Send(new ViewModelRequestShowMessage(this));
				IsSelected = true;
			}
		}

		private void AddLevelsToChildren()
		{
			ChildViewModels.ToList().ForEach(x => x.AddLevel());
		}

		protected void AddLevel()
		{
			Level++;
			AddLevelsToChildren();
		}
	}
}
