namespace Keepi.Core.UserEntryCategories;

public enum CreateUserEntryCategoryUseCaseError
{
    Unknown,
    MalformedName,
    DuplicateName,
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
        bool enabled,
        DateOnly? activeFrom,
        DateOnly? activeTo,
        CancellationToken cancellationToken
    );
}

internal class CreateUserEntryCategoryUseCase(IStoreUserEntryCategory storeUserEntryCategory)
    : ICreateUserEntryCategoryUseCase
{
    public async Task<
        IValueOrErrorResult<
            CreateUserEntryCategoryUseCaseResult,
            CreateUserEntryCategoryUseCaseError
        >
    > Execute(
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

        var createResult = await storeUserEntryCategory.Execute(
            userId: userId,
            name: name,
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
