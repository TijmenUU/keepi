namespace Keepi.Core.Users;

public interface IGetUserExists
{
    Task<IValueOrErrorResult<bool, GetUserExistsError>> Execute(
        string externalId,
        UserIdentityProvider userIdentityProvider,
        string emailAddress,
        CancellationToken cancellationToken
    );
}

public enum GetUserExistsError
{
    Unknown = 0,
}
