namespace Keepi.Api.UserEntryCategories.GetAll;

public record GetUserUserEntryCategoriesResponse(
    GetUserUserEntryCategoriesResponseCategory[] Categories
);

public record GetUserUserEntryCategoriesResponseCategory(
    int Id,
    string Name,
    bool Enabled,
    DateOnly? ActiveFrom,
    DateOnly? ActiveTo
);
