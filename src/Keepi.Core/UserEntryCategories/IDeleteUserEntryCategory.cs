namespace Keepi.Core.UserEntryCategories;

public enum DeleteUserEntryCategoryError
{
  Unknown,
  UserEntryCategoryDoesNotExist,
  UserEntryCategoryBelongsToOtherUser,
}

public interface IDeleteUserEntryCategory
{
  Task<IMaybeErrorResult<DeleteUserEntryCategoryError>> Execute(
    int userEntryCategoryId,
    int userId,
    CancellationToken cancellationToken);
}