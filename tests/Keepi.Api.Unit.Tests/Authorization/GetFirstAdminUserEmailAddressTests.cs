using Keepi.Api.Authorization;
using Microsoft.Extensions.Configuration;

namespace Keepi.Api.Unit.Tests.Authorization;

public class GetFirstAdminUserEmailAddressTests
{
    [Fact]
    public void Execute_returns_configured_email_address()
    {
        var context = new TestContext().WithConfiguredEmailAddress("reinhard@example.com");

        var result = context.BuildHelper().Execute();
        result.TrySuccess(out var successResult, out _).ShouldBeTrue();

        successResult.ShouldBe("reinhard@example.com");

        context.ConfigurationMock.Verify(x => x[TestContext.ConfigurationKey]);
        context.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Execute_returns_not_configured_error_for_missing_email_address(string? emailAddress)
    {
        var context = new TestContext().WithConfiguredEmailAddress(emailAddress);

        var result = context.BuildHelper().Execute();
        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();

        errorResult.ShouldBe(Core.Users.GetFirstAdminUserEmailAddressError.NotConfigured);

        context.ConfigurationMock.Verify(x => x[TestContext.ConfigurationKey]);
        context.VerifyNoOtherCalls();
    }

    private class TestContext
    {
        public const string ConfigurationKey = "Authentication:FirstAdminUserEmailAddress";
        public Mock<IConfiguration> ConfigurationMock { get; } = new(MockBehavior.Strict);

        public TestContext WithConfiguredEmailAddress(string? value)
        {
            ConfigurationMock.Setup(x => x[ConfigurationKey]).Returns(value);

            return this;
        }

        public GetFirstAdminUserEmailAddress BuildHelper() =>
            new(configuration: ConfigurationMock.Object);

        public void VerifyNoOtherCalls()
        {
            ConfigurationMock.VerifyNoOtherCalls();
        }
    }
}
