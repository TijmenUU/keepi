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
    int Id,
    int UserId,
    string UserName,
    DateOnly Date,
    int ProjectId,
    string ProjectName,
    int InvoiceItemId,
    string InvoiceItemName,
    int Minutes,
    string Remark
);
