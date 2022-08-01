using FinanceTracker.Core.DataTypeObjects;
using System.Text.Json.Serialization;

namespace FinanceTracker.Core.DataTypeObjects
{
	public class CardDto
	{
		[JsonPropertyName("account_id")]
		public string AccountId { get; set; }

		[JsonPropertyName("card_network")]
		public string Network { get; set; }

		[JsonPropertyName("card_type")]
		public string Type { get; set; }

		[JsonPropertyName("currency")]
		public string Currency { get; set; }

		[JsonPropertyName("display_name")]
		public string DisplayName { get; set; }

		[JsonPropertyName("partial_card_number")]
		public string PartialCardNumber { get; set; }

		[JsonPropertyName("name_on_card")]
		public string NameOnCard { get; set; }

		[JsonPropertyName("valid_from")]
		public string ValidFrom { get; set; }

		[JsonPropertyName("valid_to")]
		public string ValidTo { get; set; }

		[JsonPropertyName("update_timestamp")]
		public string UpdateTimestamp { get; set; }

		[JsonPropertyName("provider")]
		public AccountProviderDto Provider { get; set; }
	}
}
