using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.AzureApp;
using OrderCloud.SDK;

namespace SampleApp.WebApi.Controllers
{
	[Route("test")]
	public class TestController : Controller
	{
		[HttpGet("auth"), OrderCloudUserAuth]
		public object Get() => "hello protected!";

		[HttpGet("anon")]
		public object GetWithoutAuth() => "hello wide open!";

		[Route("webhook"), OrderCloudWebhookAuth]
		public object HandleAddressSave([FromBody] WebhookPayloads.Addresses.Save<MyConfigData> payload) {
			return new {
				Action = "HandleAddressSave",
				City = payload.Request.Body.City,
				Foo = payload.ConfigData.Foo
			};
		}

		[Route("webhook"), OrderCloudWebhookAuth]
		public object HandleGenericWebhook([FromBody] WebhookPayload payload) {
			return new {
				Action = "HandleGenericWebhook",
				City = payload.Request.Body.City,
				Foo = payload.ConfigData.Foo
			};
		}
	}

	public class MyConfigData
	{
		public string Foo { get; set; }
		public int Bar { get; set; }
	}
}
