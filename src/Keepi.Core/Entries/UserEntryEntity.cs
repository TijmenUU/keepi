namespace Keepi.Core.Entries;

public sealed class UserEntryEntity
{
  public UserEntryEntity(
    int id,
    int userId,
    int userEntryCategoryId,
    DateOnly date,
    int minutes,
    string? remark)
  {
    Id = id;
    UserId = userId;
    UserEntryCategoryId = userEntryCategoryId;
    Date = date;
    Minutes = minutes;
    Remark = remark;
  }

  public int Id { get; }
  public int UserId { get; }
  public int UserEntryCategoryId { get; }
  public DateOnly Date { get; set; }
  public int Minutes { get; set; }
  public string? Remark { get; set; }
}