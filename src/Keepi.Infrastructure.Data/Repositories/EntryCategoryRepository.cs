using EntityFramework.Exceptions.Common;
using Keepi.Core;
using Keepi.Core.EntryCategories;
using Microsoft.Extensions.Logging;

namespace Keepi.Infrastructure.Data.Repositories;

internal class EntryCategoryRepository(DatabaseContext databaseContext, ILogger<EntryCategoryRepository> logger)
 : IStoreEntryCategory, IUpdateEntryCategory, IDeleteEntryCategory
{
  async Task<IValueOrErrorResult<EntryCategoryEntity, StoreEntryCategoryError>> IStoreEntryCategory.Execute(
    int userId,
    string name,
    bool enabled,
    DateOnly? activeFrom,
    DateOnly? activeTo,
    CancellationToken cancellationToken)
  {
    try
    {
      var entity = new Entities.EntryCategoryEntity
      {
        Name = name,
        Enabled = enabled,
        ActiveFrom = activeFrom,
        ActiveTo = activeTo,
        UserId = userId,
      };
      databaseContext.Add(entity);
      await databaseContext.SaveChangesAsync(cancellationToken: cancellationToken);

      return ValueOrErrorResult<EntryCategoryEntity, StoreEntryCategoryError>.CreateSuccess(new EntryCategoryEntity(
        id: entity.Id,
        name: entity.Name,
        enabled: entity.Enabled,
        activeFrom: entity.ActiveFrom,
        activeTo: entity.ActiveTo
      ));
    }
    // This is a bit of a rough catch as it is not known what caused the
    // exception. Sqlite does not provide the exact constraint nor column name
    // so for now this seems all that can be done.
    catch (UniqueConstraintException)
    {
      return ValueOrErrorResult<EntryCategoryEntity, StoreEntryCategoryError>.CreateFailure(StoreEntryCategoryError.DuplicateName);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Unexpected error whilst storing new entry category");
      return ValueOrErrorResult<EntryCategoryEntity, StoreEntryCategoryError>.CreateFailure(StoreEntryCategoryError.Unknown);
    }
  }

  async Task<IMaybeErrorResult<UpdateEntryCategoryError>> IUpdateEntryCategory.Execute(
    int entryCategoryId,
    int userId,
    string name,
    bool enabled,
    DateOnly? activeFrom,
    DateOnly? activeTo,
    CancellationToken cancellationToken)
  {
    try
    {
      var entity = databaseContext.EntryCategories.SingleOrDefault(ec => ec.Id == entryCategoryId);
      if (entity == null)
      {
        return MaybeErrorResult<UpdateEntryCategoryError>.CreateFailure(UpdateEntryCategoryError.EntryCategoryDoesNotExist);
      }
      if (entity.UserId != userId)
      {
        return MaybeErrorResult<UpdateEntryCategoryError>.CreateFailure(UpdateEntryCategoryError.EntryCategoryBelongsToOtherUser);
      }

      entity.Name = name;
      entity.Enabled = enabled;
      entity.ActiveFrom = activeFrom;
      entity.ActiveTo = activeTo;
      await databaseContext.SaveChangesAsync(cancellationToken: cancellationToken);

      return MaybeErrorResult<UpdateEntryCategoryError>.CreateSuccess();
    }
    // This is a bit of a rough catch as it is not known what caused the
    // exception. Sqlite does not provide the exact constraint nor column name
    // so for now this seems all that can be done.
    catch (UniqueConstraintException)
    {
      return MaybeErrorResult<UpdateEntryCategoryError>.CreateFailure(UpdateEntryCategoryError.DuplicateName);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Unexpected error whilst updating existing entry category");
      return MaybeErrorResult<UpdateEntryCategoryError>.CreateFailure(UpdateEntryCategoryError.Unknown);
    }
  }

  async Task<IMaybeErrorResult<DeleteEntryCategoryError>> IDeleteEntryCategory.Execute(
    int entryCategoryId,
    int userId,
    CancellationToken cancellationToken)
  {
    try
    {
      var entity = databaseContext.EntryCategories.SingleOrDefault(ec => ec.Id == entryCategoryId);
      if (entity == null)
      {
        return MaybeErrorResult<DeleteEntryCategoryError>.CreateFailure(DeleteEntryCategoryError.EntryCategoryDoesNotExist);
      }
      if (entity.UserId != userId)
      {
        return MaybeErrorResult<DeleteEntryCategoryError>.CreateFailure(DeleteEntryCategoryError.EntryCategoryBelongsToOtherUser);
      }

      databaseContext.Remove(entity);
      await databaseContext.SaveChangesAsync(cancellationToken: cancellationToken);

      return MaybeErrorResult<DeleteEntryCategoryError>.CreateSuccess();
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Unexpected error whilst deleting entry category");
      return MaybeErrorResult<DeleteEntryCategoryError>.CreateFailure(DeleteEntryCategoryError.Unknown);
    }
  }
}