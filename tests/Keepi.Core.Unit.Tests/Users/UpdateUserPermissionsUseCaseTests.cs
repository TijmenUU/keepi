using Keepi.Core.Unit.Tests.Builders;
using Keepi.Core.Users;

namespace Keepi.Core.Unit.Tests.Users;

public class UpdateUserPermissionsUseCaseTests
{
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
        UserPermission.Read
    )]
    // Users permission
    [InlineData(UserPermission.None, UserPermission.None, UserPermission.None, UserPermission.Read)]
    [InlineData(
        UserPermission.None,
        UserPermission.None,
        UserPermission.None,
        UserPermission.ReadAndModify
    )]
    public async Task Execute_stores_expected_values(
        UserPermission entriesPermission,
        UserPermission exportsPermission,
        UserPermission projectsPermission,
        UserPermission usersPermission
    )
    {
        var context = new TestContext()
            .WithResolvedUser(user: ResolvedUserBuilder.CreateAdministratorBob())
            .WithUpdateUserPermissionsSuccess();

        var result = await context
            .BuildUseCase()
            .Execute(
                userId: 43,
                entriesPermission: entriesPermission,
                exportsPermission: exportsPermission,
                projectsPermission: projectsPermission,
                usersPermission: usersPermission,
                cancellationToken: CancellationToken.None
            );

        result.TrySuccess(out _).ShouldBeTrue();

        context.ResolveUserMock.Verify(x => x.Execute(It.IsAny<CancellationToken>()));
        context.UpdateUserPermissionsMock.Verify(x =>
            x.Execute(
                43,
                entriesPermission,
                exportsPermission,
                projectsPermission,
                usersPermission,
                It.IsAny<CancellationToken>()
            )
        );
        context.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(
        ResolveUserError.UserNotAuthenticated,
        UpdateUserPermissionsUseCaseError.UnauthenticatedUser
    )]
    [InlineData(ResolveUserError.MalformedUserClaims, UpdateUserPermissionsUseCaseError.Unknown)]
    [InlineData(
        ResolveUserError.UnsupportedIdentityProvider,
        UpdateUserPermissionsUseCaseError.Unknown
    )]
    [InlineData(ResolveUserError.UserNotFound, UpdateUserPermissionsUseCaseError.Unknown)]
    [InlineData(ResolveUserError.UserRegistrationFailed, UpdateUserPermissionsUseCaseError.Unknown)]
    public async Task Execute_returns_error_for_user_resolve_error(
        ResolveUserError resolveUserError,
        UpdateUserPermissionsUseCaseError expectedError
    )
    {
        var context = new TestContext().WithResolveUserError(resolveUserError);

        var result = await context
            .BuildUseCase()
            .Execute(
                userId: 43,
                entriesPermission: UserPermission.ReadAndModify,
                exportsPermission: UserPermission.ReadAndModify,
                projectsPermission: UserPermission.ReadAndModify,
                usersPermission: UserPermission.ReadAndModify,
                cancellationToken: CancellationToken.None
            );
        result.TrySuccess(out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(expectedError);
    }

    [Theory]
    [InlineData(UserPermission.None)]
    [InlineData(UserPermission.Read)]
    public async Task Execute_returns_error_for_unauthorized_user(UserPermission usersPermission)
    {
        var context = new TestContext().WithResolvedUser(
            user: ResolvedUserBuilder
                .AsAdministratorBob()
                .WithUsersPermission(usersPermission)
                .Build()
        );

        var result = await context
            .BuildUseCase()
            .Execute(
                userId: 43,
                entriesPermission: UserPermission.ReadAndModify,
                exportsPermission: UserPermission.ReadAndModify,
                projectsPermission: UserPermission.ReadAndModify,
                usersPermission: UserPermission.ReadAndModify,
                cancellationToken: CancellationToken.None
            );
        result.TrySuccess(out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(UpdateUserPermissionsUseCaseError.UnauthorizedUser);
    }

    [Fact]
    public async Task Execute_returns_error_for_user_that_modifies_their_own_permissions()
    {
        var context = new TestContext().WithResolvedUser(
            user: ResolvedUserBuilder.CreateAdministratorBob()
        );

        var result = await context
            .BuildUseCase()
            .Execute(
                userId: 42,
                entriesPermission: UserPermission.ReadAndModify,
                exportsPermission: UserPermission.ReadAndModify,
                projectsPermission: UserPermission.ReadAndModify,
                usersPermission: UserPermission.ReadAndModify,
                cancellationToken: CancellationToken.None
            );
        result.TrySuccess(out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(UpdateUserPermissionsUseCaseError.CannotModifyPermissionsOfSelf);
    }

    [Fact]
    public async Task Execute_returns_error_for_unsupported_permission_combination()
    {
        var context = new TestContext().WithResolvedUser(
            user: ResolvedUserBuilder.CreateAdministratorBob()
        );

        var result = await context
            .BuildUseCase()
            .Execute(
                userId: 43,
                entriesPermission: UserPermission.ReadAndModify,
                exportsPermission: UserPermission.ReadAndModify,
                projectsPermission: UserPermission.ReadAndModify,
                usersPermission: UserPermission.None,
                cancellationToken: CancellationToken.None
            );
        result.TrySuccess(out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(
            UpdateUserPermissionsUseCaseError.IncompatibleUserPermissionsCombination
        );
    }

    [Theory]
    [InlineData(UpdateUserPermissionsError.Unknown, UpdateUserPermissionsUseCaseError.Unknown)]
    [InlineData(
        UpdateUserPermissionsError.UnknownUserId,
        UpdateUserPermissionsUseCaseError.UnknownUserId
    )]
    public async Task Execute_returns_error_raised_by_repository_method(
        UpdateUserPermissionsError repositoryError,
        UpdateUserPermissionsUseCaseError expectedError
    )
    {
        var context = new TestContext()
            .WithResolvedUser(user: ResolvedUserBuilder.CreateAdministratorBob())
            .WithUpdateUserPermissionsFailure(repositoryError);

        var result = await context
            .BuildUseCase()
            .Execute(
                userId: 43,
                entriesPermission: UserPermission.ReadAndModify,
                exportsPermission: UserPermission.ReadAndModify,
                projectsPermission: UserPermission.ReadAndModify,
                usersPermission: UserPermission.ReadAndModify,
                cancellationToken: CancellationToken.None
            );
        result.TrySuccess(out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(expectedError);
    }

    private class TestContext
    {
        public Mock<IResolveUser> ResolveUserMock { get; } = new(MockBehavior.Strict);
        public Mock<IUpdateUserPermissions> UpdateUserPermissionsMock { get; } =
            new(MockBehavior.Strict);

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

        public TestContext WithUpdateUserPermissionsSuccess() =>
            WithUpdateUserPermissionsResult(Result.Success<UpdateUserPermissionsError>());

        public TestContext WithUpdateUserPermissionsFailure(UpdateUserPermissionsError error) =>
            WithUpdateUserPermissionsResult(Result.Failure(error));

        private TestContext WithUpdateUserPermissionsResult(
            IMaybeErrorResult<UpdateUserPermissionsError> result
        )
        {
            UpdateUserPermissionsMock
                .Setup(x =>
                    x.Execute(
                        It.IsAny<int>(),
                        It.IsAny<UserPermission>(),
                        It.IsAny<UserPermission>(),
                        It.IsAny<UserPermission>(),
                        It.IsAny<UserPermission>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(result);
            return this;
        }

        public UpdateUserPermissionsUseCase BuildUseCase() =>
            new(
                resolveUser: ResolveUserMock.Object,
                updateUserPermissions: UpdateUserPermissionsMock.Object
            );

        public void VerifyNoOtherCalls()
        {
            ResolveUserMock.VerifyNoOtherCalls();
            UpdateUserPermissionsMock.VerifyNoOtherCalls();
        }
    }
}
