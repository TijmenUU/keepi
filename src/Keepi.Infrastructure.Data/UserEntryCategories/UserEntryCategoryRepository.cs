using EntityFramework.Exceptions.Common;
using Keepi.Core;
using Keepi.Core.UserEntryCategories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Keepi.Infrastructure.Data.UserEntryCategories;

internal class UserEntryCategoryRepository(
    DatabaseContext databaseContext,
    ILogger<UserEntryCategoryRepository> logger
)
    : IGetUserUserEntryCategories,
        IStoreUserEntryCategory,
        IUpdateUserEntryCategory,
        IDeleteUserEntryCategory,
        IGetUserEntryCategoryIdByOrdinal
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
        IValueOrErrorResult<
            Core.UserEntryCategories.UserEntryCategoryEntity,
            StoreUserEntryCategoryError
        >
    > IStoreUserEntryCategory.Execute(
        int userId,
        string name,
        int ordinal,
        bool enabled,
        DateOnly? activeFrom,
        DateOnly? activeTo,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var entity = new UserEntryCategoryEntity
            {
                Name = name,
                Enabled = enabled,
                Ordinal = ordinal,
                ActiveFrom = activeFrom,
                ActiveTo = activeTo,
                UserId = userId,
            };
            databaseContext.Add(entity);
            await databaseContext.SaveChangesAsync(cancellationToken: cancellationToken);

            return ValueOrErrorResult<
                Core.UserEntryCategories.UserEntryCategoryEntity,
                StoreUserEntryCategoryError
            >.CreateSuccess(
                new Core.UserEntryCategories.UserEntryCategoryEntity(
                    id: entity.Id,
                    ordinal: entity.Ordinal,
                    name: entity.Name,
                    enabled: entity.Enabled,
                    activeFrom: entity.ActiveFrom,
                    activeTo: entity.ActiveTo
                )
            );
        }
        // This is a bit of a rough catch as it is not known what caused the
        // exception. Sqlite does not provide the exact constraint nor column name
        // so for now this seems all that can be done.
        catch (UniqueConstraintException)
        {
            return ValueOrErrorResult<
                Core.UserEntryCategories.UserEntryCategoryEntity,
                StoreUserEntryCategoryError
            >.CreateFailure(StoreUserEntryCategoryError.DuplicateName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error whilst storing new entry category");
            return ValueOrErrorResult<
                Core.UserEntryCategories.UserEntryCategoryEntity,
                StoreUserEntryCategoryError
            >.CreateFailure(StoreUserEntryCategoryError.Unknown);
        }
    }

    async Task<IMaybeErrorResult<UpdateUserEntryCategoryError>> IUpdateUserEntryCategory.Execute(
        int userEntryCategoryId,
        int userId,
        string name,
        int ordinal,
        bool enabled,
        DateOnly? activeFrom,
        DateOnly? activeTo,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var entity = databaseContext.UserEntryCategories.SingleOrDefault(ec =>
                ec.Id == userEntryCategoryId
            );
            if (entity == null)
            {
                return MaybeErrorResult<UpdateUserEntryCategoryError>.CreateFailure(
                    UpdateUserEntryCategoryError.UserEntryCategoryDoesNotExist
                );
            }
            if (entity.UserId != userId)
            {
                return MaybeErrorResult<UpdateUserEntryCategoryError>.CreateFailure(
                    UpdateUserEntryCategoryError.UserEntryCategoryBelongsToOtherUser
                );
            }

            entity.Name = name;
            entity.Ordinal = ordinal;
            entity.Enabled = enabled;
            entity.ActiveFrom = activeFrom;
            entity.ActiveTo = activeTo;
            await databaseContext.SaveChangesAsync(cancellationToken: cancellationToken);

            return MaybeErrorResult<UpdateUserEntryCategoryError>.CreateSuccess();
        }
        // This is a bit of a rough catch as it is not known what caused the
        // exception. Sqlite does not provide the exact constraint nor column name
        // so for now this seems all that can be done.
        catch (UniqueConstraintException)
        {
            return MaybeErrorResult<UpdateUserEntryCategoryError>.CreateFailure(
                UpdateUserEntryCategoryError.DuplicateName
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error whilst updating existing entry category");
            return MaybeErrorResult<UpdateUserEntryCategoryError>.CreateFailure(
                UpdateUserEntryCategoryError.Unknown
            );
        }
    }

    async Task<IMaybeErrorResult<DeleteUserEntryCategoryError>> IDeleteUserEntryCategory.Execute(
        int userEntryCategoryId,
        int userId,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var entity = databaseContext.UserEntryCategories.SingleOrDefault(ec =>
                ec.Id == userEntryCategoryId
            );
            if (entity == null)
            {
                return MaybeErrorResult<DeleteUserEntryCategoryError>.CreateFailure(
                    DeleteUserEntryCategoryError.UserEntryCategoryDoesNotExist
                );
            }
            if (entity.UserId != userId)
            {
                return MaybeErrorResult<DeleteUserEntryCategoryError>.CreateFailure(
                    DeleteUserEntryCategoryError.UserEntryCategoryBelongsToOtherUser
                );
            }

            databaseContext.Remove(entity);
            await databaseContext.SaveChangesAsync(cancellationToken: cancellationToken);

            return MaybeErrorResult<DeleteUserEntryCategoryError>.CreateSuccess();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error whilst deleting entry category");
            return MaybeErrorResult<DeleteUserEntryCategoryError>.CreateFailure(
                DeleteUserEntryCategoryError.Unknown
            );
        }
    }

    async Task<int?> IGetUserEntryCategoryIdByOrdinal.Execute(
        int userId,
        int ordinal,
        CancellationToken cancellationToken
    )
    {
        return (
            await databaseContext
                .UserEntryCategories.Where(uec => uec.UserId == userId && uec.Ordinal == ordinal)
                .Select(uec => new { uec.Id })
                .FirstOrDefaultAsync(cancellationToken: cancellationToken)
        )?.Id;
    }
}
