using Keepi.Core.Entries;
using Vogen;

namespace Keepi.Core.Unit.Tests.Entries;

public class UserEntryMinutesTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(60)]
    [InlineData(int.MaxValue)]
    public void Validate_returns_ok_for_positive_numbers(int minutes)
    {
        UserEntryMinutes.Validate(minutes).ShouldBe(Validation.Ok);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-60)]
    [InlineData(int.MinValue)]
    public void Validate_returns_invalid_for_zero_and_negative_numbers(int minutes)
    {
        UserEntryMinutes.Validate(minutes).ErrorMessage.ShouldNotBeEmpty();
    }
}
