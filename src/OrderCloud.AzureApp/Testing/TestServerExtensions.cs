using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.AspNetCore.TestHost;

namespace OrderCloud.AzureApp.Testing
{
	public static class TestServerExtensions
	{
		public static IFlurlClient CreateFlurlClient(this TestServer server) {
			var fc = new FlurlClient(server.BaseAddress.AbsoluteUri);
			fc.Settings.HttpClientFactory = new TestServerHttpClientFactory(server);
			return fc;
		}

		private class TestServerHttpClientFactory : DefaultHttpClientFactory
		{
			private readonly TestServer _server;

			public TestServerHttpClientFactory(TestServer server) {
				_server = server;
			}

			public override HttpClient CreateHttpClient(HttpMessageHandler handler) => _server.CreateClient();
		}
	}
}
