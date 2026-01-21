using Keepi.Core.Unit.Tests.Builders;
using Keepi.Core.Users;

namespace Keepi.Core.Unit.Tests.Users;

public class GetUserUseCaseTests
{
    [Fact]
    public async Task Execute_returns_expected_user()
    {
        var context = new TestContext().WithResolvedUser(
            ResolvedUserBuilder.CreateAdministratorBob()
        );

        var result = await context
            .BuildUseCase()
            .Execute(cancellationToken: CancellationToken.None);

        result.TrySuccess(out var successResult, out _).ShouldBeTrue();
        successResult.ShouldBeEquivalentTo(
            new GetUserUseCaseOutput(Id: 42, Name: "Bob", EmailAddress: "bob@example.com")
        );
    }

    [Theory]
    [InlineData(ResolveUserError.UserNotAuthenticated, GetUserUseCaseError.UnauthenticatedUser)]
    [InlineData(ResolveUserError.MalformedUserClaims, GetUserUseCaseError.Unknown)]
    [InlineData(ResolveUserError.UnsupportedIdentityProvider, GetUserUseCaseError.Unknown)]
    [InlineData(ResolveUserError.UserNotFound, GetUserUseCaseError.Unknown)]
    [InlineData(ResolveUserError.UserRegistrationFailed, GetUserUseCaseError.Unknown)]
    public async Task Execute_returns_error_for_user_resolve_error(
        ResolveUserError resolveUserError,
        GetUserUseCaseError expectedError
    )
    {
        var context = new TestContext().WithResolveUserError(resolveUserError);

        var result = await context
            .BuildUseCase()
            .Execute(cancellationToken: CancellationToken.None);

        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(expectedError);
    }

    private class TestContext
    {
        public Mock<IResolveUser> ResolveUserMock { get; } = new(MockBehavior.Strict);

        public TestContext WithResolvedUser(ResolvedUser user)
        {
            ResolveUserMock
                .Setup(x => x.Execute(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success<ResolvedUser, ResolveUserError>(user));

            return this;
        }

        public TestContext WithResolveUserError(ResolveUserError error)
        {
            ResolveUserMock
                .Setup(x => x.Execute(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure<ResolvedUser, ResolveUserError>(error));

            return this;
        }

        public GetUserUseCase BuildUseCase() => new(resolveUser: ResolveUserMock.Object);

        public void VerifyNoOtherCalls()
        {
            ResolveUserMock.VerifyNoOtherCalls();
        }
    }
}
