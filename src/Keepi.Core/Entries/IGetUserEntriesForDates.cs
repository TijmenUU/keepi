namespace Keepi.Core.Entries;

public interface IGetUserEntriesForDates
{
  Task<UserEntryEntity[]> Execute(int userId, DateOnly[] dates, CancellationToken cancellationToken);
}