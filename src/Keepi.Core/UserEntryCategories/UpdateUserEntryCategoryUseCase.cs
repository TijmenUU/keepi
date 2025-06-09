namespace Keepi.Core.UserEntryCategories;

public enum UpdateUserEntryCategoryUseCaseError
{
    Unknown,
    MalformedName,
    DuplicateName,
    DuplicateOrdinal,
    InvalidActiveDateRange,
    UnknownUserEntryCategory,
}

public interface IUpdateUserEntryCategoryUseCase
{
    Task<IMaybeErrorResult<UpdateUserEntryCategoryUseCaseError>> Execute(
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

internal class UpdateUserEntryCategoryUseCase(
    IUpdateUserEntryCategory updateUserEntryCategory,
    IGetUserEntryCategoryIdByOrdinal getUserEntryCategoryIdByOrdinal
) : IUpdateUserEntryCategoryUseCase
{
    public async Task<IMaybeErrorResult<UpdateUserEntryCategoryUseCaseError>> Execute(
        int userEntryCategoryId,
        int userId,
        string name,
        int ordinal,
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

        var existingUserEntryCategoryIdForOrdinal = await getUserEntryCategoryIdByOrdinal.Execute(
            userId: userId,
            ordinal: ordinal,
            cancellationToken: cancellationToken
        );
        if (
            existingUserEntryCategoryIdForOrdinal != null
            && existingUserEntryCategoryIdForOrdinal.Value != userEntryCategoryId
        )
        {
            return MaybeErrorResult<UpdateUserEntryCategoryUseCaseError>.CreateFailure(
                UpdateUserEntryCategoryUseCaseError.DuplicateOrdinal
            );
        }

        var updateResult = await updateUserEntryCategory.Execute(
            userEntryCategoryId: userEntryCategoryId,
            userId: userId,
            name: name,
            ordinal: ordinal,
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
