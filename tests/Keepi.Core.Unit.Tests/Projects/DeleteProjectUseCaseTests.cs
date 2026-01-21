using Keepi.Core.Projects;
using Keepi.Core.Unit.Tests.Builders;
using Keepi.Core.Users;

namespace Keepi.Core.Unit.Tests.Projects;

public class DeleteProjectUseCaseTests
{
    [Fact]
    public async Task Execute_returns_success()
    {
        var context = new TestContext()
            .WithResolvedUser(user: ResolvedUserBuilder.CreateAdministratorBob())
            .WithSuccessfulDeleteProject();

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            projectId: 50,
            cancellationToken: CancellationToken.None
        );
        result.TrySuccess(out _).ShouldBeTrue();

        context.ResolveUserMock.Verify(x => x.Execute(It.IsAny<CancellationToken>()));
        context.DeleteProjectMock.Verify(x => x.Execute(50, It.IsAny<CancellationToken>()));
        context.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(DeleteProjectError.Unknown, DeleteProjectUseCaseError.Unknown)]
    [InlineData(DeleteProjectError.UnknownProjectId, DeleteProjectUseCaseError.UnknownProjectId)]
    public async Task Execute_returns_errors_raised_by_repository_call(
        DeleteProjectError repositoryError,
        DeleteProjectUseCaseError expectedError
    )
    {
        var context = new TestContext()
            .WithResolvedUser(user: ResolvedUserBuilder.CreateAdministratorBob())
            .WithDeleteProjectFailure(repositoryError);

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            projectId: 50,
            cancellationToken: CancellationToken.None
        );
        result.TrySuccess(out var errorResult).ShouldBeFalse();

        errorResult.ShouldBe(expectedError);

        context.ResolveUserMock.Verify(x => x.Execute(It.IsAny<CancellationToken>()));
        context.DeleteProjectMock.Verify(x => x.Execute(50, It.IsAny<CancellationToken>()));
        context.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(
        ResolveUserError.UserNotAuthenticated,
        DeleteProjectUseCaseError.UnauthenticatedUser
    )]
    [InlineData(ResolveUserError.MalformedUserClaims, DeleteProjectUseCaseError.Unknown)]
    [InlineData(ResolveUserError.UnsupportedIdentityProvider, DeleteProjectUseCaseError.Unknown)]
    [InlineData(ResolveUserError.UserNotFound, DeleteProjectUseCaseError.Unknown)]
    [InlineData(ResolveUserError.UserRegistrationFailed, DeleteProjectUseCaseError.Unknown)]
    public async Task Execute_returns_error_for_user_resolve_error(
        ResolveUserError resolveUserError,
        DeleteProjectUseCaseError expectedError
    )
    {
        var context = new TestContext().WithResolveUserError(resolveUserError);

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            projectId: 50,
            cancellationToken: CancellationToken.None
        );
        result.TrySuccess(out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(expectedError);
    }

    [Theory]
    [InlineData(UserPermission.None)]
    [InlineData(UserPermission.Read)]
    public async Task Execute_returns_error_for_unauthorized_user(UserPermission projectsPermission)
    {
        var context = new TestContext().WithResolvedUser(
            user: ResolvedUserBuilder
                .AsAdministratorBob()
                .WithProjectsPermission(projectsPermission)
                .Build()
        );

        var result = await context
            .BuildUseCase()
            .Execute(projectId: 50, cancellationToken: CancellationToken.None);
        result.TrySuccess(out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(DeleteProjectUseCaseError.UnauthorizedUser);
    }

    private class TestContext
    {
        public Mock<IResolveUser> ResolveUserMock { get; } = new(MockBehavior.Strict);
        public Mock<IDeleteProject> DeleteProjectMock { get; } = new(MockBehavior.Strict);

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

        public TestContext WithSuccessfulDeleteProject()
        {
            DeleteProjectMock
                .Setup(x => x.Execute(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success<DeleteProjectError>());

            return this;
        }

        public TestContext WithDeleteProjectFailure(DeleteProjectError error)
        {
            DeleteProjectMock
                .Setup(x => x.Execute(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure(error));

            return this;
        }

        public DeleteProjectUseCase BuildUseCase() =>
            new(resolveUser: ResolveUserMock.Object, deleteProject: DeleteProjectMock.Object);

        public void VerifyNoOtherCalls()
        {
            ResolveUserMock.VerifyNoOtherCalls();
            DeleteProjectMock.VerifyNoOtherCalls();
        }
    }
}
