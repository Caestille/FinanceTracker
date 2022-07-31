using System.Text.Json.Serialization;

namespace FinanceTracker.Core.DataTypeObjects
{
	public  class AccountDto
	{
		[JsonPropertyName("update_timestamp")]
		public string UpdateTimestamp { get; set; }

		[JsonPropertyName("account_id")]
		public string AccountId { get; set; }

		[JsonPropertyName("account_type")]
		public string Type { get; set; }

		[JsonPropertyName("display_name")]
		public string DisplayName { get; set; }

		[JsonPropertyName("currency")]
		public string Currency { get; set; }

		[JsonPropertyName("account_number")]
		public AccountNumberDto AccountNumber { get; set; }

		[JsonPropertyName("provider")]
		public AccountProviderDto Provider { get; set; }
	}
}
