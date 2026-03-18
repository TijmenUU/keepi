namespace Keepi.Core.Users;

public interface IUpdateUserIdentity
{
    Task<IMaybeErrorResult<UpdateUserIdentityError>> Execute(
        UserId userId,
        EmailAddress emailAddress,
        UserName name,
        CancellationToken cancellationToken
    );
}

public enum UpdateUserIdentityError
{
    Unknown,
    UnknownUserId,
    DuplicateUser,
};
