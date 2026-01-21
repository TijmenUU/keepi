namespace Keepi.Api.Users.GetAll;

public sealed record GetAllUsersResponse(GetAllUsersResponseUser[] Users);

public sealed record GetAllUsersResponseUser(
    int Id,
    string Name,
    string EmailAddress,
    GetAllUsersResponseUserPermission EntriesPermission,
    GetAllUsersResponseUserPermission ExportsPermission,
    GetAllUsersResponseUserPermission ProjectsPermission,
    GetAllUsersResponseUserPermission UsersPermission
);

public enum GetAllUsersResponseUserPermission
{
    None = 0,
    Read,
    ReadAndModify,
}
