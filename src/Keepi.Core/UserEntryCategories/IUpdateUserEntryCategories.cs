namespace Keepi.Core.UserEntryCategories;

public enum UpdateUserEntryCategoriesError
{
    Unknown,
    UserEntryCategoryDoesNotExist,
    DuplicateName,
}

public interface IUpdateUserEntryCategories
{
    Task<IMaybeErrorResult<UpdateUserEntryCategoriesError>> Execute(
        int userId,
        UserEntryCategoryEntity[] entities,
        CancellationToken cancellationToken
    );
}
