namespace Keepi.Core.Entries;

public interface ISaveUserEntries
{
    Task<IMaybeErrorResult<SaveUserEntriesError>> Execute(
        SaveUserEntriesInput input,
        CancellationToken cancellationToken
    );
}

public enum SaveUserEntriesError
{
    Unknown,
}

public sealed record SaveUserEntriesInput(int UserId, SaveUserEntriesInputEntry[] Entries);

public sealed record SaveUserEntriesInputEntry(
    int InvoiceItemId,
    DateOnly Date,
    int Minutes,
    string? Remark
);
