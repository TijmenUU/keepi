namespace Keepi.Core.EntryCategories;

public enum UpdateEntryCategoryError
{
  Unknown,
  EntryCategoryDoesNotExist,
  EntryCategoryBelongsToOtherUser,
  DuplicateName,
}

public interface IUpdateEntryCategory
{
  Task<IMaybeErrorResult<UpdateEntryCategoryError>> Execute(
    int entryCategoryId,
    int userId,
    string name,
    bool enabled,
    DateOnly? activeFrom,
    DateOnly? activeTo,
    CancellationToken cancellationToken);
}