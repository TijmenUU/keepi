using System.Diagnostics;
using Keepi.Core;
using Keepi.Core.Entries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Keepi.Infrastructure.Data.Entries;

internal sealed class UserEntryRepository(
    DatabaseContext databaseContext,
    ILogger<UserEntryRepository> logger
) : ISaveUserEntries, IDeleteUserEntriesForDateRange, IGetUserEntriesForDates, IGetExportUserEntries
{
    async Task<IMaybeErrorResult<SaveUserEntriesError>> ISaveUserEntries.Execute(
        SaveUserEntriesInput input,
        CancellationToken cancellationToken
    )
    {
        try
        {
            foreach (var entry in input.Entries)
            {
                databaseContext.Add(
                    new UserEntryEntity
                    {
                        UserId = input.UserId,
                        InvoiceItemId = entry.InvoiceItemId,
                        Date = entry.Date,
                        Minutes = entry.Minutes,
                        Remark = entry.Remark,
                    }
                );
            }
            await databaseContext.SaveChangesAsync(cancellationToken: cancellationToken);

            return Result.Success<SaveUserEntriesError>();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unexpected error whilst saving user entries for user {UserId}",
                input.UserId
            );
            return Result.Failure(SaveUserEntriesError.Unknown);
        }
    }

    async Task<
        IMaybeErrorResult<DeleteUserEntriesForDateRangeError>
    > IDeleteUserEntriesForDateRange.Execute(
        DeleteUserEntriesForDateRangeInput input,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await databaseContext
                .UserEntries.Where(ue => ue.UserId == input.UserId)
                .Where(ue => ue.Date >= input.From && ue.Date <= input.ToInclusive)
                .Where(ue => input.ProjectIds.Contains(ue.InvoiceItem.ProjectId))
                .ExecuteDeleteAsync(cancellationToken: cancellationToken);

            return Result.Success<DeleteUserEntriesForDateRangeError>();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unexpected error whilst deleting user entries for user {UserId} from {FirstDate} to {LastDate}",
                input.UserId,
                input.From,
                input.ToInclusive
            );
            return Result.Failure(DeleteUserEntriesForDateRangeError.Unknown);
        }
    }

    async Task<
        IValueOrErrorResult<GetUserEntriesForDatesResult, GetUserEntriesForDatesError>
    > IGetUserEntriesForDates.Execute(
        int userId,
        DateOnly[] dates,
        CancellationToken cancellationToken
    )
    {
        Debug.Assert(dates.Length > 0);
        try
        {
            var entities = await databaseContext
                .UserEntries.AsNoTracking()
                .Where(ue => ue.UserId == userId && dates.Contains(ue.Date))
                .ToArrayAsync(cancellationToken: cancellationToken);

            return Result.Success<GetUserEntriesForDatesResult, GetUserEntriesForDatesError>(
                new(
                    entities
                        .Select(e => new GetUserEntriesForDatesResultEntry(
                            Id: e.Id,
                            InvoiceItemId: e.InvoiceItemId,
                            Date: e.Date,
                            Minutes: e.Minutes,
                            Remark: e.Remark
                        ))
                        .ToArray()
                )
            );
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unexpected exception when querying user {UserId} entries for {First} until {Last} date(s)",
                userId,
                dates.FirstOrDefault(),
                dates.LastOrDefault()
            );

            return Result.Failure<GetUserEntriesForDatesResult, GetUserEntriesForDatesError>(
                GetUserEntriesForDatesError.Unknown
            );
        }
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
            .UserEntries.AsNoTracking()
            .Where(e => e.UserId == userId)
            .Where(e => e.Date >= start && e.Date <= stop)
            .Select(ue => new ExportUserEntry(
                ue.Id,
                ue.Date,
                ue.InvoiceItem.Project.Id,
                ue.InvoiceItem.Project.Name,
                ue.InvoiceItem.Id,
                ue.InvoiceItem.Name,
                ue.Minutes,
                ue.Remark
            ))
            .AsAsyncEnumerable();
    }
}
