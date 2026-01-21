namespace Keepi.Core.Users;

public enum GetUsersError
{
    Unknown,
}

public interface IGetUsers
{
    Task<IValueOrErrorResult<GetUsersResult, GetUsersError>> Execute(
        CancellationToken cancellationToken
    );
}

public sealed record GetUsersResult(GetUsersResultUser[] Users);

public sealed record GetUsersResultUser(
    int Id,
    string Name,
    string EmailAddress,
    UserIdentityProvider IdentityOrigin,
    UserPermission EntriesPermission,
    UserPermission ExportsPermission,
    UserPermission ProjectsPermission,
    UserPermission UsersPermission
);
