using System.Text.Json.Serialization;

namespace FinanceTracker.Core.DataTypeObjects
{
	public class AccountProviderDto
	{
		[JsonPropertyName("provider_id")]
		public string ProviderId { get; set; }
	}
}
