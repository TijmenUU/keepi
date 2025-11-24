namespace Keepi.Api.Users.Get;

public sealed record GetAllUsersResponse(GetAllUsersResponseUser[] Users);

public sealed record GetAllUsersResponseUser(int Id, string Name, string EmailAddress);
