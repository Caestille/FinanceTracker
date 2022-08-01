using FinanceTracker.Core.DataTypeObjects;

namespace FinanceTracker.Core.Models
{
	public class DatabaseTransactionStorageModel
	{
		public bool IsFilteredOut { get; set; }

		public TransactionDto TransactionDto { get; set; }

		public int Id { get; set; }
	}
}
