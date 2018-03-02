using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace OrderCloud.AzureApp
{
	public static class ConfigExtensions
	{
		/// <summary>
		/// Register all services in a given assembly and (optionally) namespace by naming convention: IMyService -> MyService
		/// </summary>
		/// <param name="asm">Assembly to scan for interfaces and implementations.</param>
		/// <param name="namespace">Namespace to scan for interfaces (otional).</param>
		public static IServiceCollection AddByConvention(this IServiceCollection services, Assembly asm, string @namespace = null) {
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
	}
}
