namespace Keepi.Core.UserEntryCategories;

public sealed class UserEntryCategoryEntity
{
  public UserEntryCategoryEntity(
    int id,
    string name,
    bool enabled,
    DateOnly? activeFrom,
    DateOnly? activeTo)
  {
    Id = id;
    Name = name;
    Enabled = enabled;
    ActiveFrom = activeFrom;
    ActiveTo = activeTo;
  }

  public int Id { get; }
  public string Name { get; set; }
  public bool Enabled { get; set; }
  public DateOnly? ActiveFrom { get; set; }
  public DateOnly? ActiveTo { get; set; }

  public static bool IsValidName(string? name)
  {
    return !string.IsNullOrWhiteSpace(name) && name.Length <= 64;
  }

  public static bool IsValidActiveDateRange(DateOnly? from, DateOnly? to)
  {
    return from == null || to == null || from < to;
  }
}