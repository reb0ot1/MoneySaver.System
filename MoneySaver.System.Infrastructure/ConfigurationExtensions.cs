using Microsoft.Extensions.Configuration;

namespace MoneySaver.System.Infrastructure;

public static class ConfigurationExtensions
{
	public static string GetDefaultConnectionString(this IConfiguration configuration)
	{
		return configuration.GetConnectionString("DefaultConnection");
	}
}
