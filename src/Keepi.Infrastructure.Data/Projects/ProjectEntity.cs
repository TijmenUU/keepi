#nullable disable
using System.ComponentModel.DataAnnotations;
using Keepi.Infrastructure.Data.InvoiceItems;
using Keepi.Infrastructure.Data.Users;
using Microsoft.EntityFrameworkCore;

namespace Keepi.Infrastructure.Data.Projects;

[Index(nameof(Name), IsUnique = true)]
internal sealed class ProjectEntity
{
    public int Id { get; set; }

    [Required, MaxLength(Core.Projects.ProjectEntity.NameMaxLength)]
    public string Name { get; set; }
    public bool Enabled { get; set; }

    public List<ProjectEntityUserEntity> ProjectUsers { get; set; }
    public List<UserEntity> Users { get; set; }
    public List<InvoiceItemEntity> InvoiceItems { get; set; }
}
