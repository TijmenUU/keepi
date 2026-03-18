using Keepi.Core.Unit.Tests.Builders;
using Keepi.Core.Users;
using Keepi.Generators;

namespace Keepi.Core.Unit.Tests.Users;

public class GetAllUsersUseCaseTests
{
    [Fact]
    public async Task Execute_returns_projects()
    {
        var context = new GetAllUsersUseCaseTestContext()
            .WithResolveUserSuccess(ResolvedUserBuilder.CreateAdministratorBob())
            .WithGetUsersSuccess(
                new GetUsersResultUser(
                    Id: UserId.From(1),
                    Name: UserName.From("Bob"),
                    EmailAddress: EmailAddress.From("bob@example.com"),
                    IdentityOrigin: UserIdentityProvider.GitHub,
                    EntriesPermission: UserPermission.None,
                    ExportsPermission: UserPermission.None,
                    ProjectsPermission: UserPermission.None,
                    UsersPermission: UserPermission.None
                ),
                new GetUsersResultUser(
                    Id: UserId.From(2),
                    Name: UserName.From("Miro"),
                    EmailAddress: EmailAddress.From("miro@example.com"),
                    IdentityOrigin: UserIdentityProvider.GitHub,
                    EntriesPermission: UserPermission.ReadAndModify,
                    ExportsPermission: UserPermission.ReadAndModify,
                    ProjectsPermission: UserPermission.ReadAndModify,
                    UsersPermission: UserPermission.ReadAndModify
                )
            );

        var useCase = context.BuildTarget();

        var result = await useCase.Execute(cancellationToken: CancellationToken.None);
        result.TrySuccess(out var successResult, out _).ShouldBeTrue();

        successResult.ShouldBeEquivalentTo(
            new GetUsersResult(
                Users:
                [
                    new(
                        Id: UserId.From(1),
                        Name: UserName.From("Bob"),
                        EmailAddress: EmailAddress.From("bob@example.com"),
                        IdentityOrigin: UserIdentityProvider.GitHub,
                        EntriesPermission: UserPermission.None,
                        ExportsPermission: UserPermission.None,
                        ProjectsPermission: UserPermission.None,
                        UsersPermission: UserPermission.None
                    ),
                    new(
                        Id: UserId.From(2),
                        Name: UserName.From("Miro"),
                        EmailAddress: EmailAddress.From("miro@example.com"),
                        IdentityOrigin: UserIdentityProvider.GitHub,
                        EntriesPermission: UserPermission.ReadAndModify,
                        ExportsPermission: UserPermission.ReadAndModify,
                        ProjectsPermission: UserPermission.ReadAndModify,
                        UsersPermission: UserPermission.ReadAndModify
                    ),
                ]
            )
        );

        context.ResolveUserMock.Verify(x => x.Execute(It.IsAny<CancellationToken>()));
        context.GetUsersMock.Verify(x => x.Execute(It.IsAny<CancellationToken>()));
        context.VerifyNoOtherCalls();
    }

    [Theory]
    // Base case
    [InlineData(UserPermission.None, UserPermission.None, UserPermission.None, UserPermission.None)]
    // Entries permission
    [InlineData(UserPermission.Read, UserPermission.None, UserPermission.None, UserPermission.None)]
    [InlineData(
        UserPermission.ReadAndModify,
        UserPermission.None,
        UserPermission.None,
        UserPermission.None
    )]
    // Exports permission
    [InlineData(UserPermission.None, UserPermission.Read, UserPermission.None, UserPermission.None)]
    [InlineData(
        UserPermission.None,
        UserPermission.ReadAndModify,
        UserPermission.None,
        UserPermission.None
    )]
    // Projects permission
    [InlineData(UserPermission.None, UserPermission.None, UserPermission.Read, UserPermission.None)]
    [InlineData(
        UserPermission.None,
        UserPermission.None,
        UserPermission.ReadAndModify,
        UserPermission.None
    )]
    // Users permission
    [InlineData(UserPermission.None, UserPermission.None, UserPermission.None, UserPermission.Read)]
    [InlineData(
        UserPermission.None,
        UserPermission.None,
        UserPermission.None,
        UserPermission.ReadAndModify
    )]
    public async Task Execute_maps_permissions_correctly(
        UserPermission entriesPermission,
        UserPermission exportsPermission,
        UserPermission projectsPermission,
        UserPermission usersPermission
    )
    {
        var context = new GetAllUsersUseCaseTestContext()
            .WithResolveUserSuccess(ResolvedUserBuilder.CreateAdministratorBob())
            .WithGetUsersSuccess(
                new GetUsersResultUser(
                    Id: UserId.From(1),
                    Name: UserName.From("Bobby"),
                    EmailAddress: EmailAddress.From("bobby@example.com"),
                    IdentityOrigin: UserIdentityProvider.GitHub,
                    EntriesPermission: entriesPermission,
                    ExportsPermission: exportsPermission,
                    ProjectsPermission: projectsPermission,
                    UsersPermission: usersPermission
                )
            );

        var result = await context.BuildTarget().Execute(cancellationToken: CancellationToken.None);

        result.TrySuccess(out var successResult, out _).ShouldBeTrue();
        successResult.ShouldBeEquivalentTo(
            new GetUsersResult(
                Users:
                [
                    new(
                        Id: UserId.From(1),
                        Name: UserName.From("Bobby"),
                        EmailAddress: EmailAddress.From("bobby@example.com"),
                        IdentityOrigin: UserIdentityProvider.GitHub,
                        EntriesPermission: entriesPermission,
                        ExportsPermission: exportsPermission,
                        ProjectsPermission: projectsPermission,
                        UsersPermission: usersPermission
                    ),
                ]
            )
        );
    }

    [Theory]
    [InlineData(GetUsersError.Unknown, GetAllUsersUseCaseError.Unknown)]
    public async Task Execute_returns_errors_raised_by_repository_call(
        GetUsersError repositoryError,
        GetAllUsersUseCaseError expectedError
    )
    {
        var context = new GetAllUsersUseCaseTestContext()
            .WithResolveUserSuccess(ResolvedUserBuilder.CreateAdministratorBob())
            .WithGetUsersError(repositoryError);

        var useCase = context.BuildTarget();

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
        var context = new GetAllUsersUseCaseTestContext().WithResolveUserError(resolveUserError);

        var result = await context.BuildTarget().Execute(cancellationToken: CancellationToken.None);

        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(expectedError);
    }

    [Fact]
    public async Task Execute_returns_error_for_unauthorized_user()
    {
        var context = new GetAllUsersUseCaseTestContext().WithResolveUserSuccess(
            ResolvedUserBuilder
                .AsAdministratorBob()
                .WithUsersPermission(UserPermission.None)
                .Build()
        );

        var result = await context.BuildTarget().Execute(cancellationToken: CancellationToken.None);
        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(GetAllUsersUseCaseError.UnauthorizedUser);
    }
}

[GenerateTestContext(TargetType = typeof(GetAllUsersUseCase), GenerateWithMethods = true)]
internal partial class GetAllUsersUseCaseTestContext
{
    public GetAllUsersUseCaseTestContext WithGetUsersSuccess(params GetUsersResultUser[] users) =>
        WithGetUsersSuccess(new GetUsersResult(Users: users));
}
