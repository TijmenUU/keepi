namespace Keepi.Api.UserCategories.GetAll;

public record GetUserEntryCategoriesResponse(GetUserEntryCategoriesResponseCategory[] Categories);
public record GetUserEntryCategoriesResponseCategory(int Id, string Name, bool Enabled, DateOnly? ActiveFrom, DateOnly? ActiveTo);