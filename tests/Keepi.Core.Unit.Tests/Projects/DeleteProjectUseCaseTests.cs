using Keepi.Core.Projects;
using Keepi.Core.Unit.Tests.Builders;
using Keepi.Core.Users;
using Keepi.Generators;

namespace Keepi.Core.Unit.Tests.Projects;

public class DeleteProjectUseCaseTests
{
    [Fact]
    public async Task Execute_returns_success()
    {
        var context = new DeleteProjectUseCaseTestContext()
            .WithResolveUserSuccess(ResolvedUserBuilder.CreateAdministratorBob())
            .WithDeleteProjectSuccess();

        var useCase = context.BuildTarget();

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
        var context = new DeleteProjectUseCaseTestContext()
            .WithResolveUserSuccess(ResolvedUserBuilder.CreateAdministratorBob())
            .WithDeleteProjectError(repositoryError);

        var useCase = context.BuildTarget();

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
        var context = new DeleteProjectUseCaseTestContext().WithResolveUserError(resolveUserError);

        var useCase = context.BuildTarget();

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
        var context = new DeleteProjectUseCaseTestContext().WithResolveUserSuccess(
            ResolvedUserBuilder
                .AsAdministratorBob()
                .WithProjectsPermission(projectsPermission)
                .Build()
        );

        var result = await context
            .BuildTarget()
            .Execute(projectId: 50, cancellationToken: CancellationToken.None);
        result.TrySuccess(out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(DeleteProjectUseCaseError.UnauthorizedUser);
    }
}

[GenerateTestContext(TargetType = typeof(DeleteProjectUseCase), GenerateWithCallMethods = true)]
internal partial class DeleteProjectUseCaseTestContext { }
