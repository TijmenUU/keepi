using Keepi.Core.Repositories;

namespace Keepi.Core.UseCases;

public enum DeleteEntryCategoryUseCaseError
{
  Unknown,
  UnknownEntryCategory,
}

public interface IDeleteEntryCategoryUseCase
{
  Task<IMaybeErrorResult<DeleteEntryCategoryUseCaseError>> Execute(
    int entryCategoryId,
    int userId,
    CancellationToken cancellationToken);
}

internal class DeleteEntryCategoryUseCase(IDeleteEntryCategory deleteEntryCategory)
 : IDeleteEntryCategoryUseCase
{
  public async Task<IMaybeErrorResult<DeleteEntryCategoryUseCaseError>> Execute(
    int entryCategoryId,
    int userId,
    CancellationToken cancellationToken)
  {
    var deleteResult = await deleteEntryCategory.Execute(
      entryCategoryId: entryCategoryId,
      userId: userId,
      cancellationToken: cancellationToken);

    if (deleteResult.TrySuccess(out var error))
    {
      return MaybeErrorResult<DeleteEntryCategoryUseCaseError>.CreateSuccess();
    }

    if (error == DeleteEntryCategoryError.EntryCategoryDoesNotExist ||
      error == DeleteEntryCategoryError.EntryCategoryBelongsToOtherUser)
    {
      return MaybeErrorResult<DeleteEntryCategoryUseCaseError>.CreateFailure(DeleteEntryCategoryUseCaseError.UnknownEntryCategory);
    }

    return MaybeErrorResult<DeleteEntryCategoryUseCaseError>.CreateFailure(DeleteEntryCategoryUseCaseError.Unknown);
  }
}