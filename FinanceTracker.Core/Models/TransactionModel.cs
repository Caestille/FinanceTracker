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

		private string? description;
		public string? Description
		{
			get => description;
			set => SetProperty(ref description, value);
		}

		private string? tag;
		public string? Tag
		{
			get => tag;
			set => SetProperty(ref tag, value);
		}
	}
}
