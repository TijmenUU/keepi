namespace Keepi.Core.UserEntryCategories;

public enum UpdateUserEntryCategoriesUseCaseError
{
    Unknown,
    MalformedName,
    DuplicateId,
    DuplicateName,
    DuplicateOrdinal,
    InvalidActiveDateRange,
    UserEntryCategoryDoesNotExist,
}

public record UpdateUserEntryCategoriesUseCaseInput(
    int? Id,
    string Name,
    int Ordinal,
    bool Enabled,
    DateOnly? ActiveFrom,
    DateOnly? ActiveTo
);

public interface IUpdateUserEntryCategoriesUseCase
{
    Task<IMaybeErrorResult<UpdateUserEntryCategoriesUseCaseError>> Execute(
        int userId,
        UpdateUserEntryCategoriesUseCaseInput[] userEntryCategories,
        CancellationToken cancellationToken
    );
}

internal class UpdateUserEntryCategoriesUseCase(
    IUpdateUserEntryCategories updateUserEntryCategories
) : IUpdateUserEntryCategoriesUseCase
{
    public async Task<IMaybeErrorResult<UpdateUserEntryCategoriesUseCaseError>> Execute(
        int userId,
        UpdateUserEntryCategoriesUseCaseInput[] userEntryCategories,
        CancellationToken cancellationToken
    )
    {
        foreach (var category in userEntryCategories)
        {
            var validationResult = ValidateUserEntryCategory(category);
            if (!validationResult.Succeeded)
            {
                return validationResult;
            }
        }

        if (
            userEntryCategories
                .Where(c => c.Id.HasValue)
                .GroupBy(c => c.Id)
                .Select(g => g.Count())
                .Any(c => c > 1)
        )
        {
            return Result.Failure(UpdateUserEntryCategoriesUseCaseError.DuplicateId);
        }

        if (
            userEntryCategories.Select(c => c.Name).Distinct().Count() != userEntryCategories.Length
        )
        {
            return Result.Failure(UpdateUserEntryCategoriesUseCaseError.DuplicateName);
        }

        if (
            userEntryCategories.Select(c => c.Ordinal).Distinct().Count()
            != userEntryCategories.Length
        )
        {
            return Result.Failure(UpdateUserEntryCategoriesUseCaseError.DuplicateOrdinal);
        }

        var updateResult = await updateUserEntryCategories.Execute(
            userId: userId,
            entities: userEntryCategories
                .Select(c => new UserEntryCategoryEntity(
                    id: c.Id ?? 0,
                    name: c.Name,
                    ordinal: c.Ordinal,
                    enabled: c.Enabled,
                    activeFrom: c.ActiveFrom,
                    activeTo: c.ActiveTo
                ))
                .ToArray(),
            cancellationToken: cancellationToken
        );

        if (updateResult.TrySuccess(out var error))
        {
            return Result.Success<UpdateUserEntryCategoriesUseCaseError>();
        }

        if (error == UpdateUserEntryCategoriesError.DuplicateName)
        {
            return Result.Failure(UpdateUserEntryCategoriesUseCaseError.DuplicateName);
        }
        if (error == UpdateUserEntryCategoriesError.UserEntryCategoryDoesNotExist)
        {
            return Result.Failure(
                UpdateUserEntryCategoriesUseCaseError.UserEntryCategoryDoesNotExist
            );
        }

        return Result.Failure(UpdateUserEntryCategoriesUseCaseError.Unknown);
    }

    private static IMaybeErrorResult<UpdateUserEntryCategoriesUseCaseError> ValidateUserEntryCategory(
        UpdateUserEntryCategoriesUseCaseInput category
    )
    {
        if (!UserEntryCategoryEntity.IsValidName(category.Name))
        {
            return Result.Failure(UpdateUserEntryCategoriesUseCaseError.MalformedName);
        }

        if (
            !UserEntryCategoryEntity.IsValidActiveDateRange(
                from: category.ActiveFrom,
                to: category.ActiveTo
            )
        )
        {
            return Result.Failure(UpdateUserEntryCategoriesUseCaseError.InvalidActiveDateRange);
        }

        return Result.Success<UpdateUserEntryCategoriesUseCaseError>();
    }
}
