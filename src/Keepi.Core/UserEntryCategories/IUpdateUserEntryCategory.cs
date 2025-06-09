namespace Keepi.Core.UserEntryCategories;

public enum UpdateUserEntryCategoryError
{
    Unknown,
    UserEntryCategoryDoesNotExist,
    UserEntryCategoryBelongsToOtherUser,
    DuplicateName,
}

public interface IUpdateUserEntryCategory
{
    Task<IMaybeErrorResult<UpdateUserEntryCategoryError>> Execute(
        int userEntryCategoryId,
        int userId,
        string name,
        int ordinal,
        bool enabled,
        DateOnly? activeFrom,
        DateOnly? activeTo,
        CancellationToken cancellationToken
    );
}
