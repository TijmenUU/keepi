namespace Keepi.Core.Users;

public interface IUpdateUserPermissions
{
    Task<IMaybeErrorResult<UpdateUserPermissionsError>> Execute(
        UserId userId,
        UserPermission entriesPermission,
        UserPermission exportsPermission,
        UserPermission projectsPermission,
        UserPermission usersPermission,
        CancellationToken cancellationToken
    );
}

public enum UpdateUserPermissionsError
{
    Unknown = 0,
    UnknownUserId,
}
