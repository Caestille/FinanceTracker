using FinanceTracker.Core.Models;

namespace FinanceTracker.Core.Interfaces
{
	public interface IBankApiService
	{
		event EventHandler<(Guid, BankLinkStatus)> NewBankLinkStatusForGuid;

		Task<bool> LinkBank(Guid bankGuid, CancellationToken token);

		void ReloadLink(Guid bankGuid);

		Task DeleteLink(Guid bankGuid);
	}
}
