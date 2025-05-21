namespace Keepi.Api.Users.Register;

public class PostRegisterUserResponse
{
  public PostRegisterUserResponse(
    PostRegisterUserResponseResult result)
  {
    Result = result;
  }

  public PostRegisterUserResponseResult Result { get; }
}

public enum PostRegisterUserResponseResult
{
  Created,
  UserAlreadyExists
}