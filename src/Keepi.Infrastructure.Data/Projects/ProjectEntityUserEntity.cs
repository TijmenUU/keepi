#nullable disable
using Keepi.Infrastructure.Data.Users;
using Microsoft.EntityFrameworkCore;

namespace Keepi.Infrastructure.Data.Projects;

[Index(nameof(ProjectId), nameof(UserId), IsUnique = true)]
internal sealed class ProjectEntityUserEntity
{
    public int Id { get; set; }

    public int ProjectId { get; set; }
    public ProjectEntity Project { get; set; }

    public int UserId { get; set; }
    public UserEntity User { get; set; }
}
