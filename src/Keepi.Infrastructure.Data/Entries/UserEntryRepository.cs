using System.Diagnostics;
using Keepi.Core;
using Keepi.Core.Entries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Keepi.Infrastructure.Data.Entries;

internal class UserEntryRepository(
    DatabaseContext databaseContext,
    ILogger<UserEntryRepository> logger
) : IOverwriteUserEntriesForDates, IGetUserEntriesForDates, IGetExportUserEntries
{
    async Task<
        IMaybeErrorResult<OverwriteUserEntriesForDatesError>
    > IOverwriteUserEntriesForDates.Execute(
        int userId,
        DateOnly[] dates,
        Core.Entries.UserEntryEntity[] userEntries,
        CancellationToken cancellationToken
    )
    {
        Debug.Assert(dates.Length > 0);

        try
        {
            await databaseContext.Database.BeginTransactionAsync(
                cancellationToken: cancellationToken
            );

            // TODO update entities instead of nuking everything?
            await databaseContext
                .UserEntries.Where(ue => ue.UserId == userId && dates.Contains(ue.Date))
                .ExecuteDeleteAsync(cancellationToken: cancellationToken);

            foreach (var entry in userEntries)
            {
                databaseContext.Add(
                    new UserEntryEntity
                    {
                        UserId = userId,
                        UserEntryCategoryId = entry.UserEntryCategoryId,
                        Date = entry.Date,
                        Minutes = entry.Minutes,
                        Remark = entry.Remark,
                    }
                );
            }
            await databaseContext.SaveChangesAsync(cancellationToken: cancellationToken);

            await databaseContext.Database.CommitTransactionAsync(
                cancellationToken: cancellationToken
            );

            return Result.Success<OverwriteUserEntriesForDatesError>();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unexpected error whilst overwriting user entries for user {UserId} from {FirstDate} to {LastDate}",
                userId,
                dates.FirstOrDefault(),
                dates.LastOrDefault()
            );
            return Result.Failure(OverwriteUserEntriesForDatesError.Unknown);
        }
        finally
        {
            if (databaseContext.Database.CurrentTransaction != null)
            {
                await databaseContext.Database.RollbackTransactionAsync(
                    cancellationToken: CancellationToken.None
                );
            }
        }
    }

    async Task<Core.Entries.UserEntryEntity[]> IGetUserEntriesForDates.Execute(
        int userId,
        DateOnly[] dates,
        CancellationToken cancellationToken
    )
    {
        Debug.Assert(dates.Length > 0);

        return (
            await databaseContext
                .UserEntries.Where(ue => ue.UserId == userId && dates.Contains(ue.Date))
                .ToArrayAsync(cancellationToken: cancellationToken)
        )
            .Select(ue => new Core.Entries.UserEntryEntity(
                id: ue.Id,
                userId: userId,
                userEntryCategoryId: ue.UserEntryCategoryId,
                date: ue.Date,
                minutes: ue.Minutes,
                remark: ue.Remark
            ))
            .ToArray();
    }

    IAsyncEnumerable<ExportUserEntry> IGetExportUserEntries.Execute(
        int userId,
        DateOnly start,
        DateOnly stop,
        CancellationToken cancellationToken
    )
    {
        Debug.Assert(start <= stop);

        return databaseContext
            .UserEntries.Where(e => e.UserId == userId)
            .Where(e => e.Date >= start && e.Date <= stop)
            .Select(ue => new ExportUserEntry(
                ue.Id,
                ue.Date,
                ue.UserEntryCategoryId,
                ue.UserEntryCategory.Name,
                ue.Minutes,
                ue.Remark
            ))
            .AsAsyncEnumerable();
    }
}
