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
    StartGreaterThanStop,
}

internal class ExportUserEntriesUseCase(IGetExportUserEntries getExportUserEntries)
    : IExportUserEntriesUseCase
{
    public IValueOrErrorResult<
        IAsyncEnumerable<ExportUserEntry>,
        ExportUserEntriesUseCaseError
    > Execute(int userId, DateOnly start, DateOnly stop, CancellationToken cancellationToken)
    {
        if (start >= stop)
        {
            return ValueOrErrorResult<
                IAsyncEnumerable<ExportUserEntry>,
                ExportUserEntriesUseCaseError
            >.CreateFailure(ExportUserEntriesUseCaseError.StartGreaterThanStop);
        }

        return ValueOrErrorResult<
            IAsyncEnumerable<ExportUserEntry>,
            ExportUserEntriesUseCaseError
        >.CreateSuccess(
            getExportUserEntries.Execute(
                userId: userId,
                start: start,
                stop: stop,
                cancellationToken: cancellationToken
            )
        );
    }
}
