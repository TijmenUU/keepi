using Keepi.Core.Entries;

namespace Keepi.Core.Unit.Tests.Entries;

public class WeekNumberHelperTests
{
    [Fact]
    public void WeekNumberToDates_returns_expected_result_for_normal_week()
    {
        var dates = WeekNumberHelper.WeekNumberToDates(year: 2025, number: 13);
        dates.ShouldBeEquivalentTo(
            new[]
            {
                new DateOnly(year: 2025, month: 3, day: 24),
                new DateOnly(year: 2025, month: 3, day: 25),
                new DateOnly(year: 2025, month: 3, day: 26),
                new DateOnly(year: 2025, month: 3, day: 27),
                new DateOnly(year: 2025, month: 3, day: 28),
                new DateOnly(year: 2025, month: 3, day: 29),
                new DateOnly(year: 2025, month: 3, day: 30),
            }
        );
    }

    [Fact]
    public void WeekNumberToDates_returns_expected_result_for_first_week_of_year()
    {
        var dates = WeekNumberHelper.WeekNumberToDates(year: 2025, number: 1);
        dates.ShouldBeEquivalentTo(
            new[]
            {
                new DateOnly(year: 2024, month: 12, day: 30),
                new DateOnly(year: 2024, month: 12, day: 31),
                new DateOnly(year: 2025, month: 1, day: 1),
                new DateOnly(year: 2025, month: 1, day: 2),
                new DateOnly(year: 2025, month: 1, day: 3),
                new DateOnly(year: 2025, month: 1, day: 4),
                new DateOnly(year: 2025, month: 1, day: 5),
            }
        );
    }

    [Fact]
    public void WeekNumberToDates_returns_expected_result_for_last_week_of_year()
    {
        var dates = WeekNumberHelper.WeekNumberToDates(year: 2025, number: 52);
        dates.ShouldBeEquivalentTo(
            new[]
            {
                new DateOnly(year: 2025, month: 12, day: 22),
                new DateOnly(year: 2025, month: 12, day: 23),
                new DateOnly(year: 2025, month: 12, day: 24),
                new DateOnly(year: 2025, month: 12, day: 25),
                new DateOnly(year: 2025, month: 12, day: 26),
                new DateOnly(year: 2025, month: 12, day: 27),
                new DateOnly(year: 2025, month: 12, day: 28),
            }
        );
    }

    [Fact]
    public void WeekNumberToDates_returns_expected_result_for_week_of_leap_year()
    {
        var dates = WeekNumberHelper.WeekNumberToDates(year: 2024, number: 9);
        dates.ShouldBeEquivalentTo(
            new[]
            {
                new DateOnly(year: 2024, month: 2, day: 26),
                new DateOnly(year: 2024, month: 2, day: 27),
                new DateOnly(year: 2024, month: 2, day: 28),
                new DateOnly(year: 2024, month: 2, day: 29),
                new DateOnly(year: 2024, month: 3, day: 1),
                new DateOnly(year: 2024, month: 3, day: 2),
                new DateOnly(year: 2024, month: 3, day: 3),
            }
        );
    }
}
