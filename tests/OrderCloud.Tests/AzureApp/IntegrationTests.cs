using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using OrderCloud.AzureApp.Testing;
using OrderCloud.SDK;
using OrderCloud.TestWebApi;

namespace OrderCloud.Tests.AzureApp
{
	[TestFixture]
	public class IntegrationTests
	{
		[Test]
		public async Task can_allow_anonymous() {
			var result = await CreateServer().CreateFlurlClient().Request("test/anon").GetStringAsync();
			result.Should().Be("hello wide open!");
		}

		[Test]
		public async Task should_deny_access_without_oc_token() {
			var resp = await CreateServer().CreateFlurlClient().AllowAnyHttpStatus().Request("test/auth").GetAsync();
			resp.StatusCode.Should().Be(401);
		}

		[Test]
		public async Task can_auth_with_oc_token() {
			var result = await CreateServer()
				.CreateFlurlClient()
				.AllowAnyHttpStatus()
				.WithFakeOrderCloudToken("myclientid")
				.Request("test/auth")
				.GetStringAsync();

			result.Should().Be("hello protected!");
		}

		[Test]
		public async Task can_disambiguate_webhook() {
			var payload = new {
				Route = "v1/buyers/{buyerID}/addresses/{addressID}",
				Verb = "PUT",
				Request = new { Body = new { City = "Minneapolis" } },
				ConfigData = new { Foo = "blah" }
			};

			//var json = JsonConvert.SerializeObject(payload);
			//var keyBytes = Encoding.UTF8.GetBytes("myhashkey");
			//var dataBytes = Encoding.UTF8.GetBytes(json);
			//var hash = new HMACSHA256(keyBytes).ComputeHash(dataBytes);
			//var base64 = Convert.ToBase64String(hash);

			dynamic resp = await CreateServer()
				.CreateFlurlClient()
				.Request("test/webhook")
				.WithHeader("X-oc-hash", "4NPw1O9AviSOC1A3C+HqkDutRLNwyABneY/3M2OqByE=")
				.PostJsonAsync(payload)
				.ReceiveJson();

			Assert.AreEqual(resp.Action, "HandleAddressSave");
			Assert.AreEqual(resp.City, "Minneapolis");
			Assert.AreEqual(resp.Foo, "blah");
		}

		private TestServer CreateServer() {
			return new TestServer(Program.ConfigureWebHostBuilder<TestStartup>(new string[] {}));
		}

		private class TestStartup : Startup
		{
			public TestStartup(IConfiguration configuration) : base(configuration) { }

			public override void ConfigureServices(IServiceCollection services) {
				// first do real service registrations
				base.ConfigureServices(services);

				// then replace some of them with fakes
				var oc = Substitute.For<IOrderCloudClient>();
				oc.Me.GetAsync(Arg.Any<string>()).Returns(new MeUser { Username = "joe" });
				services.AddSingleton(oc);
			}
		}
	}

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
