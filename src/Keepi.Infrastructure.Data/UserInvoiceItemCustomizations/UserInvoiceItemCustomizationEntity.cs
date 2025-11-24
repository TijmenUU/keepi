#nullable disable
using Keepi.Infrastructure.Data.InvoiceItems;
using Keepi.Infrastructure.Data.Users;
using Microsoft.EntityFrameworkCore;

namespace Keepi.Infrastructure.Data.UserInvoiceItemCustomizations;

[Index(nameof(UserId), nameof(InvoiceItemId), IsUnique = true)]
internal sealed class UserInvoiceItemCustomizationEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public UserEntity User { get; set; }
    public int InvoiceItemId { get; set; }
    public InvoiceItemEntity InvoiceItem { get; set; }
    public int Ordinal { get; set; }
    public uint? Color { get; set; }
}
