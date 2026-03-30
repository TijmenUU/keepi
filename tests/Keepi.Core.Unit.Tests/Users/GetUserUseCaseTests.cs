using Keepi.Core.Unit.Tests.Builders;
using Keepi.Core.Users;
using Keepi.Generators;

namespace Keepi.Core.Unit.Tests.Users;

public class GetUserUseCaseTests
{
    [Fact]
    public async Task Execute_returns_expected_user()
    {
        var context = new GetUserUseCaseTestContext().WithResolveUserSuccess(
            ResolvedUserBuilder.CreateAdministratorBob()
        );

        var result = await context.BuildTarget().Execute(cancellationToken: CancellationToken.None);

        result.TrySuccess(out var successResult, out _).ShouldBeTrue();
        successResult.ShouldBeEquivalentTo(
            new GetUserUseCaseOutput(
                Id: UserId.From(42),
                Name: UserName.From("Bob"),
                EmailAddress: EmailAddress.From("bob@example.com"),
                EntriesPermission: UserPermission.ReadAndModify,
                ExportsPermission: UserPermission.ReadAndModify,
                ProjectsPermission: UserPermission.ReadAndModify,
                UsersPermission: UserPermission.ReadAndModify
            )
        );
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
        var context = new GetUserUseCaseTestContext().WithResolveUserSuccess(
            ResolvedUserBuilder
                .AsAdministratorBob()
                .WithEntriesPermission(entriesPermission)
                .WithExportsPermission(exportsPermission)
                .WithProjectsPermission(projectsPermission)
                .WithUsersPermission(usersPermission)
                .Build()
        );

        var result = await context.BuildTarget().Execute(cancellationToken: CancellationToken.None);

        result.TrySuccess(out var successResult, out _).ShouldBeTrue();
        successResult.ShouldBeEquivalentTo(
            new GetUserUseCaseOutput(
                Id: UserId.From(42),
                Name: UserName.From("Bob"),
                EmailAddress: EmailAddress.From("bob@example.com"),
                EntriesPermission: entriesPermission,
                ExportsPermission: exportsPermission,
                ProjectsPermission: projectsPermission,
                UsersPermission: usersPermission
            )
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
        var context = new GetUserUseCaseTestContext().WithResolveUserError(resolveUserError);

        var result = await context.BuildTarget().Execute(cancellationToken: CancellationToken.None);

        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(expectedError);
    }
}

[GenerateTestContext(targetType: typeof(GetUserUseCase), GenerateWithMethods = true)]
internal partial class GetUserUseCaseTestContext { }
