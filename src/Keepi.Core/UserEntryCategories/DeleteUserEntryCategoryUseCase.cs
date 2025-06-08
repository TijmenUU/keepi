namespace Keepi.Core.UserEntryCategories;

public enum DeleteUserEntryCategoryUseCaseError
{
    Unknown,
    UnknownUserEntryCategory,
}

public interface IDeleteUserEntryCategoryUseCase
{
    Task<IMaybeErrorResult<DeleteUserEntryCategoryUseCaseError>> Execute(
        int userEntryCategoryId,
        int userId,
        CancellationToken cancellationToken
    );
}

internal class DeleteUserEntryCategoryUseCase(IDeleteUserEntryCategory deleteUserEntryCategory)
    : IDeleteUserEntryCategoryUseCase
{
    public async Task<IMaybeErrorResult<DeleteUserEntryCategoryUseCaseError>> Execute(
        int userEntryCategoryId,
        int userId,
        CancellationToken cancellationToken
    )
    {
        var deleteResult = await deleteUserEntryCategory.Execute(
            userEntryCategoryId: userEntryCategoryId,
            userId: userId,
            cancellationToken: cancellationToken
        );

        if (deleteResult.TrySuccess(out var error))
        {
            return MaybeErrorResult<DeleteUserEntryCategoryUseCaseError>.CreateSuccess();
        }

        if (
            error == DeleteUserEntryCategoryError.UserEntryCategoryDoesNotExist
            || error == DeleteUserEntryCategoryError.UserEntryCategoryBelongsToOtherUser
        )
        {
            return MaybeErrorResult<DeleteUserEntryCategoryUseCaseError>.CreateFailure(
                DeleteUserEntryCategoryUseCaseError.UnknownUserEntryCategory
            );
        }

        return MaybeErrorResult<DeleteUserEntryCategoryUseCaseError>.CreateFailure(
            DeleteUserEntryCategoryUseCaseError.Unknown
        );
    }
}
