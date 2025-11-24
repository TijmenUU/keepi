namespace Keepi.Core.Entries;

public interface IDeleteUserEntriesForDateRange
{
    Task<IMaybeErrorResult<DeleteUserEntriesForDateRangeError>> Execute(
        DeleteUserEntriesForDateRangeInput input,
        CancellationToken cancellationToken
    );
}

public enum DeleteUserEntriesForDateRangeError
{
    Unknown,
}

public sealed record DeleteUserEntriesForDateRangeInput(
    int UserId,
    DateOnly From,
    DateOnly ToInclusive,
    int[] ProjectIds
);
