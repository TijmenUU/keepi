using System.Text;
using Keepi.Core.Entries;
using Vogen;

namespace Keepi.Core.Unit.Tests.Entries;

public class UserEntryRemarkTests
{
    [Theory]
    [InlineData("a")]
    [InlineData("This is a remark")]
    public void Validate_returns_ok_for_valid_remark(string remark)
    {
        UserEntryRemark.Validate(remark).ShouldBe(Validation.Ok);
    }

    [Fact]
    public void Validate_returns_ok_for_maximum_length_remark()
    {
        var remark = new StringBuilder().Append(value: 'a', repeatCount: 256).ToString();
        UserEntryRemark.Validate(remark).ShouldBe(Validation.Ok);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_returns_invalid_for_empty_remark(string remark)
    {
        UserEntryRemark.Validate(remark).ErrorMessage.ShouldNotBeEmpty();
    }

    [Fact]
    public void Validate_returns_invalid_for_too_long_remark()
    {
        var remark = new StringBuilder().Append(value: 'a', repeatCount: 257).ToString();
        UserEntryRemark.Validate(remark).ErrorMessage.ShouldNotBeEmpty();
    }
}
