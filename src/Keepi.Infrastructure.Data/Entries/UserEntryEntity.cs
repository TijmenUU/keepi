#nullable disable
using Keepi.Infrastructure.Data.UserEntryCategories;
using Keepi.Infrastructure.Data.Users;
using Microsoft.EntityFrameworkCore;

namespace Keepi.Infrastructure.Data.Entries;

[Index(nameof(Date))]
internal sealed class UserEntryEntity
{
  public int Id { get; set; }

  public int UserId { get; set; }
  public UserEntity User { get; set; }

  public int UserEntryCategoryId { get; set; }
  public UserEntryCategoryEntity UserEntryCategory { get; set; }

  public DateOnly Date { get; set; }
  public int Minutes { get; set; }
  public string Remark { get; set; }
}