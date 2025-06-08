namespace Keepi.Core.Users;

public enum GetUserError
{
    Unknown,
    DoesNotExist,
}

public interface IGetUser
{
    Task<IValueOrErrorResult<UserEntity, GetUserError>> Execute(
        string externalId,
        UserIdentityProvider identityProvider,
        CancellationToken cancellationToken
    );
}
