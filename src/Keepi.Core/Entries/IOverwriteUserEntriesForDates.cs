namespace Keepi.Core.Entries;

public interface IOverwriteUserEntriesForDates
{
  Task<IMaybeErrorResult<OverwriteUserEntriesForDatesError>> Execute(
    int userId,
    DateOnly[] dates,
    UserEntryEntity[] userEntries,
    CancellationToken cancellationToken);
}

public enum OverwriteUserEntriesForDatesError
{
  Unknown,
}