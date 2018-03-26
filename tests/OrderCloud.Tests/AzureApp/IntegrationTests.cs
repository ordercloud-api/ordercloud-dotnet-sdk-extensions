﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Flurl.Http;
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

			dynamic resp = await CreateServer()
				.CreateFlurlClient()
				.Request("test/webhook")
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
}
