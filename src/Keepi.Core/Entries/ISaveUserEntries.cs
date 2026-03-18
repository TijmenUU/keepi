using Keepi.Core.InvoiceItems;
using Keepi.Core.Users;

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

public sealed record SaveUserEntriesInput(UserId UserId, SaveUserEntriesInputEntry[] Entries);

public sealed record SaveUserEntriesInputEntry(
    InvoiceItemId InvoiceItemId,
    DateOnly Date,
    UserEntryMinutes Minutes,
    UserEntryRemark? Remark
);
