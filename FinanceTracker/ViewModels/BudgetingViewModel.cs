﻿using FinanceTracker.Core.ViewModels;

namespace FinanceTracker.ViewModels
{
	public class BudgetingViewModel : ViewModelBase
	{
		public BudgetingViewModel() : base("Budgeting") { }

		protected override void BindCommands() { }

		protected override void BindMessages() { base.BindMessages(); }
	}
}
