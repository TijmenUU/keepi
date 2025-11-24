#nullable disable
using System.ComponentModel.DataAnnotations;
using Keepi.Infrastructure.Data.Entries;
using Keepi.Infrastructure.Data.Projects;
using Keepi.Infrastructure.Data.UserInvoiceItemCustomizations;
using Microsoft.EntityFrameworkCore;

namespace Keepi.Infrastructure.Data.InvoiceItems;

[Index(nameof(Name), nameof(ProjectId), IsUnique = true)]
internal sealed class InvoiceItemEntity
{
    public int Id { get; set; }

    [Required, MaxLength(Core.InvoiceItems.InvoiceItemEntity.NameMaxLength)]
    public string Name { get; set; }

    public int ProjectId { get; set; }

    [Required]
    public ProjectEntity Project { get; set; }

    public List<UserEntryEntity> UserEntries { get; set; }
    public List<UserInvoiceItemCustomizationEntity> UserInvoiceItemCustomizations { get; set; }
}
