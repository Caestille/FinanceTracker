using FinanceTracker.Core.DataTypeObjects;
using FinanceTracker.Core.Models;

namespace FinanceTracker.Core.Interfaces
{
	public interface IBankApiService
	{
		event EventHandler<(Guid, BankLinkStatus)> NewBankLinkStatusForGuid;

		Task<bool> LinkBank(Guid bankGuid, CancellationToken token);

		void TryReloadBankLinkDetails(Guid bankGuid);

		Task DeleteLink(Guid bankGuid);

		Task<IEnumerable<AccountDto>> GetAccounts(Guid bankGuid);

		Task<IEnumerable<CardDto>> GetCards(Guid bankGuid);

		Task<IEnumerable<TransactionDto>> GetTransactions(Guid bankGuid, string accountName);

		Task<IEnumerable<TransactionDto>> GetCardTransactions(Guid bankGuid, string accountName);

		void Dispose();
	}
}
