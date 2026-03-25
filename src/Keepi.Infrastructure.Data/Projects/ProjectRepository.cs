using System.Data;
using EntityFramework.Exceptions.Common;
using Keepi.Core;
using Keepi.Core.Entries;
using Keepi.Core.InvoiceItems;
using Keepi.Core.Projects;
using Keepi.Core.UserInvoiceItemCustomizations;
using Keepi.Core.UserProjects;
using Keepi.Core.Users;
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
                    Id: ProjectId.From(p.Id),
                    Name: ProjectName.From(p.Name),
                    Enabled: p.Enabled,
                    Users: p.Users.Select(u => new GetProjectsResultProjectUser(
                            Id: UserId.From(u.Id),
                            Name: UserName.From(u.Name)
                        ))
                        .ToArray(),
                    InvoiceItems: p.InvoiceItems.Select(
                            i => new GetProjectsResultProjectInvoiceItem(
                                Id: InvoiceItemId.From(i.Id),
                                Name: InvoiceItemName.From(i.Name)
                            )
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
        ProjectId projectId,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var deleteCount = await databaseContext
                .Projects.Where(p => p.Id == projectId.Value)
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
        ProjectName name,
        bool enabled,
        UserId[] userIds,
        InvoiceItemName[] invoiceItemNames,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var users = new List<UserEntity>();
            foreach (var id in userIds)
            {
                var entity = new UserEntity { Id = id.Value };
                databaseContext.Attach(entity);

                users.Add(entity);
            }

            var project = new ProjectEntity
            {
                Name = name.Value,
                Enabled = enabled,
                Users = users,
                InvoiceItems = invoiceItemNames
                    .Select(i => new InvoiceItemEntity { Name = i.Value })
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
        ProjectId id,
        ProjectName name,
        bool enabled,
        UserId[] userIds,
        (InvoiceItemId? Id, InvoiceItemName Name)[] invoiceItems,
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
                .SingleOrDefaultAsync(p => p.Id == id.Value, cancellationToken: cancellationToken);

            if (project == null)
            {
                return Result.Failure(UpdateProjectError.UnknownProjectId);
            }

            project.Name = name.Value;
            project.Enabled = enabled;

            for (int i = project.ProjectUsers.Count - 1; i >= 0; --i)
            {
                var projectUser = project.ProjectUsers[i];
                if (!userIds.Contains(UserId.From(projectUser.UserId)))
                {
                    databaseContext.Remove(projectUser);
                    project.ProjectUsers.RemoveRange(index: i, count: 1);

                    await databaseContext
                        .UserEntries.Where(ue =>
                            ue.UserId == projectUser.UserId && ue.InvoiceItem.ProjectId == id.Value
                        )
                        .ExecuteDeleteAsync(cancellationToken: cancellationToken);
                    await databaseContext
                        .UserInvoiceItemCustomizations.Where(c =>
                            c.UserId == projectUser.UserId && c.InvoiceItem.ProjectId == id.Value
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
                    new ProjectEntityUserEntity { ProjectId = id.Value, UserId = userId.Value }
                );
            }

            for (int i = project.InvoiceItems.Count - 1; i >= 0; --i)
            {
                var invoiceItem = project.InvoiceItems[i];
                var invoiceItemId = InvoiceItemId.From(invoiceItem.Id);
                if (!invoiceItems.Any(ii => ii.Id == invoiceItemId))
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
                    project.InvoiceItems.Add(
                        new InvoiceItemEntity { Name = invoiceItem.Name.Value }
                    );
                }
                else
                {
                    var existingInvoiceItem = project.InvoiceItems.Single(i =>
                        i.Id == invoiceItem.Id.Value
                    );
                    existingInvoiceItem.Name = invoiceItem.Name.Value;
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
    > IGetUserProjects.Execute(UserId userId, CancellationToken cancellationToken)
    {
        try
        {
            var user = await databaseContext
                .Users.Include(u => u.Projects)
                    .ThenInclude(p => p.InvoiceItems)
                .Include(u => u.UserInvoiceItemCustomizations)
                .AsNoTracking()
                .SingleAsync(u => u.Id == userId.Value, cancellationToken: cancellationToken);

            return Result.Success<GetUserProjectResult, GetUserProjectsError>(
                new(
                    Projects: user.Projects.Select(p => new GetUserProjectResultProject(
                            Id: ProjectId.From(p.Id),
                            Name: ProjectName.From(p.Name),
                            Enabled: p.Enabled,
                            InvoiceItems: p.InvoiceItems.Select(
                                    i => new GetUserProjectResultProjectInvoiceItem(
                                        Id: InvoiceItemId.From(i.Id),
                                        Name: InvoiceItemName.From(i.Name)
                                    )
                                )
                                .ToArray()
                        ))
                        .ToArray(),
                    Customizations: user.UserInvoiceItemCustomizations.Select(
                            c => new GetUserProjectResultInvoiceItemCustomization(
                                InvoiceItemId: InvoiceItemId.From(c.InvoiceItemId),
                                Ordinal: UserInvoiceITemCustomizationOrdinal.From(c.Ordinal),
                                Color: c.Color == null ? null : Color.From(c.Color.Value)
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
            var invoiceItemIds = input.InvoiceItems.Select(i => i.InvoiceItemId.Value).ToArray();
            await databaseContext
                .UserInvoiceItemCustomizations.Where(c => c.UserId == input.UserId.Value)
                .Where(c => invoiceItemIds.Contains(c.InvoiceItemId))
                .ExecuteDeleteAsync(cancellationToken: cancellationToken);

            databaseContext.AddRange(
                input.InvoiceItems.Select(i => new UserInvoiceItemCustomizationEntity
                {
                    InvoiceItemId = i.InvoiceItemId.Value,
                    UserId = input.UserId.Value,
                    Ordinal = i.Ordinal.Value,
                    Color = i.Color?.Value,
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
