using Keepi.Core.Users;
using Keepi.Generators;

namespace Keepi.Core.Unit.Tests.Users;

public class GetOrRegisterNewUserUseCaseTests
{
    [Fact]
    public async Task Execute_returns_user_if_it_already_exists()
    {
        var context = new GetOrRegisterNewUserUseCaseTestContext().WithGetUserSuccess(
            new GetUserResult(
                Id: UserId.From(42),
                Name: UserName.From("Bob"),
                EmailAddress: EmailAddress.From("bob@example.com"),
                IdentityOrigin: UserIdentityProvider.GitHub,
                EntriesPermission: UserPermission.ReadAndModify,
                ExportsPermission: UserPermission.ReadAndModify,
                ProjectsPermission: UserPermission.ReadAndModify,
                UsersPermission: UserPermission.ReadAndModify
            )
        );
        var helper = context.BuildTarget();

        var result = await helper.Execute(
            externalId: UserExternalId.From("github-33"),
            emailAddress: EmailAddress.From("bob@example.com"),
            name: UserName.From("Bob"),
            identityProvider: UserIdentityProvider.GitHub,
            cancellationToken: CancellationToken.None
        );
        result.TrySuccess(out var successResult, out _).ShouldBeTrue();

        successResult.ShouldBeEquivalentTo(
            new GetOrRegisterNewUserUseCaseOutput(
                User: new GetUserResult(
                    Id: UserId.From(42),
                    Name: UserName.From("Bob"),
                    EmailAddress: EmailAddress.From("bob@example.com"),
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
            x.Execute(
                UserExternalId.From("github-33"),
                UserIdentityProvider.GitHub,
                It.IsAny<CancellationToken>()
            )
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
                    Id: UserId.From(42),
                    Name: UserName.From("Bob"),
                    EmailAddress: EmailAddress.From("bob@example.com"),
                    IdentityOrigin: UserIdentityProvider.GitHub,
                    EntriesPermission: UserPermission.ReadAndModify,
                    ExportsPermission: UserPermission.ReadAndModify,
                    ProjectsPermission: UserPermission.ReadAndModify,
                    UsersPermission: UserPermission.ReadAndModify
                )
            )
            .WithUserWithPermissionsExistsSuccess(result: false)
            .WithGetFirstAdminUserEmailAddressSuccess(result: EmailAddress.From("bob@example.com"))
            .WithSaveNewUserSuccess();

        var helper = context.BuildTarget();

        var result = await helper.Execute(
            externalId: UserExternalId.From("github-33"),
            emailAddress: EmailAddress.From("bob@example.com"),
            name: UserName.From("Bob"),
            identityProvider: UserIdentityProvider.GitHub,
            cancellationToken: CancellationToken.None
        );
        result.TrySuccess(out var successResult, out _).ShouldBeTrue();

        successResult.ShouldBeEquivalentTo(
            new GetOrRegisterNewUserUseCaseOutput(
                User: new GetUserResult(
                    Id: UserId.From(42),
                    Name: UserName.From("Bob"),
                    EmailAddress: EmailAddress.From("bob@example.com"),
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
            x.Execute(
                UserExternalId.From("github-33"),
                UserIdentityProvider.GitHub,
                It.IsAny<CancellationToken>()
            )
        );
        context.GetUserMock.Verify(x =>
            x.Execute(
                UserExternalId.From("github-33"),
                UserIdentityProvider.GitHub,
                It.IsAny<CancellationToken>()
            )
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
                UserExternalId.From("github-33"),
                EmailAddress.From("bob@example.com"),
                UserName.From("Bob"),
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
                    Id: UserId.From(42),
                    Name: UserName.From("Bob"),
                    EmailAddress: EmailAddress.From("bob@example.com"),
                    IdentityOrigin: UserIdentityProvider.GitHub,
                    EntriesPermission: UserPermission.ReadAndModify,
                    ExportsPermission: UserPermission.None,
                    ProjectsPermission: UserPermission.None,
                    UsersPermission: UserPermission.None
                )
            )
            .WithUserWithPermissionsExistsSuccess(result: true)
            .WithSaveNewUserSuccess();

        var helper = context.BuildTarget();

        var result = await helper.Execute(
            externalId: UserExternalId.From("github-33"),
            emailAddress: EmailAddress.From("bob@example.com"),
            name: UserName.From("Bob"),
            identityProvider: UserIdentityProvider.GitHub,
            cancellationToken: CancellationToken.None
        );
        result.TrySuccess(out var successResult, out _).ShouldBeTrue();

        successResult.ShouldBeEquivalentTo(
            new GetOrRegisterNewUserUseCaseOutput(
                User: new GetUserResult(
                    Id: UserId.From(42),
                    Name: UserName.From("Bob"),
                    EmailAddress: EmailAddress.From("bob@example.com"),
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
            x.Execute(
                UserExternalId.From("github-33"),
                UserIdentityProvider.GitHub,
                It.IsAny<CancellationToken>()
            )
        );
        context.GetUserMock.Verify(x =>
            x.Execute(
                UserExternalId.From("github-33"),
                UserIdentityProvider.GitHub,
                It.IsAny<CancellationToken>()
            )
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
                UserExternalId.From("github-33"),
                EmailAddress.From("bob@example.com"),
                UserName.From("Bob"),
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
                    Id: UserId.From(42),
                    Name: UserName.From("Bob"),
                    EmailAddress: EmailAddress.From("bob@example.com"),
                    IdentityOrigin: UserIdentityProvider.GitHub,
                    EntriesPermission: UserPermission.ReadAndModify,
                    ExportsPermission: UserPermission.None,
                    ProjectsPermission: UserPermission.None,
                    UsersPermission: UserPermission.None
                )
            )
            .WithUserWithPermissionsExistsSuccess(result: false)
            .WithGetFirstAdminUserEmailAddressError(error)
            .WithSaveNewUserSuccess();

        var helper = context.BuildTarget();

        var result = await helper.Execute(
            externalId: UserExternalId.From("github-33"),
            emailAddress: EmailAddress.From("bob@example.com"),
            name: UserName.From("Bob"),
            identityProvider: UserIdentityProvider.GitHub,
            cancellationToken: CancellationToken.None
        );
        result.TrySuccess(out var successResult, out _).ShouldBeTrue();

        successResult.ShouldBeEquivalentTo(
            new GetOrRegisterNewUserUseCaseOutput(
                User: new GetUserResult(
                    Id: UserId.From(42),
                    Name: UserName.From("Bob"),
                    EmailAddress: EmailAddress.From("bob@example.com"),
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
            x.Execute(
                UserExternalId.From("github-33"),
                UserIdentityProvider.GitHub,
                It.IsAny<CancellationToken>()
            )
        );
        context.GetUserMock.Verify(x =>
            x.Execute(
                UserExternalId.From("github-33"),
                UserIdentityProvider.GitHub,
                It.IsAny<CancellationToken>()
            )
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
                UserExternalId.From("github-33"),
                EmailAddress.From("bob@example.com"),
                UserName.From("Bob"),
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
            .WithGetUserSuccess(
                new GetUserResult(
                    Id: UserId.From(42),
                    Name: UserName.From("Bob"),
                    EmailAddress: EmailAddress.From("bob@example.com"),
                    IdentityOrigin: UserIdentityProvider.GitHub,
                    EntriesPermission: UserPermission.ReadAndModify,
                    ExportsPermission: UserPermission.ReadAndModify,
                    ProjectsPermission: UserPermission.ReadAndModify,
                    UsersPermission: UserPermission.ReadAndModify
                )
            )
            .WithUpdateUserIdentitySuccess();
        var helper = context.BuildTarget();

        var result = await helper.Execute(
            externalId: UserExternalId.From("github-33"),
            emailAddress: EmailAddress.From("bob@example.com"),
            name: UserName.From("Bobby"),
            identityProvider: UserIdentityProvider.GitHub,
            cancellationToken: CancellationToken.None
        );
        result.TrySuccess(out var successResult, out _).ShouldBeTrue();

        successResult.ShouldBeEquivalentTo(
            new GetOrRegisterNewUserUseCaseOutput(
                User: new GetUserResult(
                    Id: UserId.From(42),
                    Name: UserName.From("Bobby"),
                    EmailAddress: EmailAddress.From("bob@example.com"),
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
            x.Execute(
                UserExternalId.From("github-33"),
                UserIdentityProvider.GitHub,
                It.IsAny<CancellationToken>()
            )
        );
        context.UpdateUserIdentityMock.Verify(x =>
            x.Execute(
                UserId.From(42),
                EmailAddress.From("bob@example.com"),
                UserName.From("Bobby"),
                It.IsAny<CancellationToken>()
            )
        );
        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_updates_user_if_the_email_address_changed()
    {
        var context = new GetOrRegisterNewUserUseCaseTestContext()
            .WithGetUserSuccess(
                new GetUserResult(
                    Id: UserId.From(42),
                    Name: UserName.From("Bob"),
                    EmailAddress: EmailAddress.From("bob@example.com"),
                    IdentityOrigin: UserIdentityProvider.GitHub,
                    EntriesPermission: UserPermission.ReadAndModify,
                    ExportsPermission: UserPermission.ReadAndModify,
                    ProjectsPermission: UserPermission.ReadAndModify,
                    UsersPermission: UserPermission.ReadAndModify
                )
            )
            .WithUpdateUserIdentitySuccess();
        var helper = context.BuildTarget();

        var result = await helper.Execute(
            externalId: UserExternalId.From("github-33"),
            emailAddress: EmailAddress.From("bobby@example.com"),
            name: UserName.From("Bob"),
            identityProvider: UserIdentityProvider.GitHub,
            cancellationToken: CancellationToken.None
        );
        result.TrySuccess(out var successResult, out _).ShouldBeTrue();

        successResult.ShouldBeEquivalentTo(
            new GetOrRegisterNewUserUseCaseOutput(
                User: new GetUserResult(
                    Id: UserId.From(42),
                    Name: UserName.From("Bob"),
                    EmailAddress: EmailAddress.From("bobby@example.com"),
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
            x.Execute(
                UserExternalId.From("github-33"),
                UserIdentityProvider.GitHub,
                It.IsAny<CancellationToken>()
            )
        );
        context.UpdateUserIdentityMock.Verify(x =>
            x.Execute(
                UserId.From(42),
                EmailAddress.From("bobby@example.com"),
                UserName.From("Bob"),
                It.IsAny<CancellationToken>()
            )
        );
        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_logs_user_update_failure_and_returns_non_updated_values()
    {
        var context = new GetOrRegisterNewUserUseCaseTestContext()
            .WithGetUserSuccess(
                new GetUserResult(
                    Id: UserId.From(42),
                    Name: UserName.From("Bob"),
                    EmailAddress: EmailAddress.From("bob@example.com"),
                    IdentityOrigin: UserIdentityProvider.GitHub,
                    EntriesPermission: UserPermission.ReadAndModify,
                    ExportsPermission: UserPermission.ReadAndModify,
                    ProjectsPermission: UserPermission.ReadAndModify,
                    UsersPermission: UserPermission.ReadAndModify
                )
            )
            .WithUpdateUserIdentityError(UpdateUserIdentityError.DuplicateUser);
        var helper = context.BuildTarget();

        var result = await helper.Execute(
            externalId: UserExternalId.From("github-33"),
            emailAddress: EmailAddress.From("bobby@example.com"),
            name: UserName.From("Bob"),
            identityProvider: UserIdentityProvider.GitHub,
            cancellationToken: CancellationToken.None
        );
        result.TrySuccess(out var successResult, out _).ShouldBeTrue();

        successResult.ShouldBeEquivalentTo(
            new GetOrRegisterNewUserUseCaseOutput(
                User: new GetUserResult(
                    Id: UserId.From(42),
                    Name: UserName.From("Bob"),
                    EmailAddress: EmailAddress.From("bob@example.com"),
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
            x.Execute(
                UserExternalId.From("github-33"),
                UserIdentityProvider.GitHub,
                It.IsAny<CancellationToken>()
            )
        );
        context.UpdateUserIdentityMock.Verify(x =>
            x.Execute(
                UserId.From(42),
                EmailAddress.From("bobby@example.com"),
                UserName.From("Bob"),
                It.IsAny<CancellationToken>()
            )
        );
        context.LoggerMock.VerifyWarningLog(
            expectedMessage: "Failed to update GitHub user github-33 due to DuplicateUser error"
        );
        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_returns_error_for_unknown_user_retrieval_failure()
    {
        var context = new GetOrRegisterNewUserUseCaseTestContext().WithGetUserError(
            GetUserError.Unknown
        );

        var helper = context.BuildTarget();

        var result = await helper.Execute(
            externalId: UserExternalId.From("github-33"),
            emailAddress: EmailAddress.From("bob@example.com"),
            name: UserName.From("Bob"),
            identityProvider: UserIdentityProvider.GitHub,
            cancellationToken: CancellationToken.None
        );
        result.TrySuccess(out var _, out var errorResult).ShouldBeFalse();

        errorResult.ShouldBe(GetOrRegisterNewUserUseCaseError.Unknown);

        context.GetUserMock.Verify(x =>
            x.Execute(
                UserExternalId.From("github-33"),
                UserIdentityProvider.GitHub,
                It.IsAny<CancellationToken>()
            )
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
                    Id: UserId.From(42),
                    Name: UserName.From("Bob"),
                    EmailAddress: EmailAddress.From("bob@example.com"),
                    IdentityOrigin: UserIdentityProvider.GitHub,
                    EntriesPermission: UserPermission.ReadAndModify,
                    ExportsPermission: UserPermission.ReadAndModify,
                    ProjectsPermission: UserPermission.ReadAndModify,
                    UsersPermission: UserPermission.ReadAndModify
                )
            )
            .WithUserWithPermissionsExistsSuccess(result: false)
            .WithGetFirstAdminUserEmailAddressSuccess(result: EmailAddress.From("bob@example.com"))
            .WithSaveNewUserError(error);

        var helper = context.BuildTarget();

        var result = await helper.Execute(
            externalId: UserExternalId.From("github-33"),
            emailAddress: EmailAddress.From("bob@example.com"),
            name: UserName.From("Bob"),
            identityProvider: UserIdentityProvider.GitHub,
            cancellationToken: CancellationToken.None
        );
        result.TrySuccess(out var _, out var errorResult).ShouldBeFalse();

        errorResult.ShouldBe(GetOrRegisterNewUserUseCaseError.RegistrationFailed);

        context.GetUserMock.Verify(x =>
            x.Execute(
                UserExternalId.From("github-33"),
                UserIdentityProvider.GitHub,
                It.IsAny<CancellationToken>()
            )
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
                UserExternalId.From("github-33"),
                EmailAddress.From("bob@example.com"),
                UserName.From("Bob"),
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
            .WithUserWithPermissionsExistsSuccess(result: false)
            .WithGetFirstAdminUserEmailAddressSuccess(result: EmailAddress.From("bob@example.com"))
            .WithSaveNewUserSuccess();

        var helper = context.BuildTarget();

        var result = await helper.Execute(
            externalId: UserExternalId.From("github-33"),
            emailAddress: EmailAddress.From("bob@example.com"),
            name: UserName.From("Bob"),
            identityProvider: UserIdentityProvider.GitHub,
            cancellationToken: CancellationToken.None
        );
        result.TrySuccess(out var _, out var errorResult).ShouldBeFalse();

        errorResult.ShouldBe(GetOrRegisterNewUserUseCaseError.Unknown);

        context.GetUserMock.Verify(x =>
            x.Execute(
                UserExternalId.From("github-33"),
                UserIdentityProvider.GitHub,
                It.IsAny<CancellationToken>()
            )
        );
        context.GetUserMock.Verify(x =>
            x.Execute(
                UserExternalId.From("github-33"),
                UserIdentityProvider.GitHub,
                It.IsAny<CancellationToken>()
            )
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
                UserExternalId.From("github-33"),
                EmailAddress.From("bob@example.com"),
                UserName.From("Bob"),
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
                    Id: UserId.From(42),
                    Name: UserName.From("Bob"),
                    EmailAddress: EmailAddress.From("bob@example.com"),
                    IdentityOrigin: UserIdentityProvider.GitHub,
                    EntriesPermission: UserPermission.ReadAndModify,
                    ExportsPermission: UserPermission.ReadAndModify,
                    ProjectsPermission: UserPermission.ReadAndModify,
                    UsersPermission: UserPermission.ReadAndModify
                )
            )
            .WithUserWithPermissionsExistsError(UserWithPermissionsExistsError.Unknown);

        var helper = context.BuildTarget();

        var result = await helper.Execute(
            externalId: UserExternalId.From("github-33"),
            emailAddress: EmailAddress.From("bob@example.com"),
            name: UserName.From("Bob"),
            identityProvider: UserIdentityProvider.GitHub,
            cancellationToken: CancellationToken.None
        );
        result.TrySuccess(out var _, out var errorResult).ShouldBeFalse();

        errorResult.ShouldBe(GetOrRegisterNewUserUseCaseError.RegistrationFailed);

        context.GetUserMock.Verify(x =>
            x.Execute(
                UserExternalId.From("github-33"),
                UserIdentityProvider.GitHub,
                It.IsAny<CancellationToken>()
            )
        );
        context.GetUserMock.Verify(x =>
            x.Execute(
                UserExternalId.From("github-33"),
                UserIdentityProvider.GitHub,
                It.IsAny<CancellationToken>()
            )
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

[GenerateTestContext(target: typeof(GetOrRegisterNewUserUseCase), GenerateWithMethods = true)]
internal partial class GetOrRegisterNewUserUseCaseTestContext
{
    public GetOrRegisterNewUserUseCaseTestContext WithFirstGetUserErrorAndSecondWithResult(
        GetUserError error,
        GetUserResult result
    )
    {
        GetUserMock
            .SetupSequence(x =>
                x.Execute(
                    It.IsAny<UserExternalId>(),
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
                    It.IsAny<UserExternalId>(),
                    It.IsAny<UserIdentityProvider>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Failure<GetUserResult, GetUserError>(firstError))
            .ReturnsAsync(Result.Failure<GetUserResult, GetUserError>(secondError));

        return this;
    }
}
