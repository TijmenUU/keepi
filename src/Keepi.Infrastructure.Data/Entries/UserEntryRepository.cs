using System.Diagnostics;
using Keepi.Core;
using Keepi.Core.Entries;
using Keepi.Core.Exports;
using Keepi.Core.InvoiceItems;
using Keepi.Core.Projects;
using Keepi.Core.Users;
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
                Debug.Assert(entry.Minutes.Value > 0);

                databaseContext.Add(
                    new UserEntryEntity
                    {
                        UserId = input.UserId.Value,
                        InvoiceItemId = entry.InvoiceItemId.Value,
                        Date = entry.Date,
                        Minutes = entry.Minutes.Value,
                        Remark = entry.Remark?.Value,
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
            var projectIds = input.ProjectIds.Select(p => p.Value).ToArray();
            await databaseContext
                .UserEntries.Where(ue => ue.UserId == input.UserId.Value)
                .Where(ue => ue.Date >= input.From && ue.Date <= input.ToInclusive)
                .Where(ue => projectIds.Contains(ue.InvoiceItem.ProjectId))
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
        UserId userId,
        DateOnly[] dates,
        CancellationToken cancellationToken
    )
    {
        Debug.Assert(dates.Length > 0);
        try
        {
            var entities = await databaseContext
                .UserEntries.AsNoTracking()
                .Where(ue => ue.UserId == userId.Value && dates.Contains(ue.Date))
                .ToArrayAsync(cancellationToken: cancellationToken);

            return Result.Success<GetUserEntriesForDatesResult, GetUserEntriesForDatesError>(
                new(
                    entities
                        .Select(e => new GetUserEntriesForDatesResultEntry(
                            Id: UserEntryId.From(e.Id),
                            InvoiceItemId: InvoiceItemId.From(e.InvoiceItemId),
                            Date: e.Date,
                            Minutes: UserEntryMinutes.From(e.Minutes),
                            Remark: e.Remark == null ? null : UserEntryRemark.From(e.Remark)
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
        DateOnly start,
        DateOnly stop,
        CancellationToken cancellationToken
    )
    {
        Debug.Assert(start <= stop);

        return databaseContext
            .UserEntries.AsNoTracking()
            .Where(e => e.Date >= start && e.Date <= stop)
            .Select(ue => new
            {
                Id = ue.Id,
                UserId = ue.UserId,
                UserName = ue.User.Name,
                Date = ue.Date,
                ProjectId = ue.InvoiceItem.Project.Id,
                ProjectName = ue.InvoiceItem.Project.Name,
                InvoiceItemId = ue.InvoiceItem.Id,
                InvoiceItemName = ue.InvoiceItem.Name,
                Minutes = ue.Minutes,
                Remark = (string?)ue.Remark,
            })
            .AsAsyncEnumerable()
            .Select(ue => new ExportUserEntry(
                UserEntryId.From(ue.Id),
                UserId.From(ue.UserId),
                UserName.From(ue.UserName),
                ue.Date,
                ProjectId.From(ue.ProjectId),
                ProjectName.From(ue.ProjectName),
                InvoiceItemId.From(ue.InvoiceItemId),
                InvoiceItemName.From(ue.InvoiceItemName),
                UserEntryMinutes.From(ue.Minutes),
                ue.Remark == null ? null : UserEntryRemark.From(ue.Remark)
            ));
    }
}
