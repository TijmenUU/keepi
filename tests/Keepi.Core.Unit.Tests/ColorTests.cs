namespace Keepi.Core.Unit.Tests;

public class ColorTests
{
    [Theory]
    [InlineData(0x020304u, 2, 3, 4)]
    [InlineData(0x010203u, 1, 2, 3)]
    [InlineData(0xFF0000u, 255, 0, 0)]
    [InlineData(0x00FF00u, 0, 255, 0)]
    [InlineData(0x0000FFu, 0, 0, 255)]
    [InlineData(0xFFFFFFu, 255, 255, 255)]
    [InlineData(0x000000u, 0, 0, 0)]
    public void FromUint32_yields_expected_result(
        uint input,
        byte expectedRed,
        byte expectedGreen,
        byte expectedBlue
    )
    {
        var color = Color.FromUint32(input);
        color.Red.ShouldBe(expectedRed);
        color.Green.ShouldBe(expectedGreen);
        color.Blue.ShouldBe(expectedBlue);
    }

    [Theory]
    [InlineData(0x1FFFFFFu)]
    [InlineData(0xFFFFFFFFu)]
    public void FromUint32_throws_exception_for_too_large_values(uint input)
    {
        Should.Throw<Exception>(() => Color.FromUint32(input));
    }

    [Theory]
    [InlineData(2, 3, 4, 0x020304u)]
    [InlineData(1, 2, 3, 0x010203u)]
    [InlineData(255, 0, 0, 0xFF0000u)]
    [InlineData(0, 255, 0, 0x00FF00u)]
    [InlineData(0, 0, 255, 0x0000FFu)]
    [InlineData(255, 255, 255, 0xFFFFFFu)]
    [InlineData(0, 0, 0, 0x000000u)]
    public void ToUint32_yields_expected_result(
        byte red,
        byte green,
        byte blue,
        uint expectedOutput
    )
    {
        var color = new Color(Red: red, Blue: blue, Green: green);

        Color.ToUint32(color).ShouldBe(expectedOutput);
    }

    [Theory]
    [InlineData(0x020304u)]
    [InlineData(0x010203u)]
    [InlineData(0xFF0000u)]
    [InlineData(0x00FF00u)]
    [InlineData(0x0000FFu)]
    [InlineData(0xFFFFFFu)]
    [InlineData(0x000000u)]
    public void FromUint32_followed_by_ToUint32_should_yield_input(uint input)
    {
        var color = Color.FromUint32(input);
        Color.ToUint32(color).ShouldBe(input);
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
        new Color(Red: red, Blue: blue, Green: green).ToHexColorString().ShouldBe(expectedOutput);
    }

    [Theory]
    [InlineData("#020304", 2, 3, 4)]
    [InlineData("#ff0000", 255, 0, 0)]
    [InlineData("#00ff00", 0, 255, 0)]
    [InlineData("#0000ff", 0, 0, 255)]
    [InlineData("#ffffff", 255, 255, 255)]
    [InlineData("#000000", 0, 0, 0)]
    public void TryParseHexString_returns_expected_color(
        string input,
        byte expectedRed,
        byte expectedGreen,
        byte expectedBlue
    )
    {
        Color.TryParseHexString(input, out var color).ShouldBeTrue();
        color.Red.ShouldBe(expectedRed);
        color.Green.ShouldBe(expectedGreen);
        color.Blue.ShouldBe(expectedBlue);
    }

    [Fact]
    public void TryParseHexString_does_not_care_about_casing()
    {
        Color.TryParseHexString("#ffaabb", out var color).ShouldBeTrue();
        Color.ToUint32(color).ShouldBe(0xFFAABBu);
    }

    [Theory]
    [InlineData("0xFFFFFF")]
    [InlineData("FFFFFF")]
    [InlineData("#FFFFF")]
    [InlineData("#FFFF")]
    [InlineData("")]
    public void TryParseHexString_returns_false_for_invalid_input(string input)
    {
        Color.TryParseHexString(input, out var color).ShouldBeFalse();
    }

    [Theory]
    [InlineData("#020304")]
    [InlineData("#ff0000")]
    [InlineData("#00ff00")]
    [InlineData("#0000ff")]
    [InlineData("#ffffff")]
    [InlineData("#000000")]
    public void TryParseHexString_followed_by_ToHexColorString_yield_input(string input)
    {
        Color.TryParseHexString(input, out var color).ShouldBeTrue();
        color.ToHexColorString().ShouldBe(input);
    }
}
