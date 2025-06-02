namespace Keepi.Api.UserEntryCategories.Create;

public class PostCreateUserUserEntryCategoryRequest
{
  public string? Name { get; set; }
  public bool? Enabled { get; set; }
  // yyyy-MM-dd
  public string? ActiveFrom { get; set; }
  // yyyy-MM-dd
  public string? ActiveTo { get; set; }
}