namespace Keepi.Core.Users;

public interface ISaveNewUser
{
    Task<IMaybeErrorResult<SaveNewUserError>> Execute(
        UserExternalId externalId,
        EmailAddress emailAddress,
        UserName name,
        UserIdentityProvider userIdentityProvider,
        UserPermission entriesPermission,
        UserPermission exportsPermission,
        UserPermission projectsPermission,
        UserPermission usersPermission,
        CancellationToken cancellationToken
    );
}

public enum SaveNewUserError
{
    Unknown,
    DuplicateUser,
};
