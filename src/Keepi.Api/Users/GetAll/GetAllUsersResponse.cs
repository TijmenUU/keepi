namespace Keepi.Api.Users.GetAll;

public sealed record GetAllUsersResponse(GetAllUsersResponseUser[] Users);

public sealed record GetAllUsersResponseUser(int Id, string Name, string EmailAddress);
