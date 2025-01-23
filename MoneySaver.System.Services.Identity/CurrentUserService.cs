using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MoneySaver.System.Infrastructure;

namespace MoneySaver.System.Services.Identity;

public class CurrentUserService : ICurrentUserService
{
	private readonly ClaimsPrincipal user;

	public string UserId { get; }

	public bool IsAdministrator => user.IsAdministrator();

	public CurrentUserService(IHttpContextAccessor httpContextAccessor)
	{
		user = httpContextAccessor.HttpContext.User;
		if (user == null)
		{
			throw new InvalidOperationException("This request does not have authenticated user.");
		}
		UserId = user.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
	}
}
