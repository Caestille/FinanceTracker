using System.Text.Json.Serialization;

namespace FinanceTracker.Core.DataTypeObjects
{
	public class ResultsHostDto<T>
	{
		[JsonPropertyName("results")]
		public IEnumerable<T> Results { get; set; }

		[JsonPropertyName("status")]
		public string Status { get; set; }
	}
}
