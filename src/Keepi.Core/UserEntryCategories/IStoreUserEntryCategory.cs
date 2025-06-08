namespace Keepi.Core.UserEntryCategories;

public enum StoreUserEntryCategoryError
{
    Unknown,
    DuplicateName,
}

public interface IStoreUserEntryCategory
{
    Task<IValueOrErrorResult<UserEntryCategoryEntity, StoreUserEntryCategoryError>> Execute(
        int userId,
        string name,
        bool enabled,
        DateOnly? activeFrom,
        DateOnly? activeTo,
        CancellationToken cancellationToken
    );
}
