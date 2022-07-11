using FinanceTracker.Core.ViewModels;

namespace FinanceTracker.ViewModels
{
	public class AnalysisViewModel : ViewModelBase
	{
		public AnalysisViewModel() : base("Analysis") { }

		protected override void BindCommands() { }

		protected override void BindMessages() { base.BindMessages(); }
	}
}
