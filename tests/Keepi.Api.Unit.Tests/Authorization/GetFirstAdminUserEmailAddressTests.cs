using Keepi.Api.Authorization;
using Keepi.Core;
using Keepi.Generators;

namespace Keepi.Api.Unit.Tests.Authorization;

public class GetFirstAdminUserEmailAddressTests
{
    [Fact]
    public void Execute_returns_configured_email_address()
    {
        var context = new GetFirstAdminUserEmailAddressTestContext().WithConfiguredEmailAddress(
            "reinhard@example.com"
        );

        var result = context.BuildTarget().Execute();
        result.TrySuccess(out var successResult, out _).ShouldBeTrue();

        successResult.ShouldBe(EmailAddress.From("reinhard@example.com"));

        context.ConfigurationMock.Verify(x =>
            x[GetFirstAdminUserEmailAddressTestContext.ConfigurationKey]
        );
        context.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Execute_returns_not_configured_error_for_missing_email_address(string? emailAddress)
    {
        var context = new GetFirstAdminUserEmailAddressTestContext().WithConfiguredEmailAddress(
            emailAddress
        );

        var result = context.BuildTarget().Execute();
        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();

        errorResult.ShouldBe(Core.Users.GetFirstAdminUserEmailAddressError.NotConfigured);

        context.ConfigurationMock.Verify(x =>
            x[GetFirstAdminUserEmailAddressTestContext.ConfigurationKey]
        );
        context.VerifyNoOtherCalls();
    }
}

[GenerateTestContext(target: typeof(GetFirstAdminUserEmailAddress), GenerateWithMethods = false)]
internal partial class GetFirstAdminUserEmailAddressTestContext
{
    public const string ConfigurationKey = "Authentication:FirstAdminUserEmailAddress";

    public GetFirstAdminUserEmailAddressTestContext WithConfiguredEmailAddress(string? value)
    {
        ConfigurationMock.Setup(x => x[ConfigurationKey]).Returns(value);

        return this;
    }
}
