namespace Keepi.Core.Users;

public interface ISaveNewUser
{
    Task<IMaybeErrorResult<SaveNewUserError>> Execute(
        string externalId,
        string emailAddress,
        string name,
        UserIdentityProvider userIdentityProvider,
        CancellationToken cancellationToken
    );
}

public enum SaveNewUserError
{
    Unknown,
    DuplicateUser,
};
