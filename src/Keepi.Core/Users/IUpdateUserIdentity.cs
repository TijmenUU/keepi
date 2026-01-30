namespace Keepi.Core.Users;

public interface IUpdateUserIdentity
{
    Task<IMaybeErrorResult<UpdateUserIdentityError>> Execute(
        int userId,
        string emailAddress,
        string name,
        CancellationToken cancellationToken
    );
}

public enum UpdateUserIdentityError
{
    Unknown,
    UnknownUserId,
    DuplicateUser,
};
