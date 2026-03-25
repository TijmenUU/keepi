namespace Keepi.Core.Unit.Tests;

public class ColorTests
{
    [Theory]
    [InlineData(0x020304u)]
    [InlineData(0x010203u)]
    [InlineData(0xFF0000u)]
    [InlineData(0x00FF00u)]
    [InlineData(0x0000FFu)]
    [InlineData(0xFFFFFFu)]
    [InlineData(0x000000u)]
    public void From_yields_expected_result(uint input)
    {
        var color = Color.From(input);
        color.Value.ShouldBe(input);
    }

    [Theory]
    [InlineData(0x1FFFFFFu)]
    [InlineData(0xFFFFFFFFu)]
    public void From_throws_exception_for_too_large_values(uint input)
    {
        Should.Throw<Vogen.ValueObjectValidationException>(() => Color.From(input));
    }

    [Theory]
    [InlineData(2, 3, 4, 0x020304u)]
    [InlineData(1, 2, 3, 0x010203u)]
    [InlineData(255, 0, 0, 0xFF0000u)]
    [InlineData(0, 255, 0, 0x00FF00u)]
    [InlineData(0, 0, 255, 0x0000FFu)]
    [InlineData(255, 255, 255, 0xFFFFFFu)]
    [InlineData(0, 0, 0, 0x000000u)]
    public void FromBytes_yields_expected_result(
        byte red,
        byte green,
        byte blue,
        uint expectedOutput
    )
    {
        var color = Color.FromBytes(red: red, blue: blue, green: green);
        color.Value.ShouldBe(expectedOutput);
    }

    [Theory]
    [InlineData(2, 3, 4, "#020304")]
    [InlineData(255, 0, 0, "#ff0000")]
    [InlineData(0, 255, 0, "#00ff00")]
    [InlineData(0, 0, 255, "#0000ff")]
    [InlineData(255, 255, 255, "#ffffff")]
    [InlineData(0, 0, 0, "#000000")]
    public void ToHexColorString_returns_expected_string(
        byte red,
        byte green,
        byte blue,
        string expectedOutput
    )
    {
        Color
            .FromBytes(red: red, blue: blue, green: green)
            .ToHexColorString()
            .ShouldBe(expectedOutput);
    }

    [Theory]
    [InlineData("#020304", 0x020304)]
    [InlineData("#ff0000", 0xff0000)]
    [InlineData("#00ff00", 0x00ff00)]
    [InlineData("#0000ff", 0x0000ff)]
    [InlineData("#ffffff", 0xffffff)]
    [InlineData("#000000", 0x000000)]
    public void TryParseHexColorString_returns_expected_color(string input, uint expectedResult)
    {
        Color.TryParseHexColorString(input, out var color).ShouldBeTrue();
        color.Value.ShouldBe(expectedResult);
    }

    [Fact]
    public void TryParseHexColorString_does_not_care_about_casing()
    {
        Color.TryParseHexColorString("#ffaabb", out var color).ShouldBeTrue();
        color.Value.ShouldBe(0xFFAABBu);
    }

    [Theory]
    [InlineData("0xFFFFFF")]
    [InlineData("FFFFFF")]
    [InlineData("#FFFFF")]
    [InlineData("#FFFF")]
    [InlineData("")]
    public void TryParseHexColorString_returns_false_for_invalid_input(string input)
    {
        Color.TryParseHexColorString(input, out var color).ShouldBeFalse();
    }

    [Theory]
    [InlineData("#020304")]
    [InlineData("#ff0000")]
    [InlineData("#00ff00")]
    [InlineData("#0000ff")]
    [InlineData("#ffffff")]
    [InlineData("#000000")]
    public void TryParseHexColorString_followed_by_ToHexColorString_yield_input(string input)
    {
        Color.TryParseHexColorString(input, out var color).ShouldBeTrue();
        color.ToHexColorString().ShouldBe(input);
    }
}
