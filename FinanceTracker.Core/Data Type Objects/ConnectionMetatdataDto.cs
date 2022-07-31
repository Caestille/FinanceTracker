using FinanceTracker.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FinanceTracker.Core.DataTypeObjects
{
	public class ConnectionMetadataDto
	{
		[JsonPropertyName("client_id")]
		public string ClientId { get; set; }

		[JsonPropertyName("credentials_id")]
		public string CredentialsId { get; set; }

		[JsonPropertyName("consent_status")]
		public string ConsentStatus { get; set; }

		[JsonPropertyName("consent_status_updated_at")]
		public string ConsentUpdated { get; set; }

		[JsonPropertyName("consent_created_at")]
		public string ConsentCreated { get; set; }

		[JsonPropertyName("consent_expires_at")]
		public string ConsentExpires { get; set; }

		[JsonPropertyName("provider")]
		public ConnectionProviderDto ConnectionProvider { get; set; }

		[JsonPropertyName("scopes")]
		public IEnumerable<string> Scopes { get; set; }

		[JsonPropertyName("privacy_policy")]
		public string PrivacyPolicy { get; set; }
	}
}
