using CoreUtilities.HelperClasses;
using FinanceTracker.Core.Interfaces;
using System.Net;
using System.Text;

namespace FinanceTracker.Core.Services
{
	public class HttpService : IHttpService
	{
		private HttpClient httpClient = new HttpClient();
        private Func<IHttpRequestBuilder> httpRequestBuilderCreator;

        public HttpService(Func<IHttpRequestBuilder> builderCreateFunc)
		{
            this.httpRequestBuilderCreator = builderCreateFunc;
        }

        public IHttpRequestBuilder GetHttpRequestBuilder()
		{
            return httpRequestBuilderCreator();
        }

		public async Task<(bool, string)> WaitForAndQueryResponseOverUri(string callbackUri, string query, CancellationToken? token = null)
		{
            var listener = new HttpListener();
            listener.Prefixes.Add(callbackUri);
            listener.Start();

            HttpListenerContext? context = null;
            try
            {
                context = await listener.GetContextAsync().AsCancellable(token ?? CancellationToken.None);
            }
            catch (TaskCanceledException)
            {
                listener.Stop();
                return (false, string.Empty);
            }

            string html = string.Format("<html><body></body></html>");
            var buffer = Encoding.UTF8.GetBytes(html);
            context.Response.ContentLength64 = buffer.Length;
            var stream = context.Response.OutputStream;
            var responseTask = stream.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) =>
            {
                stream.Close();
                listener.Stop();
            });

            string? result = context.Request.QueryString[query];

            return (result != null, result ?? string.Empty);
        }

        public string QueryValueFromResponse(string valueName, string content)
        {
            var contentArray = content.Split(',');
            return contentArray.First(x => x.Split(':')[0].Contains(valueName)).Split(':')[1].Replace("\"", "").Trim(new char[] { '{', '}' });
        }

        public async Task<(HttpStatusCode, string)> SendAsyncDisposeAndGetResponse(HttpRequestMessage request)
		{
			var response = await httpClient.SendAsync(request);
			request.Dispose();

			return (response.StatusCode, await response.Content.ReadAsStringAsync());
		}
	}
}
