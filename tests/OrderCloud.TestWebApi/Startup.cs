using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using OrderCloud.AzureApp;

namespace OrderCloud.TestWebApi
{
	public class Startup
	{
		public Startup(IConfiguration configuration) {
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public virtual void ConfigureServices(IServiceCollection services) {
			services
				.AddMvc()
				// don't mess with casing https://github.com/aspnet/Announcements/issues/194
				.AddJsonOptions(opts => opts.SerializerSettings.ContractResolver = new DefaultContractResolver());

			services.AddAuthentication()
				.AddOrderCloud(opts => opts.ValidForClientIDs("myclientid"));

			services.AddWebhookDispatcher();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env) {
			if (env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			}

			app.UseAuthentication();
			app.UseMvc();
		}
	}
}
