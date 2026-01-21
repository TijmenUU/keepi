namespace Keepi.Api.Users.UpdatePermissions;

public sealed class UpdateUserPermissionsRequest
{
    public UpdateUserPermissionsRequestPermission? EntriesPermission { get; set; }
    public UpdateUserPermissionsRequestPermission? ExportsPermission { get; set; }
    public UpdateUserPermissionsRequestPermission? ProjectsPermission { get; set; }
    public UpdateUserPermissionsRequestPermission? UsersPermission { get; set; }
}

public enum UpdateUserPermissionsRequestPermission
{
    None = 0,
    Read,
    ReadAndModify,
}
