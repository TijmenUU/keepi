using System.Text;
using Keepi.Core.UserEntryCategories;

namespace Keepi.Core.Unit.Tests.UserEntryCategories;

public class UserEntryCategoryEntityTests
{
    [Theory]
    [InlineData("a")]
    [InlineData("1")]
    [InlineData("_#@!^")]
    [InlineData("12345678901234567890123456789012")]
    public void IsValidName_returns_true_for_valid_names(string name)
    {
        UserEntryCategoryEntity.IsValidName(name).ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("\t")]
    public void IsValidName_returns_false_for_invalid_names(string? name)
    {
        UserEntryCategoryEntity.IsValidName(name).ShouldBeFalse();
    }

    [Fact]
    public void IsValidName_returns_false_for_too_long_name()
    {
        var name = new StringBuilder().Append('a', repeatCount: 65).ToString();
        UserEntryCategoryEntity.IsValidName(name).ShouldBeFalse();
    }

    [Fact]
    public void IsValidActiveDateRange_returns_true_for_null_to_date()
    {
        UserEntryCategoryEntity
            .IsValidActiveDateRange(from: new DateOnly(2025, 6, 15), to: null)
            .ShouldBeTrue();
    }

    [Fact]
    public void IsValidActiveDateRange_returns_true_for_null_from_date()
    {
        UserEntryCategoryEntity
            .IsValidActiveDateRange(from: null, to: new DateOnly(2025, 6, 10))
            .ShouldBeTrue();
    }

    [Fact]
    public void IsValidActiveDateRange_returns_true_for_null_dates()
    {
        UserEntryCategoryEntity.IsValidActiveDateRange(from: null, to: null).ShouldBeTrue();
    }

    [Fact]
    public void IsValidActiveDateRange_returns_false_for_from_smaller_than_to()
    {
        UserEntryCategoryEntity
            .IsValidActiveDateRange(from: new DateOnly(2025, 6, 15), to: new DateOnly(2025, 6, 10))
            .ShouldBeFalse();
    }
}
