namespace MoneySaver.System.Services.Identity;

public interface ICurrentUserService
{
	string UserId { get; }

	bool IsAdministrator { get; }
}
