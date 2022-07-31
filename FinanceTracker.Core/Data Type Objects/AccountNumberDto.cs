using System.Text.Json.Serialization;

namespace FinanceTracker.Core.DataTypeObjects
{
	public class AccountNumberDto
	{
		[JsonPropertyName("iban")]
		public string Iban { get; set; }

		[JsonPropertyName("number")]
		public string AccountNumber { get; set; }

		[JsonPropertyName("sort_code")]
		public string SortCode { get; set; }

		[JsonPropertyName("swift_bic")]
		public string SwiftBic { get; set; }
	}
}
