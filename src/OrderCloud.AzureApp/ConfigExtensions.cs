using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OrderCloud.AzureApp
{
	public static class ConfigExtensions
	{
		/// <summary>
		/// Register all services in a given assembly and (optionally) namespace by naming convention: IMyService -> MyService
		/// </summary>
		/// <param name="asm">Assembly to scan for interfaces and implementations.</param>
		/// <param name="namespace">Namespace to scan for interfaces (otional).</param>
		public static IServiceCollection AddServicesByConvention(this IServiceCollection services, Assembly asm, string @namespace = null) {
			var mappings =
				from impl in asm.GetTypes()
				let iface = impl.GetInterface($"I{impl.Name}")
				where iface != null
				where @namespace == null || iface.Namespace == @namespace
				select new { iface, impl };

			foreach (var m in mappings)
				services.AddTransient(m.iface, m.impl);

			return services;
		}

		/// <summary>
		/// Binds your appsettings.json file (or other config source, such as App Settings in the Azure portal) to the AppSettings class
		/// so values can be accessed in a strongly typed manner. Call in your Program.cs off of WebHost.CreateDefaultBuilder(args).
		/// If called before UseStartup, then AppSettings can be injected into your Startup class.
		/// </summary>
		public static IWebHostBuilder UseAppSettings<TAppSettings>(this IWebHostBuilder hostBuilder) where TAppSettings : class, new() {
			return hostBuilder.ConfigureServices((ctx, services) => {
				// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options
				services.Configure<TAppSettings>(ctx.Configuration);

				// Breaks from the Options pattern (link above) by allowing AppSettings to be injected directly
				// into services, rather than injecting IOptions<AppSettings>.
				services.AddTransient(sp => sp.GetService<IOptionsSnapshot<TAppSettings>>().Value);
			});
		}
	}
}
