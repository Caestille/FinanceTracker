using System.Text.Json.Serialization;

namespace FinanceTracker.Core.DataTypeObjects
{
	public  class TransactionDto
	{
		[JsonPropertyName("timestamp")]
		public string Timestamp { get; set; }

		[JsonPropertyName("description")]
		public string Description { get; set; }

		[JsonPropertyName("transaction_type")]
		public string Type { get; set; }

		[JsonPropertyName("transaction_category")]
		public string Category { get; set; }

		[JsonPropertyName("transaction_classification")]
		public IEnumerable<string> Classification { get; set; }

		[JsonPropertyName("merchant_name")]
		public string MerchantName { get; set; }

		[JsonPropertyName("amount")]
		public double Amount { get; set; }

		[JsonPropertyName("currency")]
		public string Currency { get; set; }

		[JsonPropertyName("transaction_id")]
		public string Id { get; set; }

		[JsonPropertyName("provider_transaction_id")]
		public string ProviderId { get; set; }

		[JsonPropertyName("normalised_provider_transaction_id")]
		public string NormalisedProviderId { get; set; }

		// Currency
		// Amount
		[JsonPropertyName("running_balance")]
		public TransactionRunningBalanceDto RunningBalance { get; set; }

		// ProviderMerchantName
		// Address
		// ProviderCategory
		// Type
		// ProviderId
		[JsonPropertyName("meta")]
		public MetadataDto Metadata { get; set; }
	}
}
