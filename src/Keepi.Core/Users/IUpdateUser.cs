namespace Keepi.Core.Users;

public interface IUpdateUser
{
    Task<IMaybeErrorResult<UpdateUserError>> Execute(
        int userId,
        string emailAddress,
        string name,
        CancellationToken cancellationToken
    );
}

public enum UpdateUserError
{
    Unknown,
    UnknownUserId,
    DuplicateUser,
};
