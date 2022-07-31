using System.Text.Json.Serialization;

namespace FinanceTracker.Core.DataTypeObjects
{
	public class TransactionRunningBalanceDto
	{
		[JsonPropertyName("currency")]
		public string Currency { get; set; }

		[JsonPropertyName("amount")]
		public double Amount { get; set; }
	}
}
