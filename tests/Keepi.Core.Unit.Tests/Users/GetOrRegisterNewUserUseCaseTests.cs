using Keepi.Core.Users;
using Keepi.Generators;

namespace Keepi.Core.Unit.Tests.Users;

public class GetOrRegisterNewUserUseCaseTests
{
    [Fact]
    public async Task Execute_returns_user_if_it_already_exists()
    {
        var context = new GetOrRegisterNewUserUseCaseTestContext().WithFirstGetUserResult(
            new GetUserResult(
                Id: 42,
                Name: "Bob",
                EmailAddress: "bob@example.com",
                IdentityOrigin: UserIdentityProvider.GitHub,
                EntriesPermission: UserPermission.ReadAndModify,
                ExportsPermission: UserPermission.ReadAndModify,
                ProjectsPermission: UserPermission.ReadAndModify,
                UsersPermission: UserPermission.ReadAndModify
            )
        );
        var helper = context.BuildTarget();

        var result = await helper.Execute(
            externalId: "github-33",
            emailAddress: "bob@example.com",
            name: "Bob",
            identityProvider: UserIdentityProvider.GitHub,
            cancellationToken: CancellationToken.None
        );
        result.TrySuccess(out var successResult, out _).ShouldBeTrue();

        successResult.ShouldBeEquivalentTo(
            new GetOrRegisterNewUserUseCaseOutput(
                User: new GetUserResult(
                    Id: 42,
                    Name: "Bob",
                    EmailAddress: "bob@example.com",
                    IdentityOrigin: UserIdentityProvider.GitHub,
                    EntriesPermission: UserPermission.ReadAndModify,
                    ExportsPermission: UserPermission.ReadAndModify,
                    ProjectsPermission: UserPermission.ReadAndModify,
                    UsersPermission: UserPermission.ReadAndModify
                ),
                NewlyRegistered: false
            )
        );

        context.GetUserMock.Verify(x =>
            x.Execute("github-33", UserIdentityProvider.GitHub, It.IsAny<CancellationToken>())
        );
        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_registers_user_as_first_admin_if_it_does_not_yet_exist()
    {
        var context = new GetOrRegisterNewUserUseCaseTestContext()
            .WithFirstGetUserErrorAndSecondWithResult(
                error: GetUserError.DoesNotExist,
                result: new GetUserResult(
                    Id: 42,
                    Name: "Bob",
                    EmailAddress: "bob@example.com",
                    IdentityOrigin: UserIdentityProvider.GitHub,
                    EntriesPermission: UserPermission.ReadAndModify,
                    ExportsPermission: UserPermission.ReadAndModify,
                    ProjectsPermission: UserPermission.ReadAndModify,
                    UsersPermission: UserPermission.ReadAndModify
                )
            )
            .WithUserWithPermissionsExistSuccess(result: false)
            .WithGetFirstAdminUserEmailAddressSuccess(result: "bob@example.com")
            .WithSaveNewUserResult(Result.Success<SaveNewUserError>());

        var helper = context.BuildTarget();

        var result = await helper.Execute(
            externalId: "github-33",
            emailAddress: "bob@example.com",
            name: "Bob",
            identityProvider: UserIdentityProvider.GitHub,
            cancellationToken: CancellationToken.None
        );
        result.TrySuccess(out var successResult, out _).ShouldBeTrue();

        successResult.ShouldBeEquivalentTo(
            new GetOrRegisterNewUserUseCaseOutput(
                User: new GetUserResult(
                    Id: 42,
                    Name: "Bob",
                    EmailAddress: "bob@example.com",
                    IdentityOrigin: UserIdentityProvider.GitHub,
                    EntriesPermission: UserPermission.ReadAndModify,
                    ExportsPermission: UserPermission.ReadAndModify,
                    ProjectsPermission: UserPermission.ReadAndModify,
                    UsersPermission: UserPermission.ReadAndModify
                ),
                NewlyRegistered: true
            )
        );

        context.GetUserMock.Verify(x =>
            x.Execute("github-33", UserIdentityProvider.GitHub, It.IsAny<CancellationToken>())
        );
        context.GetUserMock.Verify(x =>
            x.Execute("github-33", UserIdentityProvider.GitHub, It.IsAny<CancellationToken>())
        );
        context.UserWithPermissionsExistsMock.Verify(x =>
            x.Execute(
                UserPermission.ReadAndModify,
                UserPermission.ReadAndModify,
                UserPermission.ReadAndModify,
                UserPermission.ReadAndModify,
                It.IsAny<CancellationToken>()
            )
        );
        context.GetFirstAdminUserEmailAddressMock.Verify(x => x.Execute());
        context.SaveNewUserMock.Verify(x =>
            x.Execute(
                "github-33",
                "bob@example.com",
                "Bob",
                UserIdentityProvider.GitHub,
                UserPermission.ReadAndModify,
                UserPermission.ReadAndModify,
                UserPermission.ReadAndModify,
                UserPermission.ReadAndModify,
                It.IsAny<CancellationToken>()
            )
        );
        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_registers_user_as_normal_user_if_and_admin_already_exists()
    {
        var context = new GetOrRegisterNewUserUseCaseTestContext()
            .WithFirstGetUserErrorAndSecondWithResult(
                error: GetUserError.DoesNotExist,
                result: new GetUserResult(
                    Id: 42,
                    Name: "Bob",
                    EmailAddress: "bob@example.com",
                    IdentityOrigin: UserIdentityProvider.GitHub,
                    EntriesPermission: UserPermission.ReadAndModify,
                    ExportsPermission: UserPermission.None,
                    ProjectsPermission: UserPermission.None,
                    UsersPermission: UserPermission.None
                )
            )
            .WithUserWithPermissionsExistSuccess(result: true)
            .WithSaveNewUserResult(Result.Success<SaveNewUserError>());

        var helper = context.BuildTarget();

        var result = await helper.Execute(
            externalId: "github-33",
            emailAddress: "bob@example.com",
            name: "Bob",
            identityProvider: UserIdentityProvider.GitHub,
            cancellationToken: CancellationToken.None
        );
        result.TrySuccess(out var successResult, out _).ShouldBeTrue();

        successResult.ShouldBeEquivalentTo(
            new GetOrRegisterNewUserUseCaseOutput(
                User: new GetUserResult(
                    Id: 42,
                    Name: "Bob",
                    EmailAddress: "bob@example.com",
                    IdentityOrigin: UserIdentityProvider.GitHub,
                    EntriesPermission: UserPermission.ReadAndModify,
                    ExportsPermission: UserPermission.None,
                    ProjectsPermission: UserPermission.None,
                    UsersPermission: UserPermission.None
                ),
                NewlyRegistered: true
            )
        );

        context.GetUserMock.Verify(x =>
            x.Execute("github-33", UserIdentityProvider.GitHub, It.IsAny<CancellationToken>())
        );
        context.GetUserMock.Verify(x =>
            x.Execute("github-33", UserIdentityProvider.GitHub, It.IsAny<CancellationToken>())
        );
        context.UserWithPermissionsExistsMock.Verify(x =>
            x.Execute(
                UserPermission.ReadAndModify,
                UserPermission.ReadAndModify,
                UserPermission.ReadAndModify,
                UserPermission.ReadAndModify,
                It.IsAny<CancellationToken>()
            )
        );
        context.SaveNewUserMock.Verify(x =>
            x.Execute(
                "github-33",
                "bob@example.com",
                "Bob",
                UserIdentityProvider.GitHub,
                UserPermission.ReadAndModify,
                UserPermission.None,
                UserPermission.None,
                UserPermission.None,
                It.IsAny<CancellationToken>()
            )
        );
        context.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(GetFirstAdminUserEmailAddressError.Unknown)]
    [InlineData(GetFirstAdminUserEmailAddressError.NotConfigured)]
    public async Task Execute_registers_user_as_normal_user_if_first_admin_email_address_cannot_be_determined(
        GetFirstAdminUserEmailAddressError error
    )
    {
        var context = new GetOrRegisterNewUserUseCaseTestContext()
            .WithFirstGetUserErrorAndSecondWithResult(
                error: GetUserError.DoesNotExist,
                result: new GetUserResult(
                    Id: 42,
                    Name: "Bob",
                    EmailAddress: "bob@example.com",
                    IdentityOrigin: UserIdentityProvider.GitHub,
                    EntriesPermission: UserPermission.ReadAndModify,
                    ExportsPermission: UserPermission.None,
                    ProjectsPermission: UserPermission.None,
                    UsersPermission: UserPermission.None
                )
            )
            .WithUserWithPermissionsExistSuccess(result: false)
            .WithGetFirstAdminUserEmailAddressFailure(error)
            .WithSaveNewUserResult(Result.Success<SaveNewUserError>());

        var helper = context.BuildTarget();

        var result = await helper.Execute(
            externalId: "github-33",
            emailAddress: "bob@example.com",
            name: "Bob",
            identityProvider: UserIdentityProvider.GitHub,
            cancellationToken: CancellationToken.None
        );
        result.TrySuccess(out var successResult, out _).ShouldBeTrue();

        successResult.ShouldBeEquivalentTo(
            new GetOrRegisterNewUserUseCaseOutput(
                User: new GetUserResult(
                    Id: 42,
                    Name: "Bob",
                    EmailAddress: "bob@example.com",
                    IdentityOrigin: UserIdentityProvider.GitHub,
                    EntriesPermission: UserPermission.ReadAndModify,
                    ExportsPermission: UserPermission.None,
                    ProjectsPermission: UserPermission.None,
                    UsersPermission: UserPermission.None
                ),
                NewlyRegistered: true
            )
        );

        context.GetUserMock.Verify(x =>
            x.Execute("github-33", UserIdentityProvider.GitHub, It.IsAny<CancellationToken>())
        );
        context.GetUserMock.Verify(x =>
            x.Execute("github-33", UserIdentityProvider.GitHub, It.IsAny<CancellationToken>())
        );
        context.UserWithPermissionsExistsMock.Verify(x =>
            x.Execute(
                UserPermission.ReadAndModify,
                UserPermission.ReadAndModify,
                UserPermission.ReadAndModify,
                UserPermission.ReadAndModify,
                It.IsAny<CancellationToken>()
            )
        );
        context.GetFirstAdminUserEmailAddressMock.Verify(x => x.Execute());
        context.SaveNewUserMock.Verify(x =>
            x.Execute(
                "github-33",
                "bob@example.com",
                "Bob",
                UserIdentityProvider.GitHub,
                UserPermission.ReadAndModify,
                UserPermission.None,
                UserPermission.None,
                UserPermission.None,
                It.IsAny<CancellationToken>()
            )
        );
        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_updates_user_if_the_name_changed()
    {
        var context = new GetOrRegisterNewUserUseCaseTestContext()
            .WithFirstGetUserResult(
                new GetUserResult(
                    Id: 42,
                    Name: "Bob",
                    EmailAddress: "bob@example.com",
                    IdentityOrigin: UserIdentityProvider.GitHub,
                    EntriesPermission: UserPermission.ReadAndModify,
                    ExportsPermission: UserPermission.ReadAndModify,
                    ProjectsPermission: UserPermission.ReadAndModify,
                    UsersPermission: UserPermission.ReadAndModify
                )
            )
            .WithUserUpdateSuccess();
        var helper = context.BuildTarget();

        var result = await helper.Execute(
            externalId: "github-33",
            emailAddress: "bob@example.com",
            name: "Bobby",
            identityProvider: UserIdentityProvider.GitHub,
            cancellationToken: CancellationToken.None
        );
        result.TrySuccess(out var successResult, out _).ShouldBeTrue();

        successResult.ShouldBeEquivalentTo(
            new GetOrRegisterNewUserUseCaseOutput(
                User: new GetUserResult(
                    Id: 42,
                    Name: "Bobby",
                    EmailAddress: "bob@example.com",
                    IdentityOrigin: UserIdentityProvider.GitHub,
                    EntriesPermission: UserPermission.ReadAndModify,
                    ExportsPermission: UserPermission.ReadAndModify,
                    ProjectsPermission: UserPermission.ReadAndModify,
                    UsersPermission: UserPermission.ReadAndModify
                ),
                NewlyRegistered: false
            )
        );

        context.GetUserMock.Verify(x =>
            x.Execute("github-33", UserIdentityProvider.GitHub, It.IsAny<CancellationToken>())
        );
        context.UpdateUserIdentityMock.Verify(x =>
            x.Execute(42, "bob@example.com", "Bobby", It.IsAny<CancellationToken>())
        );
        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_updates_user_if_the_email_address_changed()
    {
        var context = new GetOrRegisterNewUserUseCaseTestContext()
            .WithFirstGetUserResult(
                new GetUserResult(
                    Id: 42,
                    Name: "Bob",
                    EmailAddress: "bob@example.com",
                    IdentityOrigin: UserIdentityProvider.GitHub,
                    EntriesPermission: UserPermission.ReadAndModify,
                    ExportsPermission: UserPermission.ReadAndModify,
                    ProjectsPermission: UserPermission.ReadAndModify,
                    UsersPermission: UserPermission.ReadAndModify
                )
            )
            .WithUserUpdateSuccess();
        var helper = context.BuildTarget();

        var result = await helper.Execute(
            externalId: "github-33",
            emailAddress: "bobby@example.com",
            name: "Bob",
            identityProvider: UserIdentityProvider.GitHub,
            cancellationToken: CancellationToken.None
        );
        result.TrySuccess(out var successResult, out _).ShouldBeTrue();

        successResult.ShouldBeEquivalentTo(
            new GetOrRegisterNewUserUseCaseOutput(
                User: new GetUserResult(
                    Id: 42,
                    Name: "Bob",
                    EmailAddress: "bobby@example.com",
                    IdentityOrigin: UserIdentityProvider.GitHub,
                    EntriesPermission: UserPermission.ReadAndModify,
                    ExportsPermission: UserPermission.ReadAndModify,
                    ProjectsPermission: UserPermission.ReadAndModify,
                    UsersPermission: UserPermission.ReadAndModify
                ),
                NewlyRegistered: false
            )
        );

        context.GetUserMock.Verify(x =>
            x.Execute("github-33", UserIdentityProvider.GitHub, It.IsAny<CancellationToken>())
        );
        context.UpdateUserIdentityMock.Verify(x =>
            x.Execute(42, "bobby@example.com", "Bob", It.IsAny<CancellationToken>())
        );
        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_logs_user_update_failure_and_returns_non_updated_values()
    {
        var context = new GetOrRegisterNewUserUseCaseTestContext()
            .WithFirstGetUserResult(
                new GetUserResult(
                    Id: 42,
                    Name: "Bob",
                    EmailAddress: "bob@example.com",
                    IdentityOrigin: UserIdentityProvider.GitHub,
                    EntriesPermission: UserPermission.ReadAndModify,
                    ExportsPermission: UserPermission.ReadAndModify,
                    ProjectsPermission: UserPermission.ReadAndModify,
                    UsersPermission: UserPermission.ReadAndModify
                )
            )
            .WithUserUpdateFailure();
        var helper = context.BuildTarget();

        var result = await helper.Execute(
            externalId: "github-33",
            emailAddress: "bobby@example.com",
            name: "Bob",
            identityProvider: UserIdentityProvider.GitHub,
            cancellationToken: CancellationToken.None
        );
        result.TrySuccess(out var successResult, out _).ShouldBeTrue();

        successResult.ShouldBeEquivalentTo(
            new GetOrRegisterNewUserUseCaseOutput(
                User: new GetUserResult(
                    Id: 42,
                    Name: "Bob",
                    EmailAddress: "bob@example.com",
                    IdentityOrigin: UserIdentityProvider.GitHub,
                    EntriesPermission: UserPermission.ReadAndModify,
                    ExportsPermission: UserPermission.ReadAndModify,
                    ProjectsPermission: UserPermission.ReadAndModify,
                    UsersPermission: UserPermission.ReadAndModify
                ),
                NewlyRegistered: false
            )
        );

        context.GetUserMock.Verify(x =>
            x.Execute("github-33", UserIdentityProvider.GitHub, It.IsAny<CancellationToken>())
        );
        context.UpdateUserIdentityMock.Verify(x =>
            x.Execute(42, "bobby@example.com", "Bob", It.IsAny<CancellationToken>())
        );
        context.LoggerMock.VerifyWarningLog(
            expectedMessage: "Failed to update GitHub user github-33 due to DuplicateUser error"
        );
        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_returns_error_for_unknown_user_retrieval_failure()
    {
        var context = new GetOrRegisterNewUserUseCaseTestContext().WithFirstGetUserError(
            GetUserError.Unknown
        );

        var helper = context.BuildTarget();

        var result = await helper.Execute(
            externalId: "github-33",
            emailAddress: "bob@example.com",
            name: "Bob",
            identityProvider: UserIdentityProvider.GitHub,
            cancellationToken: CancellationToken.None
        );
        result.TrySuccess(out var _, out var errorResult).ShouldBeFalse();

        errorResult.ShouldBe(GetOrRegisterNewUserUseCaseError.Unknown);

        context.GetUserMock.Verify(x =>
            x.Execute("github-33", UserIdentityProvider.GitHub, It.IsAny<CancellationToken>())
        );
        context.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(SaveNewUserError.Unknown)]
    [InlineData(SaveNewUserError.DuplicateUser)]
    public async Task Execute_returns_error_for_user_registration_failure(SaveNewUserError error)
    {
        var context = new GetOrRegisterNewUserUseCaseTestContext()
            .WithFirstGetUserErrorAndSecondWithResult(
                error: GetUserError.DoesNotExist,
                result: new GetUserResult(
                    Id: 42,
                    Name: "Bob",
                    EmailAddress: "bob@example.com",
                    IdentityOrigin: UserIdentityProvider.GitHub,
                    EntriesPermission: UserPermission.ReadAndModify,
                    ExportsPermission: UserPermission.ReadAndModify,
                    ProjectsPermission: UserPermission.ReadAndModify,
                    UsersPermission: UserPermission.ReadAndModify
                )
            )
            .WithUserWithPermissionsExistSuccess(result: false)
            .WithGetFirstAdminUserEmailAddressSuccess(result: "bob@example.com")
            .WithSaveNewUserResult(Result.Failure(error));

        var helper = context.BuildTarget();

        var result = await helper.Execute(
            externalId: "github-33",
            emailAddress: "bob@example.com",
            name: "Bob",
            identityProvider: UserIdentityProvider.GitHub,
            cancellationToken: CancellationToken.None
        );
        result.TrySuccess(out var _, out var errorResult).ShouldBeFalse();

        errorResult.ShouldBe(GetOrRegisterNewUserUseCaseError.RegistrationFailed);

        context.GetUserMock.Verify(x =>
            x.Execute("github-33", UserIdentityProvider.GitHub, It.IsAny<CancellationToken>())
        );
        context.UserWithPermissionsExistsMock.Verify(x =>
            x.Execute(
                UserPermission.ReadAndModify,
                UserPermission.ReadAndModify,
                UserPermission.ReadAndModify,
                UserPermission.ReadAndModify,
                It.IsAny<CancellationToken>()
            )
        );
        context.GetFirstAdminUserEmailAddressMock.Verify(x => x.Execute());
        context.SaveNewUserMock.Verify(x =>
            x.Execute(
                "github-33",
                "bob@example.com",
                "Bob",
                UserIdentityProvider.GitHub,
                UserPermission.ReadAndModify,
                UserPermission.ReadAndModify,
                UserPermission.ReadAndModify,
                UserPermission.ReadAndModify,
                It.IsAny<CancellationToken>()
            )
        );
        context.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(GetUserError.DoesNotExist)]
    [InlineData(GetUserError.Unknown)]
    public async Task Execute_returns_error_for_post_registration_unknown_user_retrieval_failure(
        GetUserError secondGetUserErrror
    )
    {
        var context = new GetOrRegisterNewUserUseCaseTestContext()
            .WithFirstGetUserErrorAndSecondError(
                firstError: GetUserError.DoesNotExist,
                secondError: secondGetUserErrror
            )
            .WithUserWithPermissionsExistSuccess(result: false)
            .WithGetFirstAdminUserEmailAddressSuccess(result: "bob@example.com")
            .WithSaveNewUserResult(Result.Success<SaveNewUserError>());

        var helper = context.BuildTarget();

        var result = await helper.Execute(
            externalId: "github-33",
            emailAddress: "bob@example.com",
            name: "Bob",
            identityProvider: UserIdentityProvider.GitHub,
            cancellationToken: CancellationToken.None
        );
        result.TrySuccess(out var _, out var errorResult).ShouldBeFalse();

        errorResult.ShouldBe(GetOrRegisterNewUserUseCaseError.Unknown);

        context.GetUserMock.Verify(x =>
            x.Execute("github-33", UserIdentityProvider.GitHub, It.IsAny<CancellationToken>())
        );
        context.GetUserMock.Verify(x =>
            x.Execute("github-33", UserIdentityProvider.GitHub, It.IsAny<CancellationToken>())
        );
        context.UserWithPermissionsExistsMock.Verify(x =>
            x.Execute(
                UserPermission.ReadAndModify,
                UserPermission.ReadAndModify,
                UserPermission.ReadAndModify,
                UserPermission.ReadAndModify,
                It.IsAny<CancellationToken>()
            )
        );
        context.GetFirstAdminUserEmailAddressMock.Verify(x => x.Execute());
        context.SaveNewUserMock.Verify(x =>
            x.Execute(
                "github-33",
                "bob@example.com",
                "Bob",
                UserIdentityProvider.GitHub,
                UserPermission.ReadAndModify,
                UserPermission.ReadAndModify,
                UserPermission.ReadAndModify,
                UserPermission.ReadAndModify,
                It.IsAny<CancellationToken>()
            )
        );
        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_returns_error_if_admin_user_exists_check_fails()
    {
        var context = new GetOrRegisterNewUserUseCaseTestContext()
            .WithFirstGetUserErrorAndSecondWithResult(
                error: GetUserError.DoesNotExist,
                result: new GetUserResult(
                    Id: 42,
                    Name: "Bob",
                    EmailAddress: "bob@example.com",
                    IdentityOrigin: UserIdentityProvider.GitHub,
                    EntriesPermission: UserPermission.ReadAndModify,
                    ExportsPermission: UserPermission.ReadAndModify,
                    ProjectsPermission: UserPermission.ReadAndModify,
                    UsersPermission: UserPermission.ReadAndModify
                )
            )
            .WithUserWithPermissionsExistFailure(UserWithPermissionsExistsError.Unknown);

        var helper = context.BuildTarget();

        var result = await helper.Execute(
            externalId: "github-33",
            emailAddress: "bob@example.com",
            name: "Bob",
            identityProvider: UserIdentityProvider.GitHub,
            cancellationToken: CancellationToken.None
        );
        result.TrySuccess(out var _, out var errorResult).ShouldBeFalse();

        errorResult.ShouldBe(GetOrRegisterNewUserUseCaseError.RegistrationFailed);

        context.GetUserMock.Verify(x =>
            x.Execute("github-33", UserIdentityProvider.GitHub, It.IsAny<CancellationToken>())
        );
        context.GetUserMock.Verify(x =>
            x.Execute("github-33", UserIdentityProvider.GitHub, It.IsAny<CancellationToken>())
        );
        context.UserWithPermissionsExistsMock.Verify(x =>
            x.Execute(
                UserPermission.ReadAndModify,
                UserPermission.ReadAndModify,
                UserPermission.ReadAndModify,
                UserPermission.ReadAndModify,
                It.IsAny<CancellationToken>()
            )
        );
        context.VerifyNoOtherCalls();
    }
}

[GenerateTestContext(TargetType = typeof(GetOrRegisterNewUserUseCase))]
internal partial class GetOrRegisterNewUserUseCaseTestContext
{
    public GetOrRegisterNewUserUseCaseTestContext WithFirstGetUserResult(GetUserResult result)
    {
        GetUserMock
            .Setup(x =>
                x.Execute(
                    It.IsAny<string>(),
                    It.IsAny<UserIdentityProvider>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Success<GetUserResult, GetUserError>(result));

        return this;
    }

    public GetOrRegisterNewUserUseCaseTestContext WithFirstGetUserError(GetUserError error)
    {
        GetUserMock
            .Setup(x =>
                x.Execute(
                    It.IsAny<string>(),
                    It.IsAny<UserIdentityProvider>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Failure<GetUserResult, GetUserError>(error));

        return this;
    }

    public GetOrRegisterNewUserUseCaseTestContext WithFirstGetUserErrorAndSecondWithResult(
        GetUserError error,
        GetUserResult result
    )
    {
        GetUserMock
            .SetupSequence(x =>
                x.Execute(
                    It.IsAny<string>(),
                    It.IsAny<UserIdentityProvider>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Failure<GetUserResult, GetUserError>(error))
            .ReturnsAsync(Result.Success<GetUserResult, GetUserError>(result));

        return this;
    }

    public GetOrRegisterNewUserUseCaseTestContext WithFirstGetUserErrorAndSecondError(
        GetUserError firstError,
        GetUserError secondError
    )
    {
        GetUserMock
            .SetupSequence(x =>
                x.Execute(
                    It.IsAny<string>(),
                    It.IsAny<UserIdentityProvider>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Failure<GetUserResult, GetUserError>(firstError))
            .ReturnsAsync(Result.Failure<GetUserResult, GetUserError>(secondError));

        return this;
    }

    public GetOrRegisterNewUserUseCaseTestContext WithUserUpdateSuccess() =>
        WithUserUpdateResult(Result.Success<UpdateUserIdentityError>());

    public GetOrRegisterNewUserUseCaseTestContext WithUserUpdateFailure() =>
        WithUserUpdateResult(Result.Failure(UpdateUserIdentityError.DuplicateUser));

    public GetOrRegisterNewUserUseCaseTestContext WithUserUpdateResult(
        IMaybeErrorResult<UpdateUserIdentityError> result
    )
    {
        UpdateUserIdentityMock
            .Setup(x =>
                x.Execute(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(result);

        return this;
    }

    public GetOrRegisterNewUserUseCaseTestContext WithUserWithPermissionsExistSuccess(
        bool result
    ) =>
        WithUserWithPermissionsExistResult(
            Result.Success<bool, UserWithPermissionsExistsError>(result)
        );

    public GetOrRegisterNewUserUseCaseTestContext WithUserWithPermissionsExistFailure(
        UserWithPermissionsExistsError error
    ) =>
        WithUserWithPermissionsExistResult(
            Result.Failure<bool, UserWithPermissionsExistsError>(error)
        );

    private GetOrRegisterNewUserUseCaseTestContext WithUserWithPermissionsExistResult(
        IValueOrErrorResult<bool, UserWithPermissionsExistsError> result
    )
    {
        UserWithPermissionsExistsMock
            .Setup(x =>
                x.Execute(
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

    public GetOrRegisterNewUserUseCaseTestContext WithGetFirstAdminUserEmailAddressSuccess(
        string result
    ) =>
        WithGetFirstAdminUserEmailAddressResult(
            Result.Success<string, GetFirstAdminUserEmailAddressError>(result)
        );

    public GetOrRegisterNewUserUseCaseTestContext WithGetFirstAdminUserEmailAddressFailure(
        GetFirstAdminUserEmailAddressError error
    ) =>
        WithGetFirstAdminUserEmailAddressResult(
            Result.Failure<string, GetFirstAdminUserEmailAddressError>(error)
        );

    private GetOrRegisterNewUserUseCaseTestContext WithGetFirstAdminUserEmailAddressResult(
        IValueOrErrorResult<string, GetFirstAdminUserEmailAddressError> result
    )
    {
        GetFirstAdminUserEmailAddressMock.Setup(x => x.Execute()).Returns(result);

        return this;
    }

    public GetOrRegisterNewUserUseCaseTestContext WithSaveNewUserResult(
        IMaybeErrorResult<SaveNewUserError> result
    )
    {
        SaveNewUserMock
            .Setup(x =>
                x.Execute(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<UserIdentityProvider>(),
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
}
