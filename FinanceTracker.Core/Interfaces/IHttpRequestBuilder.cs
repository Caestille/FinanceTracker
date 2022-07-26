using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTracker.Core.Interfaces
{
	public interface IHttpRequestBuilder
	{
		public enum HttpCommandType
		{
			[Description("POST")]
			Post,
			[Description("GET")]
			Get,
			[Description("DELETE")]
			Delete
		}

		IHttpRequestBuilder CreateRequest(HttpCommandType commandType, string requestTo);

		IHttpRequestBuilder WithUnvalidatedHeader(string header, string value);

		IHttpRequestBuilder WithHeaderContentType(string type);

		IHttpRequestBuilder WithContent(string name, string value);

		HttpRequestMessage Build();
	}
}
