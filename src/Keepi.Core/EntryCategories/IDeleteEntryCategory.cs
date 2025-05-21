namespace Keepi.Core.EntryCategories;

public enum DeleteEntryCategoryError
{
  Unknown,
  EntryCategoryDoesNotExist,
  EntryCategoryBelongsToOtherUser,
}

public interface IDeleteEntryCategory
{
  Task<IMaybeErrorResult<DeleteEntryCategoryError>> Execute(
    int entryCategoryId,
    int userId,
    CancellationToken cancellationToken);
}