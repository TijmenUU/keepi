#nullable disable
using System.ComponentModel.DataAnnotations;
using Keepi.Infrastructure.Data.Entries;
using Keepi.Infrastructure.Data.Users;
using Microsoft.EntityFrameworkCore;

namespace Keepi.Infrastructure.Data.UserEntryCategories;

[Index(nameof(Name), nameof(UserId), IsUnique = true)]
[Index(nameof(Ordinal), nameof(UserId), IsUnique = true)]
[Index(nameof(ActiveFrom), nameof(ActiveTo))]
internal sealed class UserEntryCategoryEntity
{
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string Name { get; set; }
    public int Ordinal { get; set; }
    public bool Enabled { get; set; }
    public DateOnly? ActiveFrom { get; set; }
    public DateOnly? ActiveTo { get; set; }

    public int UserId { get; set; }

    [Required]
    public UserEntity User { get; set; }

    public List<UserEntryEntity> UserEntries { get; set; }
}
