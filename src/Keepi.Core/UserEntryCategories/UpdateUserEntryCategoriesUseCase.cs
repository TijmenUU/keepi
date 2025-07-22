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
            return MaybeErrorResult<UpdateUserEntryCategoriesUseCaseError>.CreateFailure(
                UpdateUserEntryCategoriesUseCaseError.DuplicateId
            );
        }

        if (
            userEntryCategories.Select(c => c.Name).Distinct().Count() != userEntryCategories.Length
        )
        {
            return MaybeErrorResult<UpdateUserEntryCategoriesUseCaseError>.CreateFailure(
                UpdateUserEntryCategoriesUseCaseError.DuplicateName
            );
        }

        if (
            userEntryCategories.Select(c => c.Ordinal).Distinct().Count()
            != userEntryCategories.Length
        )
        {
            return MaybeErrorResult<UpdateUserEntryCategoriesUseCaseError>.CreateFailure(
                UpdateUserEntryCategoriesUseCaseError.DuplicateOrdinal
            );
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
            return MaybeErrorResult<UpdateUserEntryCategoriesUseCaseError>.CreateSuccess();
        }

        if (error == UpdateUserEntryCategoriesError.DuplicateName)
        {
            return MaybeErrorResult<UpdateUserEntryCategoriesUseCaseError>.CreateFailure(
                UpdateUserEntryCategoriesUseCaseError.DuplicateName
            );
        }
        if (error == UpdateUserEntryCategoriesError.UserEntryCategoryDoesNotExist)
        {
            return MaybeErrorResult<UpdateUserEntryCategoriesUseCaseError>.CreateFailure(
                UpdateUserEntryCategoriesUseCaseError.UserEntryCategoryDoesNotExist
            );
        }

        return MaybeErrorResult<UpdateUserEntryCategoriesUseCaseError>.CreateFailure(
            UpdateUserEntryCategoriesUseCaseError.Unknown
        );
    }

    private static IMaybeErrorResult<UpdateUserEntryCategoriesUseCaseError> ValidateUserEntryCategory(
        UpdateUserEntryCategoriesUseCaseInput category
    )
    {
        if (!UserEntryCategoryEntity.IsValidName(category.Name))
        {
            return MaybeErrorResult<UpdateUserEntryCategoriesUseCaseError>.CreateFailure(
                UpdateUserEntryCategoriesUseCaseError.MalformedName
            );
        }

        if (
            !UserEntryCategoryEntity.IsValidActiveDateRange(
                from: category.ActiveFrom,
                to: category.ActiveTo
            )
        )
        {
            return MaybeErrorResult<UpdateUserEntryCategoriesUseCaseError>.CreateFailure(
                UpdateUserEntryCategoriesUseCaseError.InvalidActiveDateRange
            );
        }

        return MaybeErrorResult<UpdateUserEntryCategoriesUseCaseError>.CreateSuccess();
    }
}
