namespace Keepi.Core.UserEntryCategories;

public enum CreateUserEntryCategoryUseCaseError
{
    Unknown,
    MalformedName,
    DuplicateName,
    DuplicateOrdinal,
    InvalidActiveDateRange,
}

public record CreateUserEntryCategoryUseCaseResult(int UserEntryCategoryId);

public interface ICreateUserEntryCategoryUseCase
{
    Task<
        IValueOrErrorResult<
            CreateUserEntryCategoryUseCaseResult,
            CreateUserEntryCategoryUseCaseError
        >
    > Execute(
        int userId,
        string name,
        int ordinal,
        bool enabled,
        DateOnly? activeFrom,
        DateOnly? activeTo,
        CancellationToken cancellationToken
    );
}

internal class CreateUserEntryCategoryUseCase(
    IStoreUserEntryCategory storeUserEntryCategory,
    IGetUserEntryCategoryIdByOrdinal getUserEntryCategoryIdByOrdinal
) : ICreateUserEntryCategoryUseCase
{
    public async Task<
        IValueOrErrorResult<
            CreateUserEntryCategoryUseCaseResult,
            CreateUserEntryCategoryUseCaseError
        >
    > Execute(
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
            return ValueOrErrorResult<
                CreateUserEntryCategoryUseCaseResult,
                CreateUserEntryCategoryUseCaseError
            >.CreateFailure(CreateUserEntryCategoryUseCaseError.MalformedName);
        }

        if (!UserEntryCategoryEntity.IsValidActiveDateRange(from: activeFrom, to: activeTo))
        {
            return ValueOrErrorResult<
                CreateUserEntryCategoryUseCaseResult,
                CreateUserEntryCategoryUseCaseError
            >.CreateFailure(CreateUserEntryCategoryUseCaseError.InvalidActiveDateRange);
        }

        if (
            await getUserEntryCategoryIdByOrdinal.Execute(
                userId: userId,
                ordinal: ordinal,
                cancellationToken: cancellationToken
            ) != null
        )
        {
            return ValueOrErrorResult<
                CreateUserEntryCategoryUseCaseResult,
                CreateUserEntryCategoryUseCaseError
            >.CreateFailure(CreateUserEntryCategoryUseCaseError.DuplicateOrdinal);
        }

        var createResult = await storeUserEntryCategory.Execute(
            userId: userId,
            name: name,
            ordinal: ordinal,
            enabled: enabled,
            activeFrom: activeFrom,
            activeTo: activeTo,
            cancellationToken: cancellationToken
        );

        if (createResult.TrySuccess(out var success, out var error))
        {
            return ValueOrErrorResult<
                CreateUserEntryCategoryUseCaseResult,
                CreateUserEntryCategoryUseCaseError
            >.CreateSuccess(
                new CreateUserEntryCategoryUseCaseResult(UserEntryCategoryId: success.Id)
            );
        }

        if (error == StoreUserEntryCategoryError.DuplicateName)
        {
            return ValueOrErrorResult<
                CreateUserEntryCategoryUseCaseResult,
                CreateUserEntryCategoryUseCaseError
            >.CreateFailure(CreateUserEntryCategoryUseCaseError.DuplicateName);
        }

        return ValueOrErrorResult<
            CreateUserEntryCategoryUseCaseResult,
            CreateUserEntryCategoryUseCaseError
        >.CreateFailure(CreateUserEntryCategoryUseCaseError.Unknown);
    }
}
