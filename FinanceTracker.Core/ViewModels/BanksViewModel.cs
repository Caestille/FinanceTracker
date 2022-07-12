using FinanceTracker.Core.ViewModels;
using System;
using System.Collections.Generic;

namespace FinanceTracker.ViewModels
{
	public class BanksViewModel : ViewModelBase
	{
		public BanksViewModel() : base("Banks", new Func<ViewModelBase>(() => new BankViewModel("Unnamed Bank"))) 
		{
			
		}
	}
}
