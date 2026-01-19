namespace Keepi.Core.Users;

public interface IUpdateUserInfo
{
    Task<IMaybeErrorResult<UpdateUserInfoError>> Execute(
        int userId,
        string emailAddress,
        string name,
        CancellationToken cancellationToken
    );
}

public enum UpdateUserInfoError
{
    Unknown,
    UnknownUserId,
    DuplicateUser,
};
