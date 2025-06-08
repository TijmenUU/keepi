using System.Text;
using Keepi.Core.Entries;

namespace Keepi.Core.Unit.Tests.Entries;

public class UserEntryEntityTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(60)]
    [InlineData(int.MaxValue)]
    public void IsValidMinutes_returns_true_for_positive_numbers(int minutes)
    {
        UserEntryEntity.IsValidMinutes(minutes).ShouldBeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-60)]
    [InlineData(int.MinValue)]
    public void IsValidMinutes_returns_false_for_zero_and_negative_numbers(int minutes)
    {
        UserEntryEntity.IsValidMinutes(minutes).ShouldBeFalse();
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("This is a remark")]
    public void IsValidRemark_returns_true_for_valid_remark(string? remark)
    {
        UserEntryEntity.IsValidRemark(remark).ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsValidRemark_returns_true_for_empty_remark(string? remark)
    {
        UserEntryEntity.IsValidRemark(remark).ShouldBeTrue();
    }

    [Fact]
    public void IsValidRemark_returns_true_for_maximum_length_remark()
    {
        var remark = new StringBuilder().Append(value: 'a', repeatCount: 256).ToString();
        UserEntryEntity.IsValidRemark(remark).ShouldBeTrue();
    }

    [Fact]
    public void IsValidRemark_returns_false_for_too_long_remark()
    {
        var remark = new StringBuilder().Append(value: 'a', repeatCount: 257).ToString();
        UserEntryEntity.IsValidRemark(remark).ShouldBeFalse();
    }
}
