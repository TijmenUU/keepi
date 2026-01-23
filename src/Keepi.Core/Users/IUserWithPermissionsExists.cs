namespace Keepi.Core.Users;

public interface IUserWithPermissionsExists
{
    Task<IValueOrErrorResult<bool, UserWithPermissionsExistsError>> Execute(
        UserPermission entriesPermission,
        UserPermission exportsPermission,
        UserPermission projectsPermission,
        UserPermission usersPermission,
        CancellationToken cancellationToken
    );
}

public enum UserWithPermissionsExistsError
{
    Unknown = 0,
}
