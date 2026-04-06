namespace Keepi.Core.Unit.Tests;

public class WeekTests
{
    [Fact]
    public void Today_should_return_value()
    {
        var today = Week.Today;
        today.Number.Value.ShouldBeInRange(from: 1, to: 53);
        today.Year.Value.ShouldBe(DateTime.Today.Year);
    }

    [Fact]
    public void ToDates_returns_expected_result_for_normal_week()
    {
        var dates = new Week(Year: Year.From(2025), Number: WeekNumber.From(13)).ToDates();
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
    public void ToDates_returns_expected_result_for_first_week_of_year()
    {
        var dates = new Week(Year: Year.From(2025), Number: WeekNumber.From(1)).ToDates();
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
    public void ToDates_returns_expected_result_for_last_week_of_52_week_long_year()
    {
        var dates = new Week(Year: Year.From(2025), Number: WeekNumber.From(52)).ToDates();
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
    public void ToDates_returns_expected_result_for_last_week_of_53_week_long_year()
    {
        var dates = new Week(Year: Year.From(2009), Number: WeekNumber.From(53)).ToDates();
        dates.ShouldBeEquivalentTo(
            new[]
            {
                new DateOnly(year: 2009, month: 12, day: 28),
                new DateOnly(year: 2009, month: 12, day: 29),
                new DateOnly(year: 2009, month: 12, day: 30),
                new DateOnly(year: 2009, month: 12, day: 31),
                new DateOnly(year: 2010, month: 1, day: 1),
                new DateOnly(year: 2010, month: 1, day: 2),
                new DateOnly(year: 2010, month: 1, day: 3),
            }
        );
    }

    [Fact]
    public void ToDates_returns_expected_result_for_week_of_leap_year()
    {
        var dates = new Week(Year: Year.From(2024), Number: WeekNumber.From(9)).ToDates();
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
