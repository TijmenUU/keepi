#nullable disable
using System.ComponentModel.DataAnnotations;
using Keepi.Infrastructure.Data.InvoiceItems;
using Keepi.Infrastructure.Data.Users;
using Microsoft.EntityFrameworkCore;

namespace Keepi.Infrastructure.Data.Entries;

[Index(nameof(Date))]
internal sealed class UserEntryEntity
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public UserEntity User { get; set; }

    public int InvoiceItemId { get; set; }
    public InvoiceItemEntity InvoiceItem { get; set; }

    public DateOnly Date { get; set; }
    public int Minutes { get; set; }

    [MaxLength(Core.Entries.UserEntryEntity.RemarkMaxLength)]
    public string Remark { get; set; }
}
