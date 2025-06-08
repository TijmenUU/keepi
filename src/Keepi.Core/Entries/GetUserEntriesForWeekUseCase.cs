using System.Diagnostics;

namespace Keepi.Core.Entries;

public interface IGetUserEntriesForWeekUseCase
{
    Task<GetUserEntriesForWeekUseCaseOutput> Execute(
        int userId,
        int year,
        int weekNumber,
        CancellationToken cancellationToken
    );
}

internal class GetUserEntriesForWeekUseCase(IGetUserEntriesForDates getUserEntriesForDates)
    : IGetUserEntriesForWeekUseCase
{
    public async Task<GetUserEntriesForWeekUseCaseOutput> Execute(
        int userId,
        int year,
        int weekNumber,
        CancellationToken cancellationToken
    )
    {
        var dates = WeekNumberHelper.WeekNumberToDates(year: year, number: weekNumber);
        var entities = await getUserEntriesForDates.Execute(
            userId: userId,
            dates: dates,
            cancellationToken: cancellationToken
        );

        Debug.Assert(dates.Length == 7);

        return new(
            Monday: GetDayForEntitiesOnDate(date: dates[0], entities: entities),
            Tuesday: GetDayForEntitiesOnDate(date: dates[1], entities: entities),
            Wednesday: GetDayForEntitiesOnDate(date: dates[2], entities: entities),
            Thursday: GetDayForEntitiesOnDate(date: dates[3], entities: entities),
            Friday: GetDayForEntitiesOnDate(date: dates[4], entities: entities),
            Saturday: GetDayForEntitiesOnDate(date: dates[5], entities: entities),
            Sunday: GetDayForEntitiesOnDate(date: dates[6], entities: entities)
        );
    }

    private static GetUserEntriesForWeekUseCaseOutputDay GetDayForEntitiesOnDate(
        DateOnly date,
        UserEntryEntity[] entities
    )
    {
        return new(
            Entries: entities
                .Where(e => e.Date == date)
                .Select(e => new GetUserEntriesForWeekUseCaseOutputDayEntry(
                    EntryCategoryId: e.UserEntryCategoryId,
                    Minutes: e.Minutes,
                    Remark: e.Remark
                ))
                .ToArray()
        );
    }
}

public enum GetUserEntriesForWeekUseCaseError
{
    Unknown,
}

public record GetUserEntriesForWeekUseCaseOutput(
    GetUserEntriesForWeekUseCaseOutputDay Monday,
    GetUserEntriesForWeekUseCaseOutputDay Tuesday,
    GetUserEntriesForWeekUseCaseOutputDay Wednesday,
    GetUserEntriesForWeekUseCaseOutputDay Thursday,
    GetUserEntriesForWeekUseCaseOutputDay Friday,
    GetUserEntriesForWeekUseCaseOutputDay Saturday,
    GetUserEntriesForWeekUseCaseOutputDay Sunday
);

public record GetUserEntriesForWeekUseCaseOutputDay(
    GetUserEntriesForWeekUseCaseOutputDayEntry[] Entries
);

public record GetUserEntriesForWeekUseCaseOutputDayEntry(
    int EntryCategoryId,
    int Minutes,
    string? Remark
);
