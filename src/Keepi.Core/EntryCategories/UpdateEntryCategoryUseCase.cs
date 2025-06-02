namespace Keepi.Core.EntryCategories;

public enum UpdateEntryCategoryUseCaseError
{
  Unknown,
  MalformedName,
  DuplicateName,
  InvalidActiveDateRange,
  UnknownEntryCategory,
}

public interface IUpdateEntryCategoryUseCase
{
  Task<IMaybeErrorResult<UpdateEntryCategoryUseCaseError>> Execute(
    int entryCategoryId,
    int userId,
    string name,
    bool enabled,
    DateOnly? activeFrom,
    DateOnly? activeTo,
    CancellationToken cancellationToken);
}

internal class UpdateEntryCategoryUseCase(IUpdateEntryCategory updateEntryCategory)
 : IUpdateEntryCategoryUseCase
{
  public async Task<IMaybeErrorResult<UpdateEntryCategoryUseCaseError>> Execute(
    int entryCategoryId,
    int userId,
    string name,
    bool enabled,
    DateOnly? activeFrom,
    DateOnly? activeTo,
    CancellationToken cancellationToken)
  {
    if (!EntryCategoryEntity.IsValidName(name))
    {
      return MaybeErrorResult<UpdateEntryCategoryUseCaseError>.CreateFailure(UpdateEntryCategoryUseCaseError.MalformedName);
    }

    if (!EntryCategoryEntity.IsValidActiveDateRange(from: activeFrom, to: activeTo))
    {
      return MaybeErrorResult<UpdateEntryCategoryUseCaseError>.CreateFailure(UpdateEntryCategoryUseCaseError.InvalidActiveDateRange);
    }

    var updateResult = await updateEntryCategory.Execute(
      entryCategoryId: entryCategoryId,
      userId: userId,
      name: name,
      enabled: enabled,
      activeFrom: activeFrom,
      activeTo: activeTo,
      cancellationToken: cancellationToken);

    if (updateResult.TrySuccess(out var error))
    {
      return MaybeErrorResult<UpdateEntryCategoryUseCaseError>.CreateSuccess();
    }

    if (error == UpdateEntryCategoryError.EntryCategoryDoesNotExist ||
      error == UpdateEntryCategoryError.EntryCategoryBelongsToOtherUser)
    {
      return MaybeErrorResult<UpdateEntryCategoryUseCaseError>.CreateFailure(UpdateEntryCategoryUseCaseError.UnknownEntryCategory);
    }

    if (error == UpdateEntryCategoryError.DuplicateName)
    {
      return MaybeErrorResult<UpdateEntryCategoryUseCaseError>.CreateFailure(UpdateEntryCategoryUseCaseError.DuplicateName);
    }

    return MaybeErrorResult<UpdateEntryCategoryUseCaseError>.CreateFailure(UpdateEntryCategoryUseCaseError.Unknown);
  }
}