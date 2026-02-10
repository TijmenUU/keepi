using Keepi.App.Authorization;

namespace Keepi.App.Unit.Tests.Authorization;

public class GetFirstAdminUserEmailAddressTests
{
    [Fact]
    public void Execute_returns_hardcoded_value()
    {
        var helper = new GetFirstAdminUserEmailAddress();
        helper.Execute().TrySuccess(out var successResult, out _).ShouldBeTrue();
        successResult.ShouldBe("user@localhost");
    }
}
