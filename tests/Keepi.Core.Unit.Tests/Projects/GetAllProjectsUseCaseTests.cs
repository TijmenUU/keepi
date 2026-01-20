using Keepi.Core.Projects;
using Keepi.Core.Users;

namespace Keepi.Core.Unit.Tests.Projects;

public class GetAllProjectsUseCaseTests
{
    [Fact]
    public async Task Execute_returns_projects()
    {
        var context = new TestContext()
            .WithResolvedUser(user: ResolvedUserBuilder.CreateAdministratorBob())
            .WithProjectsResult(
                new GetProjectsResultProject(
                    Id: 1,
                    Name: "Project 1",
                    Enabled: true,
                    Users: [new(Id: 10, Name: "Henk"), new(Id: 11, Name: "Piet")],
                    InvoiceItems: [new(Id: 20, Name: "Post 1"), new(Id: 21, Name: "Post 2")]
                ),
                new GetProjectsResultProject(
                    Id: 2,
                    Name: "Project 2",
                    Enabled: false,
                    Users: [],
                    InvoiceItems: []
                )
            );

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(cancellationToken: CancellationToken.None);
        result.TrySuccess(out var successResult, out _).ShouldBeTrue();

        successResult.ShouldBeEquivalentTo(
            new GetProjectsResult(
                Projects:
                [
                    new(
                        Id: 1,
                        Name: "Project 1",
                        Enabled: true,
                        Users: [new(Id: 10, Name: "Henk"), new(Id: 11, Name: "Piet")],
                        InvoiceItems: [new(Id: 20, Name: "Post 1"), new(Id: 21, Name: "Post 2")]
                    ),
                    new(Id: 2, Name: "Project 2", Enabled: false, Users: [], InvoiceItems: []),
                ]
            )
        );

        context.ResolveUserMock.Verify(x => x.Execute(It.IsAny<CancellationToken>()));
        context.GetProjectsMock.Verify(x => x.Execute(It.IsAny<CancellationToken>()));
        context.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(GetProjectsError.Unknown, GetAllProjectsUseCaseError.Unknown)]
    public async Task Execute_returns_errors_raised_by_repository_call(
        GetProjectsError repositoryError,
        GetAllProjectsUseCaseError expectedError
    )
    {
        var context = new TestContext()
            .WithResolvedUser(user: ResolvedUserBuilder.CreateAdministratorBob())
            .WithProjectsResult(repositoryError);

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(cancellationToken: CancellationToken.None);
        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();

        errorResult.ShouldBe(expectedError);

        context.ResolveUserMock.Verify(x => x.Execute(It.IsAny<CancellationToken>()));
        context.GetProjectsMock.Verify(x => x.Execute(It.IsAny<CancellationToken>()));
        context.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(
        ResolveUserError.UserNotAuthenticated,
        GetAllProjectsUseCaseError.UnauthenticatedUser
    )]
    [InlineData(ResolveUserError.MalformedUserClaims, GetAllProjectsUseCaseError.Unknown)]
    [InlineData(ResolveUserError.UnsupportedIdentityProvider, GetAllProjectsUseCaseError.Unknown)]
    [InlineData(ResolveUserError.UserNotFound, GetAllProjectsUseCaseError.Unknown)]
    [InlineData(ResolveUserError.UserRegistrationFailed, GetAllProjectsUseCaseError.Unknown)]
    public async Task Execute_returns_error_for_user_resolve_error(
        ResolveUserError resolveUserError,
        GetAllProjectsUseCaseError expectedError
    )
    {
        var context = new TestContext().WithResolveUserError(resolveUserError);

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(cancellationToken: CancellationToken.None);

        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(expectedError);
    }

    private class TestContext
    {
        public Mock<IResolveUser> ResolveUserMock { get; } = new(MockBehavior.Strict);
        public Mock<IGetProjects> GetProjectsMock { get; } = new(MockBehavior.Strict);

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

        public TestContext WithProjectsResult(params GetProjectsResultProject[] projects)
        {
            GetProjectsMock
                .Setup(x => x.Execute(It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    Result.Success<GetProjectsResult, GetProjectsError>(
                        new GetProjectsResult(Projects: projects)
                    )
                );

            return this;
        }

        public TestContext WithProjectsResult(GetProjectsError error)
        {
            GetProjectsMock
                .Setup(x => x.Execute(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure<GetProjectsResult, GetProjectsError>(error));

            return this;
        }

        public GetAllProjectsUseCase BuildUseCase() =>
            new(resolveUser: ResolveUserMock.Object, getProjects: GetProjectsMock.Object);

        public void VerifyNoOtherCalls()
        {
            ResolveUserMock.VerifyNoOtherCalls();
            GetProjectsMock.VerifyNoOtherCalls();
        }
    }
}
