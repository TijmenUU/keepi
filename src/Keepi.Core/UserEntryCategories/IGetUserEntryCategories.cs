namespace Keepi.Core.UserEntryCategories;

public interface IGetUserUserEntryCategories
{
    Task<UserEntryCategoryEntity[]> Execute(int userId, CancellationToken cancellationToken);
    Task<UserEntryCategoryEntity[]> Execute(
        int userId,
        int[] userEntryCategoryIds,
        CancellationToken cancellationToken
    );
}
