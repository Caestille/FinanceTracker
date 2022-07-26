using CoreUtilities.HelperClasses;
using FinanceTracker.Core.Interfaces;
using System.Net.Http.Headers;
using System.Text;

namespace FinanceTracker.Core.Services
{
	public class HttpRequestBuilder : IHttpRequestBuilder
	{
		private HttpRequestMessage currentRequest;

		private string requestType;
		private string requestTo;
		private Dictionary<string, string> unvalidatedHeaders = new Dictionary<string, string>();
		private Dictionary<string, string> content = new Dictionary<string, string>();
		private string headerContentType;

		public IHttpRequestBuilder CreateRequest(IHttpRequestBuilder.HttpCommandType commandType, string requestTo)
		{
			try { currentRequest?.Dispose(); } catch { /* Already disposed */ }
			unvalidatedHeaders.Clear();
			content.Clear();
			this.requestTo = requestTo;
			requestType = commandType.GetEnumDescription();
			headerContentType = string.Empty;
			return this;
		}

		public IHttpRequestBuilder WithContent(string name, string value)
		{
			content[name] = value;
			return this;
		}

		public IHttpRequestBuilder WithHeaderContentType(string type)
		{
			headerContentType = type;
			return this;
		}

		public IHttpRequestBuilder WithUnvalidatedHeader(string header, string value)
		{
			unvalidatedHeaders[header] = value;
			return this;
		}

		public HttpRequestMessage Build()
		{
			currentRequest = new HttpRequestMessage(new HttpMethod(requestType), requestTo);

			if (unvalidatedHeaders.Any())
			{
				foreach (var kvp in unvalidatedHeaders)
				{
					currentRequest.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
				}
			}

			if (content.Any())
			{
				StringBuilder contentSb = new StringBuilder().Append("{ ");
				int i = 0;
				foreach (var kvp in content)
				{
					i++;
					var comma = i == content.Count ? " " : ", ";
					contentSb.Append($"\"{kvp.Key}\": \"{kvp.Value}\"{comma}");
				}
				contentSb.Append(" }");
				currentRequest.Content = new StringContent(contentSb.ToString());
			}

			if (!string.IsNullOrEmpty(headerContentType))
			{
				currentRequest.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(headerContentType);
			}

			return currentRequest;
		}
	}
}
