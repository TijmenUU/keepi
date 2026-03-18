using Keepi.Core.Entries;
using Keepi.Core.InvoiceItems;
using Keepi.Core.Projects;
using Keepi.Core.Users;

namespace Keepi.Core.Exports;

public interface IGetExportUserEntries
{
    IAsyncEnumerable<ExportUserEntry> Execute(
        DateOnly start,
        DateOnly stop,
        CancellationToken cancellationToken
    );
}

public record ExportUserEntry(
    UserEntryId Id,
    UserId UserId,
    UserName UserName,
    DateOnly Date,
    ProjectId ProjectId,
    ProjectName ProjectName,
    InvoiceItemId InvoiceItemId,
    InvoiceItemName InvoiceItemName,
    UserEntryMinutes Minutes,
    UserEntryRemark? Remark
);
