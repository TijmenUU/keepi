namespace Keepi.Core.Entries;

public interface IExportUserEntriesUseCase
{
    IValueOrErrorResult<IAsyncEnumerable<ExportUserEntry>, ExportUserEntriesUseCaseError> Execute(
        int userId,
        DateOnly start,
        DateOnly stop,
        CancellationToken cancellationToken
    );
}

public enum ExportUserEntriesUseCaseError
{
    Unknown = 0,
    StartGreaterThanStop,
}

internal sealed class ExportUserEntriesUseCase(IGetExportUserEntries getExportUserEntries)
    : IExportUserEntriesUseCase
{
    public IValueOrErrorResult<
        IAsyncEnumerable<ExportUserEntry>,
        ExportUserEntriesUseCaseError
    > Execute(int userId, DateOnly start, DateOnly stop, CancellationToken cancellationToken)
    {
        if (start >= stop)
        {
            return Result.Failure<IAsyncEnumerable<ExportUserEntry>, ExportUserEntriesUseCaseError>(
                ExportUserEntriesUseCaseError.StartGreaterThanStop
            );
        }

        return Result.Success<IAsyncEnumerable<ExportUserEntry>, ExportUserEntriesUseCaseError>(
            getExportUserEntries.Execute(
                userId: userId,
                start: start,
                stop: stop,
                cancellationToken: cancellationToken
            )
        );
    }
}
