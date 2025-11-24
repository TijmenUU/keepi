namespace Keepi.Core.Users;

public enum GetUserError
{
    Unknown,
    DoesNotExist,
}

public interface IGetUser
{
    Task<IValueOrErrorResult<GetUserResult, GetUserError>> Execute(
        string externalId,
        UserIdentityProvider identityProvider,
        CancellationToken cancellationToken
    );
}

public sealed record GetUserResult(
    int Id,
    string Name,
    string EmailAddress,
    UserIdentityProvider IdentityOrigin
);
