using Keepi.Core.Users;

namespace Keepi.Core.Exports;

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
    UnauthorizedUser,
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
        if (!userSuccessResult.ExportsPermission.CanRead())
        {
            return Result.Failure<IAsyncEnumerable<ExportUserEntry>, ExportUserEntriesUseCaseError>(
                ExportUserEntriesUseCaseError.UnauthorizedUser
            );
        }

        if (start >= stop)
        {
            return Result.Failure<IAsyncEnumerable<ExportUserEntry>, ExportUserEntriesUseCaseError>(
                ExportUserEntriesUseCaseError.StartGreaterThanStop
            );
        }

        return Result.Success<IAsyncEnumerable<ExportUserEntry>, ExportUserEntriesUseCaseError>(
            getExportUserEntries.Execute(
                start: start,
                stop: stop,
                cancellationToken: cancellationToken
            )
        );
    }
}
