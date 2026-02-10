namespace Keepi.Core.Users;

public interface IResolveUser
{
    Task<IValueOrErrorResult<ResolvedUser, ResolveUserError>> Execute(
        CancellationToken cancellationToken
    );
}

public sealed record ResolvedUser(
    int Id,
    string Name,
    string EmailAddress,
    UserPermission EntriesPermission,
    UserPermission ExportsPermission,
    UserPermission ProjectsPermission,
    UserPermission UsersPermission
);

public enum ResolveUserError
{
    Unknown = 0,
    UnsupportedIdentityProvider,
    MalformedUserClaims,
    UserNotAuthenticated,
    UserNotFound,
    UserRegistrationFailed,
    UnexpectedLocalApplicationUser,
    UnexpectedNonLocalApplicationUser,
}
