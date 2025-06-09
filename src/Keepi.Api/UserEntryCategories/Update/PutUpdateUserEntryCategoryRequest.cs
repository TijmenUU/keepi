namespace Keepi.Api.UserEntryCategories.Update;

public class PutUpdateUserUserEntryCategoryRequest
{
    public string? Name { get; set; }
    public int? Ordinal { get; set; }
    public bool? Enabled { get; set; }

    // yyyy-MM-dd
    public string? ActiveFrom { get; set; }

    // yyyy-MM-dd
    public string? ActiveTo { get; set; }
}
