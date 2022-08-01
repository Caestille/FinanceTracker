using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace FinanceTracker.Core.Models
{
	public class TransactionModel : ObservableObject
	{
		private DateTime date;
		public DateTime Date
		{
			get => date;
			set => SetProperty(ref date, value);
		}

		private double inValue;
		public double InValue
		{
			get => inValue;
			set => SetProperty(ref inValue, value);
		}

		private double outValue;
		public double OutValue
		{
			get => outValue;
			set => SetProperty(ref outValue, value);
		}

		private double runningBalance;
		public double RunningBalance
		{
			get => runningBalance;
			set => SetProperty(ref runningBalance, value);
		}

		private string merchant;
		public string Merchant
		{
			get => merchant;
			set => SetProperty(ref merchant, value);
		}

		private string description;
		public string Description
		{
			get => description;
			set => SetProperty(ref description, value);
		}

		private string location;
		public string Location
		{
			get => location;
			set => SetProperty(ref location, value);
		}

		private string tag;
		public string Tag
		{
			get => tag;
			set => SetProperty(ref tag, value);
		}
	}
}
