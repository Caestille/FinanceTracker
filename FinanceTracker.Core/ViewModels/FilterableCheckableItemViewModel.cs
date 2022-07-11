using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace FinanceTracker.Core.Models
{
	public class FilterableCheckableItemViewModel : ObservableObject
	{
		public ICommand ToggleCheckStateCommand => new RelayCommand(ToggleCheckState);
		public ICommand DeleteRequestedCommand => new RelayCommand(DeleteRequested);

		private void ToggleCheckState()
		{
			IsChecked = !IsChecked;
		}

		private bool suppressNextNotification;
		public void SuppressNextNotification()
		{
			suppressNextNotification = true;
		}

		public string? Name { get; set; }
		public string? DisplayName { get; set; }

		private bool isChecked;
		public bool IsChecked
		{
			get => isChecked;
			set
			{
				var updated = isChecked != value;
				SetProperty(ref isChecked, value);
				//if (updated && !suppressNextNotification)
					//CheckedChanged?.Invoke(this, isChecked);
				
				suppressNextNotification = false;
			}
		}

		private double modelValue;
		public double ModelValue
		{
			get => modelValue;
			set => SetProperty(ref modelValue, value);
		}

		private bool isFilteredOut;
		public bool IsFilteredOut
		{
			get => isFilteredOut;
			set
			{
				var updated = isChecked != value;
				SetProperty(ref isFilteredOut, value);
				//if (updated)
					//FilterStateChanged?.Invoke(this, isFilteredOut);
			}
		}

		private void DeleteRequested()
		{
			//DeletionRequested?.Invoke(this, EventArgs.Empty);
		}
	}
}