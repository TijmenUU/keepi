using System.Globalization;

namespace Keepi.Core;


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
}
