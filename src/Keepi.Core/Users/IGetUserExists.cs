namespace Keepi.Core.Users;

public interface IGetUserExists
{
  Task<bool> Execute(string externalId, string emailAddress, CancellationToken cancellationToken);
}