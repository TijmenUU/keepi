namespace Keepi.Api.UserCategories.Create;

public class PostCreateUserEntryCategoryRequest
{
  public string? Name { get; set; }
  public bool? Enabled { get; set; }
  // yyyy-MM-dd
  public string? ActiveFrom { get; set; }
  // yyyy-MM-dd
  public string? ActiveTo { get; set; }
}