using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace OrderCloud.AzureApp
{
	/// <summary>
	/// Inherit your ASP.NET Core 2.0 Startup class from this. It provides more intutive methods than the standard
	/// Microsoft-documented Startup class, such as RegisterServices, ConfigureMiddleware, and ConfigureLogging.
	/// Override these methods INSTEAD of defining ConfigureServices and Configure methods.
	/// </summary>
	public abstract class OrderCloudStartup
    {
		public void ConfigureServices(IServiceCollection services) {
			// set up logging first so we can log errors as early as possible
			var logConfig = new LoggerConfiguration();
			ConfigureLogging(logConfig);
			Log.Logger = logConfig.CreateLogger();

			try {
				RegisterServices(services);
			}
			catch (Exception ex) {
				Log.Logger.Error(ex, "Startup error in RegisterServices");
			}
		}

	    public void Configure(IApplicationBuilder app) {
		    try {
			    ConfigureMiddleware(app);
		    }
			catch (Exception ex) {
			    Log.Logger.Error(ex, "Startup error in ConfigureMiddleware");
		    }
	    }

		/// <summary>
		/// Override to register services in the IoC container. Consider following the naming convention
		/// IMyService / MyService and using services.AddServicesByConvention. Use this INSTEAD of defining
		/// a ConfigureServices method per the standard ASP.NET Core Startup class pattern.
		/// </summary>
		public virtual void RegisterServices(IServiceCollection services) { }

		/// <summary>
		/// Override to add middleware to the request pipeline. Use this INSTEAD of defining a Configure
		/// method per the standard ASP.NET Core Startup class pattern.
		/// </summary>
	    public virtual void ConfigureMiddleware(IApplicationBuilder app) { }

		/// <summary>
		/// Configure your logger. See Serilog documentation. This will be called first in Startup so that
		/// any subsequent exceptions thrown during startup can be logged.
		/// </summary>
	    public virtual void ConfigureLogging(LoggerConfiguration config) { }
    }
}
