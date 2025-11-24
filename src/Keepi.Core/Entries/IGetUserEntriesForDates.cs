namespace Keepi.Core.Entries;

public interface IGetUserEntriesForDates
{
    Task<IValueOrErrorResult<GetUserEntriesForDatesResult, GetUserEntriesForDatesError>> Execute(
        int userId,
        DateOnly[] dates,
        CancellationToken cancellationToken
    );
}

public enum GetUserEntriesForDatesError
{
    Unknown,
}

public sealed record GetUserEntriesForDatesResult(GetUserEntriesForDatesResultEntry[] Entries);

public sealed record GetUserEntriesForDatesResultEntry(
    int Id,
    int InvoiceItemId,
    DateOnly Date,
    int Minutes,
    string? Remark
);
