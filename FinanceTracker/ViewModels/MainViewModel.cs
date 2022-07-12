﻿using CoreUtilities.HelperClasses;
using FinanceTracker.Core.Messages;
using FinanceTracker.Core.ViewModels;
using Microsoft.Toolkit.Mvvm.Messaging;
using System.Collections.Generic;

namespace FinanceTracker.ViewModels
{
	public class MainViewModel : ViewModelBase
	{
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
			set => SetProperty(ref isMenuPinned, value);
		}

		private bool isMenuVisible;
		public bool IsMenuVisible
		{
			get => isMenuVisible;
			set => SetProperty(ref isMenuVisible, value);
		}

		public MainViewModel(List<ViewModelBase> viewModels) : base(string.Empty)
		{
			ChildViewModels.AddRange(viewModels);
			SetLevel(0);
		}

		protected override void BindMessages()
		{
			Messenger.Register<ViewModelRequestShowMessage>(this, (sender, message) => { VisibleViewModel = message.ViewModel; });
		}
	}
}
