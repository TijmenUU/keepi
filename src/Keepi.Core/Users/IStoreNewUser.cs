namespace Keepi.Core.Users;

public enum StoreNewUserError
{
  Unknown,
  DuplicateUser,
};

public interface IStoreNewUser
{
  Task<IMaybeErrorResult<StoreNewUserError>> Execute(
    string externalId,
    string emailAddress,
    string name,
    UserIdentityProvider userIdentityProvider,
    CancellationToken cancellationToken);
}