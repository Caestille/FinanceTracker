using System.Net;

namespace FinanceTracker.Core.Interfaces
{
	public interface IHttpService
	{
		Task<(bool, string)> WaitForAndQueryResponseOverUri(string callbackUri, string query, CancellationToken? token = null);

		IHttpRequestBuilder GetHttpRequestBuilder();

		Task<(HttpStatusCode, string)> SendAsyncDisposeAndGetResponse(HttpRequestMessage request, CancellationToken? token = null);
	}
}
