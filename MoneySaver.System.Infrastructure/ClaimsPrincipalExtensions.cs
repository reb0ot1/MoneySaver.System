using System.Security.Claims;

namespace MoneySaver.System.Infrastructure;

public static class ClaimsPrincipalExtensions
{
	public static bool IsAdministrator(this ClaimsPrincipal user)
	{
		return user.IsInRole("Administrator");
	}
}
