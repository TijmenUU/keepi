namespace Keepi.Core.Users;

public interface IGetUserExists
{
    Task<IValueOrErrorResult<bool, GetUserExistsError>> Execute(
        UserExternalId externalId,
        UserIdentityProvider userIdentityProvider,
        EmailAddress emailAddress,
        CancellationToken cancellationToken
    );
}

public enum GetUserExistsError
{
    Unknown = 0,
}
