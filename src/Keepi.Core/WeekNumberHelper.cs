using System.Globalization;

namespace Keepi.Core;

public record Week(int Year, int Number);

public static class WeekNumberHelper
{
    public static DateOnly[] WeekNumberToDates(int year, int number)
    {
        var monday = DateOnly.FromDateTime(
            ISOWeek.ToDateTime(year: year, week: number, dayOfWeek: DayOfWeek.Monday)
        );
        return
        [
            monday,
            monday.AddDays(1),
            monday.AddDays(2),
            monday.AddDays(3),
            monday.AddDays(4),
            monday.AddDays(5),
            monday.AddDays(6),
        ];
    }

    public static Week GetCurrentWeek() => GetWeekForDate(DateOnly.FromDateTime(DateTime.Today));

    public static Week GetNextWeek(Week week)
    {
        // This could be optimized since there are always weeks 1 to 52

        var monday = DateOnly.FromDateTime(
            ISOWeek.ToDateTime(year: week.Year, week: week.Number, dayOfWeek: DayOfWeek.Monday)
        );
        return GetWeekForDate(date: monday.AddDays(7));
    }

    public static Week GetPreviousWeek(Week week)
    {
        // This could be optimized since there are always weeks 1 to 52

        var monday = DateOnly.FromDateTime(
            ISOWeek.ToDateTime(year: week.Year, week: week.Number, dayOfWeek: DayOfWeek.Monday)
        );
        return GetWeekForDate(date: monday.AddDays(-1));
    }

    public static Week GetWeekForDate(DateOnly date) =>
        new(Year: ISOWeek.GetYear(date: date), Number: ISOWeek.GetWeekOfYear(date: date));
}
