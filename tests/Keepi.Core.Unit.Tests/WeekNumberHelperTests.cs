namespace Keepi.Core.Unit.Tests;

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

    [Fact]
    public void GetCurrentWeek_returns_current_week()
    {
        var result = WeekNumberHelper.GetCurrentWeek();
        result.Year.ShouldBe(DateTime.Today.Year);
        result.Number.ShouldBeGreaterThan(0);
        result.Number.ShouldBeLessThan(54);
    }

    [Theory]
    [InlineData(2024, 1, 2024, 2)]
    [InlineData(2024, 50, 2024, 51)]
    [InlineData(2024, 52, 2025, 1)]
    [InlineData(2026, 53, 2027, 1)]
    public void GetNextWeek_returns_expected_week(
        int inputYear,
        int inputWeekNumber,
        int expectedYear,
        int expectedWeekNumber
    )
    {
        var result = WeekNumberHelper.GetNextWeek(new(Year: inputYear, Number: inputWeekNumber));
        result.ShouldBeEquivalentTo(new Week(Year: expectedYear, Number: expectedWeekNumber));
    }

    [Theory]
    [InlineData(2024, 2, 2024, 1)]
    [InlineData(2024, 51, 2024, 50)]
    [InlineData(2025, 1, 2024, 52)]
    [InlineData(2027, 1, 2026, 53)]
    public void GetPreviousWeek_returns_expected_week(
        int inputYear,
        int inputWeekNumber,
        int expectedYear,
        int expectedWeekNumber
    )
    {
        var result = WeekNumberHelper.GetPreviousWeek(
            new(Year: inputYear, Number: inputWeekNumber)
        );
        result.ShouldBeEquivalentTo(new Week(Year: expectedYear, Number: expectedWeekNumber));
    }

    [Theory]
    [InlineData("2024-02-29", 2024, 9)] // Leap year day
    [InlineData("2024-12-30", 2025, 1)] // First week starts in previous year
    [InlineData("2026-02-13", 2026, 7)] // Normal week
    [InlineData("2027-01-01", 2026, 53)] // Last week overlaps with next year
    public void GetWeekForDate_returns_expected_week(
        string inputDate,
        int expectedYear,
        int expectedWeekNumber
    )
    {
        var result = WeekNumberHelper.GetWeekForDate(DateOnly.Parse(inputDate));
        result.ShouldBeEquivalentTo(new Week(Year: expectedYear, Number: expectedWeekNumber));
    }
}
