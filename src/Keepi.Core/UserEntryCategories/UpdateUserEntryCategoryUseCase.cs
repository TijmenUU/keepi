namespace Keepi.Core.UserEntryCategories;

public enum UpdateUserEntryCategoryUseCaseError
{
    Unknown,
    MalformedName,
    DuplicateName,
    InvalidActiveDateRange,
    UnknownUserEntryCategory,
}

public interface IUpdateUserEntryCategoryUseCase
{
    Task<IMaybeErrorResult<UpdateUserEntryCategoryUseCaseError>> Execute(
        int userEntryCategoryId,
        int userId,
        string name,
        bool enabled,
        DateOnly? activeFrom,
        DateOnly? activeTo,
        CancellationToken cancellationToken
    );
}

internal class UpdateUserEntryCategoryUseCase(IUpdateUserEntryCategory updateUserEntryCategory)
    : IUpdateUserEntryCategoryUseCase
{
    public async Task<IMaybeErrorResult<UpdateUserEntryCategoryUseCaseError>> Execute(
        int userEntryCategoryId,
        int userId,
        string name,
        bool enabled,
        DateOnly? activeFrom,
        DateOnly? activeTo,
        CancellationToken cancellationToken
    )
    {
        if (!UserEntryCategoryEntity.IsValidName(name))
        {
            return MaybeErrorResult<UpdateUserEntryCategoryUseCaseError>.CreateFailure(
                UpdateUserEntryCategoryUseCaseError.MalformedName
            );
        }

        if (!UserEntryCategoryEntity.IsValidActiveDateRange(from: activeFrom, to: activeTo))
        {
            return MaybeErrorResult<UpdateUserEntryCategoryUseCaseError>.CreateFailure(
                UpdateUserEntryCategoryUseCaseError.InvalidActiveDateRange
            );
        }

        var updateResult = await updateUserEntryCategory.Execute(
            userEntryCategoryId: userEntryCategoryId,
            userId: userId,
            name: name,
            enabled: enabled,
            activeFrom: activeFrom,
            activeTo: activeTo,
            cancellationToken: cancellationToken
        );

        if (updateResult.TrySuccess(out var error))
        {
            return MaybeErrorResult<UpdateUserEntryCategoryUseCaseError>.CreateSuccess();
        }

        if (
            error == UpdateUserEntryCategoryError.UserEntryCategoryDoesNotExist
            || error == UpdateUserEntryCategoryError.UserEntryCategoryBelongsToOtherUser
        )
        {
            return MaybeErrorResult<UpdateUserEntryCategoryUseCaseError>.CreateFailure(
                UpdateUserEntryCategoryUseCaseError.UnknownUserEntryCategory
            );
        }

        if (error == UpdateUserEntryCategoryError.DuplicateName)
        {
            return MaybeErrorResult<UpdateUserEntryCategoryUseCaseError>.CreateFailure(
                UpdateUserEntryCategoryUseCaseError.DuplicateName
            );
        }

        return MaybeErrorResult<UpdateUserEntryCategoryUseCaseError>.CreateFailure(
            UpdateUserEntryCategoryUseCaseError.Unknown
        );
    }
}
