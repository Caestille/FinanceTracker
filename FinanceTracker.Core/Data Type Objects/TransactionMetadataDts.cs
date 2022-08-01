using System.Text.Json.Serialization;

namespace FinanceTracker.Core.DataTypeObjects
{
	public class MetadataDto
	{
		[JsonPropertyName("provider_merchant_name")]
		public string ProviderMerchantName { get; set; }

		[JsonPropertyName("address")]
		public string Address { get; set; }

		[JsonPropertyName("provider_category")]
		public string ProviderCategory { get; set; }

		[JsonPropertyName("transaction_type")]
		public string Type { get; set; }

		[JsonPropertyName("provider_id")]
		public string ProviderId { get; set; }
	}
}
