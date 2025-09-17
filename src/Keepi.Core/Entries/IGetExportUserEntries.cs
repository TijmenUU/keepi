namespace Keepi.Core.Entries;

public interface IGetExportUserEntries
{
    IAsyncEnumerable<ExportUserEntry> Execute(
        int userId,
        DateOnly start,
        DateOnly stop,
        CancellationToken cancellationToken
    );
}

public record ExportUserEntry(
    int Id,
    DateOnly Date,
    int UserEntryCategoryId,
    string UserEntryCategoryName,
    int Minutes,
    string Remark
);
