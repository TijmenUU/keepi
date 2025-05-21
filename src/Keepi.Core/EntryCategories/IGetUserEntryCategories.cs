namespace Keepi.Core.EntryCategories;

public interface IGetUserEntryCategories
{
  Task<EntryCategoryEntity[]> Execute(int userId, CancellationToken cancellationToken);
}