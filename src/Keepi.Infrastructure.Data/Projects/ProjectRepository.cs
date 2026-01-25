using System.Data;
using EntityFramework.Exceptions.Common;
using Keepi.Core;
using Keepi.Core.Projects;
using Keepi.Core.UserInvoiceItemCustomizations;
using Keepi.Core.UserProjects;
using Keepi.Infrastructure.Data.InvoiceItems;
using Keepi.Infrastructure.Data.UserInvoiceItemCustomizations;
using Keepi.Infrastructure.Data.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Keepi.Infrastructure.Data.Projects;

internal sealed class ProjectRepository(
    DatabaseContext databaseContext,
    ILogger<ProjectRepository> logger
)
    : IGetProjects,
        IDeleteProject,
        ISaveNewProject,
        IUpdateProject,
        IGetUserProjects,
        IOverwriteUserInvoiceItemCustomizations
{
    async Task<IValueOrErrorResult<GetProjectsResult, GetProjectsError>> IGetProjects.Execute(
        CancellationToken cancellationToken
    )
    {
        try
        {
            var projects = await databaseContext
                .Projects.Select(p => new GetProjectsResultProject(
                    Id: p.Id,
                    Name: p.Name,
                    Enabled: p.Enabled,
                    Users: p.Users.Select(u => new GetProjectsResultProjectUser(
                            Id: u.Id,
                            Name: u.Name
                        ))
                        .ToArray(),
                    InvoiceItems: p.InvoiceItems.Select(
                            i => new GetProjectsResultProjectInvoiceItem(Id: i.Id, Name: i.Name)
                        )
                        .ToArray()
                ))
                .AsNoTracking()
                .ToArrayAsync(cancellationToken: cancellationToken);

            return Result.Success<GetProjectsResult, GetProjectsError>(
                new GetProjectsResult(Projects: projects)
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error querying all projects");
            return Result.Failure<GetProjectsResult, GetProjectsError>(GetProjectsError.Unknown);
        }
    }

    async Task<IMaybeErrorResult<DeleteProjectError>> IDeleteProject.Execute(
        int projectId,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var deleteCount = await databaseContext
                .Projects.Where(p => p.Id == projectId)
                .ExecuteDeleteAsync(cancellationToken: cancellationToken);

            if (deleteCount == 1)
            {
                return Result.Success<DeleteProjectError>();
            }

            if (deleteCount < 1)
            {
                return Result.Failure(DeleteProjectError.UnknownProjectId);
            }

            logger.LogError(
                "Unexpected number of deletions {Count} for project {ProjectId}",
                deleteCount,
                projectId
            );
            return Result.Failure(DeleteProjectError.Unknown);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error deleting project {ProjectId}", projectId);
            return Result.Failure(DeleteProjectError.Unknown);
        }
    }

    async Task<IValueOrErrorResult<int, SaveNewProjectError>> ISaveNewProject.Execute(
        string name,
        bool enabled,
        int[] userIds,
        string[] invoiceItemNames,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var users = new List<UserEntity>();
            foreach (var id in userIds)
            {
                var entity = new UserEntity { Id = id };
                databaseContext.Attach(entity);

                users.Add(entity);
            }

            var project = new ProjectEntity
            {
                Name = name,
                Enabled = enabled,
                Users = users,
                InvoiceItems = invoiceItemNames
                    .Select(i => new InvoiceItemEntity { Name = i })
                    .ToList(),
            };
            databaseContext.Add(project);
            await databaseContext.SaveChangesAsync(cancellationToken: cancellationToken);

            return Result.Success<int, SaveNewProjectError>(project.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error saving new project {ProjectName}", name);

            if (ex is UniqueConstraintException)
            {
                // TODO This could also be a duplicate invoice item name
                return Result.Failure<int, SaveNewProjectError>(
                    SaveNewProjectError.DuplicateProjectName
                );
            }

            if (ex is ConstraintException)
            {
                return Result.Failure<int, SaveNewProjectError>(SaveNewProjectError.UnknownUserId);
            }

            return Result.Failure<int, SaveNewProjectError>(SaveNewProjectError.Unknown);
        }
    }

    async Task<IMaybeErrorResult<UpdateProjectError>> IUpdateProject.Execute(
        int id,
        string name,
        bool enabled,
        int[] userIds,
        (int? Id, string Name)[] invoiceItems,
        CancellationToken cancellationToken
    )
    {
        await using var transaction = await databaseContext.Database.BeginTransactionAsync(
            cancellationToken: cancellationToken
        );

        try
        {
            var project = await databaseContext
                .Projects.Include(p => p.ProjectUsers)
                .Include(p => p.InvoiceItems)
                .SingleOrDefaultAsync(p => p.Id == id, cancellationToken: cancellationToken);

            if (project == null)
            {
                return Result.Failure(UpdateProjectError.UnknownProjectId);
            }

            project.Name = name;
            project.Enabled = enabled;

            for (int i = project.ProjectUsers.Count - 1; i >= 0; --i)
            {
                var projectUser = project.ProjectUsers[i];
                if (!userIds.Contains(projectUser.UserId))
                {
                    databaseContext.Remove(projectUser);
                    project.ProjectUsers.RemoveRange(index: i, count: 1);

                    await databaseContext
                        .UserEntries.Where(ue =>
                            ue.UserId == projectUser.UserId && ue.InvoiceItem.ProjectId == id
                        )
                        .ExecuteDeleteAsync(cancellationToken: cancellationToken);
                    await databaseContext
                        .UserInvoiceItemCustomizations.Where(c =>
                            c.UserId == projectUser.UserId && c.InvoiceItem.ProjectId == id
                        )
                        .ExecuteDeleteAsync(cancellationToken: cancellationToken);
                }
            }
            foreach (var userId in userIds)
            {
                if (project.ProjectUsers.Any(u => u.UserId == userId))
                {
                    continue;
                }

                project.ProjectUsers.Add(
                    new ProjectEntityUserEntity { ProjectId = id, UserId = userId }
                );
            }

            for (int i = project.InvoiceItems.Count - 1; i >= 0; --i)
            {
                var invoiceItem = project.InvoiceItems[i];
                if (!invoiceItems.Any(ii => ii.Id == invoiceItem.Id))
                {
                    databaseContext.Remove(invoiceItem);
                    project.InvoiceItems.RemoveRange(index: i, count: 1);

                    await databaseContext
                        .UserEntries.Where(ue => ue.InvoiceItemId == invoiceItem.Id)
                        .ExecuteDeleteAsync(cancellationToken: cancellationToken);
                    await databaseContext
                        .UserInvoiceItemCustomizations.Where(c => c.InvoiceItemId == invoiceItem.Id)
                        .ExecuteDeleteAsync(cancellationToken: cancellationToken);
                }
            }
            foreach (var invoiceItem in invoiceItems)
            {
                if (invoiceItem.Id == null)
                {
                    project.InvoiceItems.Add(new InvoiceItemEntity { Name = invoiceItem.Name });
                }
                else
                {
                    var existingInvoiceItem = project.InvoiceItems.Single(i =>
                        i.Id == invoiceItem.Id
                    );
                    existingInvoiceItem.Name = invoiceItem.Name;
                }
            }

            await databaseContext.SaveChangesAsync(cancellationToken: cancellationToken);
            await transaction.CommitAsync(cancellationToken: cancellationToken);

            return Result.Success<UpdateProjectError>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error saving new project {ProjectName}", name);
            await transaction.RollbackAsync();

            if (ex is UniqueConstraintException)
            {
                // TODO This could also be a duplicate invoice item name
                return Result.Failure(UpdateProjectError.DuplicateProjectName);
            }

            if (ex is ConstraintException)
            {
                return Result.Failure(UpdateProjectError.UnknownUserId);
            }

            return Result.Failure(UpdateProjectError.Unknown);
        }
    }

    async Task<
        IValueOrErrorResult<GetUserProjectResult, GetUserProjectsError>
    > IGetUserProjects.Execute(int userId, CancellationToken cancellationToken)
    {
        try
        {
            var user = await databaseContext
                .Users.Include(u => u.Projects)
                    .ThenInclude(p => p.InvoiceItems)
                .Include(u => u.UserInvoiceItemCustomizations)
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .SingleAsync(u => u.Id == userId, cancellationToken: cancellationToken);

            return Result.Success<GetUserProjectResult, GetUserProjectsError>(
                new(
                    Projects: user.Projects.Select(p => new GetUserProjectResultProject(
                            Id: p.Id,
                            Name: p.Name,
                            Enabled: p.Enabled,
                            InvoiceItems: p.InvoiceItems.Select(
                                    i => new GetUserProjectResultProjectInvoiceItem(
                                        Id: i.Id,
                                        Name: i.Name
                                    )
                                )
                                .ToArray()
                        ))
                        .ToArray(),
                    Customizations: user.UserInvoiceItemCustomizations.Select(
                            c => new GetUserProjectResultInvoiceItemCustomization(
                                InvoiceItemId: c.InvoiceItemId,
                                Ordinal: c.Ordinal,
                                Color: c.Color == null ? null : Color.FromUint32(c.Color.Value)
                            )
                        )
                        .ToArray()
                )
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error querying projects for user {UserId}", userId);
            return Result.Failure<GetUserProjectResult, GetUserProjectsError>(
                GetUserProjectsError.Unknown
            );
        }
    }

    async Task<
        IMaybeErrorResult<OverwriteUserInvoiceItemCustomizationsError>
    > IOverwriteUserInvoiceItemCustomizations.Execute(
        OverwriteUserInvoiceItemCustomizationsInput input,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var invoiceItemIds = input.InvoiceItems.Select(i => i.InvoiceItemId).ToArray();
            await databaseContext
                .UserInvoiceItemCustomizations.Where(c => c.UserId == input.UserId)
                .Where(c => invoiceItemIds.Contains(c.InvoiceItemId))
                .ExecuteDeleteAsync(cancellationToken: cancellationToken);

            databaseContext.AddRange(
                input.InvoiceItems.Select(i => new UserInvoiceItemCustomizationEntity
                {
                    InvoiceItemId = i.InvoiceItemId,
                    UserId = input.UserId,
                    Ordinal = i.Ordinal,
                    Color = i.Color == null ? null : Color.ToUint32(i.Color),
                })
            );
            await databaseContext.SaveChangesAsync(cancellationToken: cancellationToken);

            return Result.Success<OverwriteUserInvoiceItemCustomizationsError>();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unexpected error overwriting user invoice item customizations for user {UserId}",
                input.UserId
            );

            if (ex is ConstraintException)
            {
                return Result.Failure(
                    OverwriteUserInvoiceItemCustomizationsError.UnknownInvoiceItemId
                );
            }

            return Result.Failure(OverwriteUserInvoiceItemCustomizationsError.Unknown);
        }
    }
}
