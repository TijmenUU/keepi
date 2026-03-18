using Keepi.Core.Projects;
using Keepi.Core.Users;

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
    UserId UserId,
    DateOnly From,
    DateOnly ToInclusive,
    ProjectId[] ProjectIds
);
