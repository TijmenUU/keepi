using Keepi.Core.Users;

namespace Keepi.Core.Unit.Tests.Users;

public class GetAllUsersUseCaseTests
{
    [Fact]
    public async Task Execute_returns_projects()
    {
        var context = new TestContext().WithUsersResult(
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
        var context = new TestContext().WithUsersResult(repositoryError);

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(cancellationToken: CancellationToken.None);
        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();

        errorResult.ShouldBe(expectedError);

        context.GetUsersMock.Verify(x => x.Execute(It.IsAny<CancellationToken>()));
        context.VerifyNoOtherCalls();
    }

    private class TestContext
    {
        public Mock<IGetUsers> GetUsersMock { get; } = new(MockBehavior.Strict);

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

        public GetAllUsersUseCase BuildUseCase() => new(getUsers: GetUsersMock.Object);

        public void VerifyNoOtherCalls()
        {
            GetUsersMock.VerifyNoOtherCalls();
        }
    }
}
