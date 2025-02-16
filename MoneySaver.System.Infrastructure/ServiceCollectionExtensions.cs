using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using MoneySaver.System.Models;
using MoneySaver.System.Services.Identity;

namespace MoneySaver.System.Infrastructure;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddWebService<TDbContext>(this IServiceCollection services, IConfiguration configuration) where TDbContext : DbContext
	{
		services.AddDatabase<TDbContext>(configuration).AddApplicationSettings(configuration).AddTokenAuthentication(configuration)
			.AddHealth(configuration)
			.AddAutomapperProfile(Assembly.GetCallingAssembly())
			.AddControllers();
		return services;
	}

	public static IServiceCollection AddDatabase<TDbContext>(this IServiceCollection services, IConfiguration configuration) where TDbContext : DbContext
	{
		return EntityFrameworkServiceCollectionExtensions.AddDbContext<TDbContext>(services.AddScoped<DbContext, TDbContext>(), (Action<DbContextOptionsBuilder>)delegate(DbContextOptionsBuilder options)
		{
			SqlServerDbContextOptionsExtensions.UseSqlServer(options, configuration.GetDefaultConnectionString(), (Action<SqlServerDbContextOptionsBuilder>)delegate(SqlServerDbContextOptionsBuilder sqloptions)
			{
				sqloptions.EnableRetryOnFailure(10, TimeSpan.FromSeconds(30.0), (ICollection<int>)null);
			});
		}, ServiceLifetime.Scoped, ServiceLifetime.Scoped);
	}

	public static IServiceCollection AddApplicationSettings(this IServiceCollection services, IConfiguration configuration)
	{
		return services.Configure<ApplicationSettings>(configuration.GetSection("ApplicationSettings"), delegate(BinderOptions config)
		{
			config.BindNonPublicProperties = true;
		});
	}

	public static IServiceCollection AddTokenAuthentication(this IServiceCollection services, IConfiguration configuration, JwtBearerEvents events = null)
	{
		string value = configuration.GetSection("ApplicationSettings").GetValue<string>("Secret");
		byte[] key = Encoding.ASCII.GetBytes(value);
        services.AddAuthentication(delegate (AuthenticationOptions authentication)
        {
            authentication.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            authentication.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(bearer => 
        {
            // DO NOT USE THIS CONFIGURATION IN PRODUCTION
            bearer.RequireHttpsMetadata = false;
            bearer.SaveToken = true;
            
            bearer.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
            };
            if (events != null)
            {
                bearer.Events = events;
            }
            else
            {
                //bearer.Events = new JwtBearerEvents
                //{
                //    OnMessageReceived = c =>
                //    {
                //        ;
                //        return Task.CompletedTask;
                //    }
                //};
            }
        });
        //JwtBearerExtensions.AddJwtBearer(services.AddAuthentication(delegate(AuthenticationOptions authentication)
        //{
        //	authentication.DefaultAuthenticateScheme = "Bearer";
        //	authentication.DefaultChallengeScheme = "Bearer";
        //}), (Action<JwtBearerOptions>)delegate(JwtBearerOptions bearer)
        //{
        //	//IL_000f: Unknown result type (might be due to invalid IL or missing references)
        //	//IL_0014: Unknown result type (might be due to invalid IL or missing references)
        //	//IL_001b: Unknown result type (might be due to invalid IL or missing references)
        //	//IL_0022: Unknown result type (might be due to invalid IL or missing references)
        //	//IL_002c: Expected O, but got Unknown
        //	//IL_002c: Unknown result type (might be due to invalid IL or missing references)
        //	//IL_0033: Unknown result type (might be due to invalid IL or missing references)
        //	//IL_003f: Expected O, but got Unknown
        //	bearer.RequireHttpsMetadata = false;
        //	bearer.SaveToken = true;
        //	bearer.TokenValidationParameters = new TokenValidationParameters
        //	{
        //		ValidateIssuerSigningKey = true,
        //		IssuerSigningKey = new SymmetricSecurityKey(key),
        //		ValidateIssuer = false,
        //		ValidateAudience = false
        //	};
        //	if (events != null)
        //	{
        //		bearer.Events = events;
        //	}
        //});
        services.AddHttpContextAccessor();
		services.AddScoped<ICurrentUserService, CurrentUserService>();
		return services;
	}

	public static IServiceCollection AddAutomapperProfile(this IServiceCollection services, Assembly assembly)
	{
		return Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddAutoMapper(services, (Action<IServiceProvider, IMapperConfigurationExpression>)delegate(IServiceProvider _, IMapperConfigurationExpression config)
		{
			config.AddProfile((Profile)(object)new MappingProfile(assembly));
		}, Array.Empty<Assembly>());
	}

	public static IServiceCollection AddHealth(this IServiceCollection services, IConfiguration configuration)
	{
		SqlServerHealthCheckBuilderExtensions.AddSqlServer(services.AddHealthChecks(), configuration.GetDefaultConnectionString());
		return services;
	}
}
