using EntityFramework.Exceptions.Common;
using Keepi.Core;
using Keepi.Core.UserEntryCategories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Keepi.Infrastructure.Data.UserEntryCategories;

internal class UserEntryCategoryRepository(
    DatabaseContext databaseContext,
    ILogger<UserEntryCategoryRepository> logger
) : IGetUserUserEntryCategories, IUpdateUserEntryCategories
{
    async Task<Core.UserEntryCategories.UserEntryCategoryEntity[]> IGetUserUserEntryCategories.Execute(
        int userId,
        CancellationToken cancellationToken
    )
    {
        var userEntryCategories = await databaseContext
            .UserEntryCategories.Where(c => c.UserId == userId)
            .ToArrayAsync(cancellationToken);

        return userEntryCategories.Select(MapDatabaseEntityToDomainEntity).ToArray();
    }

    async Task<Core.UserEntryCategories.UserEntryCategoryEntity[]> IGetUserUserEntryCategories.Execute(
        int userId,
        int[] userEntryCategoryIds,
        CancellationToken cancellationToken
    )
    {
        var userEntryCategories = await databaseContext
            .UserEntryCategories.Where(c =>
                c.UserId == userId && userEntryCategoryIds.Contains(c.Id)
            )
            .ToArrayAsync(cancellationToken);

        return userEntryCategories.Select(MapDatabaseEntityToDomainEntity).ToArray();
    }

    private static Core.UserEntryCategories.UserEntryCategoryEntity MapDatabaseEntityToDomainEntity(
        UserEntryCategoryEntity c
    ) =>
        new(
            id: c.Id,
            name: c.Name,
            ordinal: c.Ordinal,
            enabled: c.Enabled,
            activeFrom: c.ActiveFrom,
            activeTo: c.ActiveTo
        );

    async Task<
        IMaybeErrorResult<UpdateUserEntryCategoriesError>
    > IUpdateUserEntryCategories.Execute(
        int userId,
        Core.UserEntryCategories.UserEntryCategoryEntity[] entities,
        CancellationToken cancellationToken
    )
    {
        var transaction = await databaseContext.Database.BeginTransactionAsync(
            cancellationToken: cancellationToken
        );
        try
        {
            var existingEntities = await databaseContext
                .UserEntryCategories.Where(c => c.UserId == userId)
                .ToArrayAsync(cancellationToken);

            foreach (var entity in entities)
            {
                if (entity.Id > 0)
                {
                    var existingEntity = existingEntities.FirstOrDefault(e => e.Id == entity.Id);
                    if (existingEntity == null)
                    {
                        return MaybeErrorResult<UpdateUserEntryCategoriesError>.CreateFailure(
                            UpdateUserEntryCategoriesError.UserEntryCategoryDoesNotExist
                        );
                    }

                    existingEntity.Name = entity.Name;
                    existingEntity.Ordinal = entity.Ordinal;
                    existingEntity.Enabled = entity.Enabled;
                    existingEntity.ActiveFrom = entity.ActiveFrom;
                    existingEntity.ActiveTo = entity.ActiveTo;
                }
                else
                {
                    databaseContext.Add(
                        new UserEntryCategoryEntity
                        {
                            UserId = userId,
                            Name = entity.Name,
                            Ordinal = entity.Ordinal,
                            Enabled = entity.Enabled,
                            ActiveFrom = entity.ActiveFrom,
                            ActiveTo = entity.ActiveTo,
                        }
                    );
                }
            }
            foreach (var existingEntity in existingEntities)
            {
                if (!entities.Any(e => e.Id == existingEntity.Id))
                {
                    databaseContext.Remove(existingEntity);
                }
            }

            await databaseContext.SaveChangesAsync(cancellationToken: cancellationToken);
            await transaction.CommitAsync(cancellationToken: cancellationToken);

            return MaybeErrorResult<UpdateUserEntryCategoriesError>.CreateSuccess();
        }
        // This is a bit of a rough catch as it is not known what caused the
        // exception. Sqlite does not provide the exact constraint nor column name
        // so for now this seems all that can be done.
        catch (UniqueConstraintException)
        {
            return MaybeErrorResult<UpdateUserEntryCategoriesError>.CreateFailure(
                UpdateUserEntryCategoriesError.DuplicateName
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error whilst updating existing entry category");
            return MaybeErrorResult<UpdateUserEntryCategoriesError>.CreateFailure(
                UpdateUserEntryCategoriesError.Unknown
            );
        }
    }
}
