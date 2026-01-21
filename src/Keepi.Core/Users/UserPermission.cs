namespace Keepi.Core.Users;

public enum UserPermission
{
    None = 0,
    Read,
    ReadAndModify,
}

internal static class UserPermissionExtensions
{
    public static bool CanRead(this UserPermission permission) =>
        permission switch
        {
            UserPermission.Read => true,
            UserPermission.ReadAndModify => true,
            _ => false,
        };

    public static bool CanModify(this UserPermission permission) =>
        permission switch
        {
            UserPermission.Read => false,
            UserPermission.ReadAndModify => true,
            _ => false,
        };
}
