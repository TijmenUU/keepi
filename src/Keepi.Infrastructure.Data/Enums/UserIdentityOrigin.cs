namespace Keepi.Infrastructure.Data.Enums;

internal enum UserIdentityOrigin
{
  Unknown,
  GitHub,
}

internal static class UserIdentityOriginExtensions
{
  public static Core.Users.UserIdentityOrigin MapToDomainModel(this UserIdentityOrigin value)
  {
    return value switch
    {
      UserIdentityOrigin.GitHub => Core.Users.UserIdentityOrigin.GitHub,
      _ => throw new ArgumentOutOfRangeException(paramName: nameof(value), message: $"Value {value} does not exist in the domain")
    };
  }
}