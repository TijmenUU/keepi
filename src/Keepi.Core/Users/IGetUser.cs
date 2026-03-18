namespace Keepi.Core.Users;

public enum GetUserError
{
    Unknown,
    DoesNotExist,
}

public interface IGetUser
{
    Task<IValueOrErrorResult<GetUserResult, GetUserError>> Execute(
        UserExternalId externalId,
        UserIdentityProvider identityProvider,
        CancellationToken cancellationToken
    );
}

public sealed record GetUserResult(
    UserId Id,
    UserName Name,
    EmailAddress EmailAddress,
    UserIdentityProvider IdentityOrigin,
    UserPermission EntriesPermission,
    UserPermission ExportsPermission,
    UserPermission ProjectsPermission,
    UserPermission UsersPermission
);
