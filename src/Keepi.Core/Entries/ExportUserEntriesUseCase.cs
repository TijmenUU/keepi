using Keepi.Core.Users;

namespace Keepi.Core.Entries;

public interface IExportUserEntriesUseCase
{
    Task<
        IValueOrErrorResult<IAsyncEnumerable<ExportUserEntry>, ExportUserEntriesUseCaseError>
    > Execute(DateOnly start, DateOnly stop, CancellationToken cancellationToken);
}

public enum ExportUserEntriesUseCaseError
{
    Unknown = 0,
    StartGreaterThanStop,
    UnauthenticatedUser,
}

internal sealed class ExportUserEntriesUseCase(
    IResolveUser resolveUser,
    IGetExportUserEntries getExportUserEntries
) : IExportUserEntriesUseCase
{
    public async Task<
        IValueOrErrorResult<IAsyncEnumerable<ExportUserEntry>, ExportUserEntriesUseCaseError>
    > Execute(DateOnly start, DateOnly stop, CancellationToken cancellationToken)
    {
        var userResult = await resolveUser.Execute(cancellationToken: cancellationToken);
        if (!userResult.TrySuccess(out var userSuccessResult, out var userErrorResult))
        {
            return userErrorResult switch
            {
                ResolveUserError.UserNotAuthenticated => Result.Failure<
                    IAsyncEnumerable<ExportUserEntry>,
                    ExportUserEntriesUseCaseError
                >(ExportUserEntriesUseCaseError.UnauthenticatedUser),
                _ => Result.Failure<
                    IAsyncEnumerable<ExportUserEntry>,
                    ExportUserEntriesUseCaseError
                >(ExportUserEntriesUseCaseError.Unknown),
            };
        }

        if (start >= stop)
        {
            return Result.Failure<IAsyncEnumerable<ExportUserEntry>, ExportUserEntriesUseCaseError>(
                ExportUserEntriesUseCaseError.StartGreaterThanStop
            );
        }

        return Result.Success<IAsyncEnumerable<ExportUserEntry>, ExportUserEntriesUseCaseError>(
            getExportUserEntries.Execute(
                userId: userSuccessResult.Id,
                start: start,
                stop: stop,
                cancellationToken: cancellationToken
            )
        );
    }
}
