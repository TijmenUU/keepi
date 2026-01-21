using Keepi.Core.Unit.Tests.Builders;
using Keepi.Core.Users;

namespace Keepi.Core.Unit.Tests.Users;

public class GetAllUsersUseCaseTests
{
    [Fact]
    public async Task Execute_returns_projects()
    {
        var context = new TestContext()
            .WithResolvedUser(user: ResolvedUserBuilder.CreateAdministratorBob())
            .WithUsersResult(
                new GetUsersResultUser(
                    Id: 1,
                    Name: "Bob",
                    EmailAddress: "bob@example.com",
                    IdentityOrigin: UserIdentityProvider.GitHub
                ),
                new GetUsersResultUser(
                    Id: 2,
                    Name: "Miro",
                    EmailAddress: "miro@example.com",
                    IdentityOrigin: UserIdentityProvider.GitHub
                )
            );

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(cancellationToken: CancellationToken.None);
        result.TrySuccess(out var successResult, out _).ShouldBeTrue();

        successResult.ShouldBeEquivalentTo(
            new GetUsersResult(
                Users:
                [
                    new(
                        Id: 1,
                        Name: "Bob",
                        EmailAddress: "bob@example.com",
                        IdentityOrigin: UserIdentityProvider.GitHub
                    ),
                    new(
                        Id: 2,
                        Name: "Miro",
                        EmailAddress: "miro@example.com",
                        IdentityOrigin: UserIdentityProvider.GitHub
                    ),
                ]
            )
        );

        context.ResolveUserMock.Verify(x => x.Execute(It.IsAny<CancellationToken>()));
        context.GetUsersMock.Verify(x => x.Execute(It.IsAny<CancellationToken>()));
        context.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(GetUsersError.Unknown, GetAllUsersUseCaseError.Unknown)]
    public async Task Execute_returns_errors_raised_by_repository_call(
        GetUsersError repositoryError,
        GetAllUsersUseCaseError expectedError
    )
    {
        var context = new TestContext()
            .WithResolvedUser(user: ResolvedUserBuilder.CreateAdministratorBob())
            .WithUsersResult(repositoryError);

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(cancellationToken: CancellationToken.None);
        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();

        errorResult.ShouldBe(expectedError);

        context.ResolveUserMock.Verify(x => x.Execute(It.IsAny<CancellationToken>()));
        context.GetUsersMock.Verify(x => x.Execute(It.IsAny<CancellationToken>()));
        context.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(ResolveUserError.UserNotAuthenticated, GetAllUsersUseCaseError.UnauthenticatedUser)]
    [InlineData(ResolveUserError.MalformedUserClaims, GetAllUsersUseCaseError.Unknown)]
    [InlineData(ResolveUserError.UnsupportedIdentityProvider, GetAllUsersUseCaseError.Unknown)]
    [InlineData(ResolveUserError.UserNotFound, GetAllUsersUseCaseError.Unknown)]
    [InlineData(ResolveUserError.UserRegistrationFailed, GetAllUsersUseCaseError.Unknown)]
    public async Task Execute_returns_error_for_user_resolve_error(
        ResolveUserError resolveUserError,
        GetAllUsersUseCaseError expectedError
    )
    {
        var context = new TestContext().WithResolveUserError(resolveUserError);

        var result = await context
            .BuildUseCase()
            .Execute(cancellationToken: CancellationToken.None);

        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(expectedError);
    }

    [Fact]
    public async Task Execute_returns_error_for_unauthorized_user()
    {
        var context = new TestContext().WithResolvedUser(
            user: ResolvedUserBuilder
                .AsAdministratorBob()
                .WithUsersPermission(UserPermission.None)
                .Build()
        );

        var result = await context
            .BuildUseCase()
            .Execute(cancellationToken: CancellationToken.None);
        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(GetAllUsersUseCaseError.UnauthorizedUser);
    }

    private class TestContext
    {
        public Mock<IResolveUser> ResolveUserMock { get; } = new(MockBehavior.Strict);
        public Mock<IGetUsers> GetUsersMock { get; } = new(MockBehavior.Strict);

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

        public TestContext WithUsersResult(params GetUsersResultUser[] users)
        {
            GetUsersMock
                .Setup(x => x.Execute(It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    Result.Success<GetUsersResult, GetUsersError>(new GetUsersResult(Users: users))
                );

            return this;
        }

        public TestContext WithUsersResult(GetUsersError error)
        {
            GetUsersMock
                .Setup(x => x.Execute(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure<GetUsersResult, GetUsersError>(error));

            return this;
        }

        public GetAllUsersUseCase BuildUseCase() =>
            new(resolveUser: ResolveUserMock.Object, getUsers: GetUsersMock.Object);

        public void VerifyNoOtherCalls()
        {
            ResolveUserMock.VerifyNoOtherCalls();
            GetUsersMock.VerifyNoOtherCalls();
        }
    }
}
