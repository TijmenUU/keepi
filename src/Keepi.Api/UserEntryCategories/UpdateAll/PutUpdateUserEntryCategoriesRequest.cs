namespace Keepi.Api.UserEntryCategories.UpdateAll;

public class PutUpdateUserEntryCategoriesRequest
{
    public PutUpdateUserEntryCategoriesRequestCategory?[]? UserEntryCategories { get; set; }
}

public class PutUpdateUserEntryCategoriesRequestCategory
{
    public int? Id { get; set; }
    public string? Name { get; set; }
    public int? Ordinal { get; set; }
    public bool? Enabled { get; set; }

    // yyyy-MM-dd
    public string? ActiveFrom { get; set; }

    // yyyy-MM-dd
    public string? ActiveTo { get; set; }
}
