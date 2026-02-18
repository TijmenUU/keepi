namespace Keepi.App.Unit.Tests;

public class HoursMinuteNotationTests
{
    [Theory]
    [InlineData("", 0)]
    [InlineData("1m", 1)]
    [InlineData("59m", 59)]
    [InlineData("1u", 60)]
    [InlineData("1u1m", 61)]
    [InlineData("1u15m", 75)]
    [InlineData("2u", 120)]
    [InlineData("-59m", -59)]
    [InlineData("-1u", -60)]
    [InlineData("-1u1m", -61)]
    [InlineData("-1u15m", -75)]
    [InlineData("abc", 42)]
    public void ParseOrFallback_returns_expected_result(string input, int expectedOutput)
    {
        HoursMinuteNotation.ParseOrFallback(input: input, fallback: 42).ShouldBe(expectedOutput);
    }

    [Theory]
    [InlineData("", true, 0)]
    [InlineData("1m", true, 1)]
    [InlineData("59m", true, 59)]
    [InlineData("1u", true, 60)]
    [InlineData("1u1m", true, 61)]
    [InlineData("1u15m", true, 75)]
    [InlineData("2u", true, 120)]
    [InlineData("-59m", true, -59)]
    [InlineData("-1u", true, -60)]
    [InlineData("-1u1m", true, -61)]
    [InlineData("-1u15m", true, -75)]
    [InlineData("abc", false, 0)]
    public void TryParse_returns_expected_result(
        string input,
        bool expectedOutput,
        int expectedValue
    )
    {
        HoursMinuteNotation.TryParse(input: input, out var value).ShouldBe(expectedOutput);
        value.ShouldBe(expectedValue);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1u")]
    [InlineData("15m")]
    [InlineData("1u9m")]
    [InlineData("1u59m")]
    [InlineData("123u45m")]
    public void IsValid_returns_true_for_valid_notations(string notation)
    {
        HoursMinuteNotation.IsValid(notation: notation).ShouldBeTrue();
    }

    [Theory]
    [InlineData("1")]
    [InlineData("1u32")]
    [InlineData("1h30m")]
    [InlineData("61m")]
    public void IsValid_returns_false_for_invalid_notations(string notation)
    {
        HoursMinuteNotation.IsValid(notation: notation).ShouldBeFalse();
    }

    [Theory]
    [InlineData(0, "")]
    [InlineData(1, "1m")]
    [InlineData(59, "59m")]
    [InlineData(60, "1u")]
    [InlineData(61, "1u1m")]
    [InlineData(75, "1u15m")]
    [InlineData(120, "2u")]
    [InlineData(-59, "-59m")]
    [InlineData(-60, "-1u")]
    [InlineData(-61, "-1u1m")]
    [InlineData(-75, "-1u15m")]
    public void Format_returns_expected_result(int minutes, string expectedOutput)
    {
        HoursMinuteNotation.Format(minutes: minutes).ShouldBe(expectedOutput);
    }
}
