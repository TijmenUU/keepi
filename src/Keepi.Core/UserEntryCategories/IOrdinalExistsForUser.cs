namespace Keepi.Core.UserEntryCategories;

public interface IGetUserEntryCategoryIdByOrdinal
{
    Task<int?> Execute(int userId, int ordinal, CancellationToken cancellationToken);
}
