using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Keepi.Core.Entries;

public interface IGetUserEntriesForWeekUseCase
{
    Task<
        IValueOrErrorResult<GetUserEntriesForWeekUseCaseOutput, GetUserEntriesForWeekUseCaseError>
    > Execute(int userId, int year, int weekNumber, CancellationToken cancellationToken);
}

internal sealed class GetUserEntriesForWeekUseCase(
    IGetUserEntriesForDates getUserEntriesForDates,
    ILogger<GetUserEntriesForWeekUseCase> logger
) : IGetUserEntriesForWeekUseCase
{
    public async Task<
        IValueOrErrorResult<GetUserEntriesForWeekUseCaseOutput, GetUserEntriesForWeekUseCaseError>
    > Execute(int userId, int year, int weekNumber, CancellationToken cancellationToken)
    {
        var dates = WeekNumberHelper.WeekNumberToDates(year: year, number: weekNumber);
        var getUserEntriesForDatesResult = await getUserEntriesForDates.Execute(
            userId: userId,
            dates: dates,
            cancellationToken: cancellationToken
        );
        if (!getUserEntriesForDatesResult.TrySuccess(out var successResult, out var errorResult))
        {
            logger.LogError(
                "Unexpected error {Error} whilst trying to fetch user entries for week {WeekNumber} {Year} for user {UserId}",
                errorResult,
                weekNumber,
                year,
                userId
            );
            return Result.Failure<
                GetUserEntriesForWeekUseCaseOutput,
                GetUserEntriesForWeekUseCaseError
            >(GetUserEntriesForWeekUseCaseError.Unknown);
        }

        Debug.Assert(dates.Length == 7);

        return Result.Success<
            GetUserEntriesForWeekUseCaseOutput,
            GetUserEntriesForWeekUseCaseError
        >(
            new(
                Monday: GetDayForEntitiesOnDate(date: dates[0], entries: successResult.Entries),
                Tuesday: GetDayForEntitiesOnDate(date: dates[1], entries: successResult.Entries),
                Wednesday: GetDayForEntitiesOnDate(date: dates[2], entries: successResult.Entries),
                Thursday: GetDayForEntitiesOnDate(date: dates[3], entries: successResult.Entries),
                Friday: GetDayForEntitiesOnDate(date: dates[4], entries: successResult.Entries),
                Saturday: GetDayForEntitiesOnDate(date: dates[5], entries: successResult.Entries),
                Sunday: GetDayForEntitiesOnDate(date: dates[6], entries: successResult.Entries)
            )
        );
    }

    private static GetUserEntriesForWeekUseCaseOutputDay GetDayForEntitiesOnDate(
        DateOnly date,
        GetUserEntriesForDatesResultEntry[] entries
    )
    {
        return new(
            Entries: entries
                .Where(e => e.Date == date)
                .Select(e => new GetUserEntriesForWeekUseCaseOutputDayEntry(
                    InvoiceItemId: e.InvoiceItemId,
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
    int InvoiceItemId,
    int Minutes,
    string? Remark
);
