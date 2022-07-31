﻿using CoreUtilities.HelperClasses;
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

        public async Task<(HttpStatusCode, string)> SendAsyncDisposeAndGetResponse(HttpRequestMessage request, CancellationToken? token)
		{
            HttpResponseMessage response = null;
            try
            {
                response = await httpClient.SendAsync(request).AsCancellable(token ?? CancellationToken.None);
            }
            catch (TaskCanceledException e)
            {
                // Task was cancelled
                // TODO: Logging
            }
            finally
            {
                request.Dispose();
            }

            if (response == null)
			{

			}

			return (response != null ? response.StatusCode : HttpStatusCode.ServiceUnavailable, 
                response != null ? await response.Content.ReadAsStringAsync() : string.Empty);
		}
	}
}
