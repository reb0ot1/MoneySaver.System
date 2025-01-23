using System;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MoneySaver.System.Services;

namespace MoneySaver.System.Infrastructure;

public static class ApplicationBuilderExtension
{
	public static IApplicationBuilder UseWebService(this IApplicationBuilder app, IWebHostEnvironment env)
	{
		if (env.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
		}
		app.UseHttpsRedirection().UseRouting().UseCors(delegate(CorsPolicyBuilder options)
		{
			options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
		})
			.UseAuthentication()
			.UseAuthorization()
			.UseEndpoints(delegate(IEndpointRouteBuilder endpoints)
			{
				endpoints.MapHealthChecks("/health", new HealthCheckOptions
				{
					ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
				});
				endpoints.MapControllers();
			});
		return app;
	}

	public static IApplicationBuilder Initialize(this IApplicationBuilder app)
	{
		using IServiceScope serviceScope = app.ApplicationServices.CreateScope();
		IServiceProvider serviceProvider = serviceScope.ServiceProvider;
		RelationalDatabaseFacadeExtensions.Migrate(serviceProvider.GetRequiredService<DbContext>().Database);
		foreach (IDataSeeder service in serviceProvider.GetServices<IDataSeeder>())
		{
			service.SeedData();
		}
		return app;
	}
}
