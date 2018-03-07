using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OrderCloud.AzureApp
{
    public abstract class GlobalErrorHandler : IMiddleware
	{
		public async Task InvokeAsync(HttpContext context, RequestDelegate next) {
			try {
				await next(context).ConfigureAwait(false);
			}
			catch (Exception ex) {
				await HandleAsync(context, ex).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Override if your error handler needs to make any async calls.
		/// </summary>
		public virtual Task HandleAsync(HttpContext context, Exception ex) {
			Handle(context, ex);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Override if your error handler does NOT need to make any async calls.
		/// </summary>
		public virtual void Handle(HttpContext context, Exception ex) { }
	}
}
