using Keepi.Core.InvoiceItems;
using Keepi.Core.Users;

namespace Keepi.Core.Entries;

public interface IGetUserEntriesForDates
{
    Task<IValueOrErrorResult<GetUserEntriesForDatesResult, GetUserEntriesForDatesError>> Execute(
        UserId userId,
        DateOnly[] dates,
        CancellationToken cancellationToken
    );
}

public enum GetUserEntriesForDatesError
{
    Unknown,
    UnauthenticatedUser,
}

public sealed record GetUserEntriesForDatesResult(GetUserEntriesForDatesResultEntry[] Entries);

public sealed record GetUserEntriesForDatesResultEntry(
    UserEntryId Id,
    InvoiceItemId InvoiceItemId,
    DateOnly Date,
    UserEntryMinutes Minutes,
    UserEntryRemark? Remark
);
