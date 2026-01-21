namespace Keepi.Api.Users.Get;

public sealed record GetUserResponse(
    int Id,
    string Name,
    string EmailAddress,
    GetUserResponsePermission EntriesPermission,
    GetUserResponsePermission ExportsPermission,
    GetUserResponsePermission ProjectsPermission,
    GetUserResponsePermission UsersPermission
);

public enum GetUserResponsePermission
{
    None = 0,
    Read,
    ReadAndModify,
}
