using CoreUtilities.HelperClasses;
using CoreUtilities.Interfaces;
using FinanceTracker.Core.Messages;
using FinanceTracker.Core.ViewModels;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FinanceTracker.ViewModels
{
	public class MainViewModel : ViewModelBase
	{
		private IRegistryService registryService;

		private const string MenuPinnedSettingName = "MenuPinned";

		public ICommand ToggleMenuOpenCommand => new RelayCommand(ToggleMenuOpen);
		public ICommand ToggleMenuPinCommand => new RelayCommand(ToggleMenuPin);
		public ICommand FormLoadedCommand => new RelayCommand(Loaded);

		private ViewModelBase visibleViewModel;
		public ViewModelBase VisibleViewModel
		{
			get => visibleViewModel;
			set => SetProperty(ref visibleViewModel, value);
		}

		private bool isMenuPinned;
		public bool IsMenuPinned
		{
			get => isMenuPinned;
			set
			{
				SetProperty(ref isMenuPinned, value);
				registryService.SetSetting(MenuPinnedSettingName, value.ToString());
			}
		}

		private bool isMenuVisible = true;
		public bool IsMenuOpen
		{
			get => isMenuVisible;
			set => SetProperty(ref isMenuVisible, value);
		}

		private string searchText = string.Empty;
		public string SearchText
		{
			get => searchText;
			set
			{
				SetProperty(ref searchText, value);
				OnPropertyChanged(nameof(FilteredViewModels));
			}
		}

		public RangeObservableCollection<ViewModelBase> AllViewModels
		{
			get
			{
				RangeObservableCollection<ViewModelBase> result = new RangeObservableCollection<ViewModelBase>();
				this.GetChildren(ref result, true);
				return result;
			}
		}

		public RangeObservableCollection<ViewModelBase> FilteredViewModels
		{
			get
			{
				return new RangeObservableCollection<ViewModelBase>(AllViewModels.Where(x => x.Name.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase)));
			}
		}

		public MainViewModel(List<ViewModelBase> viewModels, IRegistryService registryService) : base(string.Empty)
		{
			this.registryService = registryService;

			ChildViewModels.AddRange(viewModels);

            SetLevel(0);
		}

		protected override void BindMessages()
		{
			Messenger.Register<ViewModelRequestShowMessage>(this, (sender, message) => 
			{
				VisibleViewModel = message.ViewModel;
				VisibleViewModel.IsSelected = true;
				SearchText = string.Empty; 
			});

			Messenger.Register<ViewModelRequestDeleteMessage>(this, (sender, message) =>
			{
				if (message.ViewModel.IsSelected)
				{
					VisibleViewModel = AllViewModels.First(x => x.GetType() == typeof(DashboardViewModel));
				}
			});
		}

		private void ToggleMenuOpen()
		{
			IsMenuOpen = !IsMenuOpen;
		}

		private void ToggleMenuPin()
		{
			IsMenuPinned = !IsMenuPinned;
		}

		private void Loaded()
        {
			registryService.TryGetSetting(MenuPinnedSettingName, false, out var menuPinned);
			if (menuPinned && !IsMenuOpen)
			{
				IsMenuOpen = true;
			}
			IsMenuPinned = menuPinned;
		}
	}
}
