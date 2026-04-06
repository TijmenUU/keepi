using System.Globalization;

namespace Keepi.Core;

public sealed record Week(Year Year, WeekNumber Number)
{
    public static Week Today
    {
        get
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var weekNumber = ISOWeek.GetWeekOfYear(date: today);

            return new Week(Year: Year.From(today.Year), WeekNumber.From(weekNumber));
        }
    }

    public DateOnly[] ToDates()
    {
        var monday = DateOnly.FromDateTime(
            ISOWeek.ToDateTime(year: Year.Value, week: Number.Value, dayOfWeek: DayOfWeek.Monday)
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
}
