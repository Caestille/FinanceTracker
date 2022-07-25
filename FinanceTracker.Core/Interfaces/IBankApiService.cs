using FinanceTracker.Core.Models;

namespace FinanceTracker.Core.Interfaces
{
	public interface IBankApiService
	{
		event EventHandler<(Guid, BankLinkStatus)> NewBankLinkStatusForGuid;

		Task<bool> LinkBank(Guid bankGuid, CancellationToken token);

		void ReloadBankLinkDetails(Guid bankGuid);

		Task DeleteLink(Guid bankGuid);
	}
}
