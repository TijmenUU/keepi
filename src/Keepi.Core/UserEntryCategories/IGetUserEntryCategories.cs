namespace Keepi.Core.UserEntryCategories;

public interface IGetUserUserEntryCategories
{
  Task<UserEntryCategoryEntity[]> Execute(int userId, CancellationToken cancellationToken);
}