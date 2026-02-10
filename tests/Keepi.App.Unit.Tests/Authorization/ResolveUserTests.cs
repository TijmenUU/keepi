using Keepi.App.Authorization;
using Keepi.Core;
using Keepi.Core.Users;

namespace Keepi.App.Unit.Tests.Authorization;

public class ResolveUserTests
{
    [Fact]
    public async Task Execute_returns_existing_user()
    {
        var context = new TestContext()
            .WithGetUserNameResult(result: "Barry")
            .WithExistingUser(
                id: 42,
                name: "Barry",
                emailAddress: "user@localhost",
                identityProvider: UserIdentityProvider.LocalApplication,
                entriesPermission: UserPermission.ReadAndModify,
                exportsPermission: UserPermission.ReadAndModify,
                projectsPermission: UserPermission.ReadAndModify,
                usersPermission: UserPermission.ReadAndModify
            );

        var result = await context.BuildHelper().Execute(CancellationToken.None);
        result.TrySuccess(out var successResult, out _).ShouldBeTrue();

        successResult.ShouldBeEquivalentTo(
            new ResolvedUser(
                Id: 42,
                Name: "Barry",
                EmailAddress: "user@localhost",
                EntriesPermission: UserPermission.ReadAndModify,
                ExportsPermission: UserPermission.ReadAndModify,
                ProjectsPermission: UserPermission.ReadAndModify,
                UsersPermission: UserPermission.ReadAndModify
            )
        );

        context.GetUserNameMock.Verify(x => x.Execute());
        context.GetOrRegisterNewUserUseCaseMock.Verify(x =>
            x.Execute(
                "Barry",
                "user@localhost",
                "Barry",
                UserIdentityProvider.LocalApplication,
                It.IsAny<CancellationToken>()
            )
        );
        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_returns_newly_registered_user()
    {
        var context = new TestContext()
            .WithGetUserNameResult(result: "Barry")
            .WithNewlyRegisteredUser(
                id: 42,
                name: "Barry",
                emailAddress: "user@localhost",
                identityProvider: UserIdentityProvider.LocalApplication,
                entriesPermission: UserPermission.ReadAndModify,
                exportsPermission: UserPermission.ReadAndModify,
                projectsPermission: UserPermission.ReadAndModify,
                usersPermission: UserPermission.ReadAndModify
            );

        var result = await context.BuildHelper().Execute(CancellationToken.None);
        result.TrySuccess(out var successResult, out _).ShouldBeTrue();

        successResult.ShouldBeEquivalentTo(
            new ResolvedUser(
                Id: 42,
                Name: "Barry",
                EmailAddress: "user@localhost",
                EntriesPermission: UserPermission.ReadAndModify,
                ExportsPermission: UserPermission.ReadAndModify,
                ProjectsPermission: UserPermission.ReadAndModify,
                UsersPermission: UserPermission.ReadAndModify
            )
        );

        context.GetUserNameMock.Verify(x => x.Execute());
        context.GetOrRegisterNewUserUseCaseMock.Verify(x =>
            x.Execute(
                "Barry",
                "user@localhost",
                "Barry",
                UserIdentityProvider.LocalApplication,
                It.IsAny<CancellationToken>()
            )
        );
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
    public async Task Execute_returns_correctly_mapped_user_permissions(
        UserPermission entriesPermission,
        UserPermission exportsPermission,
        UserPermission projectsPermission,
        UserPermission usersPermission
    )
    {
        var context = new TestContext()
            .WithGetUserNameResult(result: "Barry")
            .WithExistingUser(
                id: 42,
                name: "Barry",
                emailAddress: "user@localhost",
                identityProvider: UserIdentityProvider.LocalApplication,
                entriesPermission: entriesPermission,
                exportsPermission: exportsPermission,
                projectsPermission: projectsPermission,
                usersPermission: usersPermission
            );

        var result = await context.BuildHelper().Execute(cancellationToken: CancellationToken.None);
        result.TrySuccess(out var successResult, out _).ShouldBeTrue();

        successResult.ShouldBeEquivalentTo(
            new ResolvedUser(
                Id: 42,
                Name: "Barry",
                EmailAddress: "user@localhost",
                EntriesPermission: entriesPermission,
                ExportsPermission: exportsPermission,
                ProjectsPermission: projectsPermission,
                UsersPermission: usersPermission
            )
        );

        context.GetUserNameMock.Verify(x => x.Execute());
        context.GetOrRegisterNewUserUseCaseMock.Verify(x =>
            x.Execute(
                "Barry",
                "user@localhost",
                "Barry",
                UserIdentityProvider.LocalApplication,
                It.IsAny<CancellationToken>()
            )
        );
        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_returns_error_for_user_with_empty_string_as_name()
    {
        var context = new TestContext().WithGetUserNameResult(result: "");

        var result = await context.BuildHelper().Execute(CancellationToken.None);
        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();

        errorResult.ShouldBe(ResolveUserError.UserNotAuthenticated);

        context.GetUserNameMock.Verify(x => x.Execute());
        context.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(
        GetOrRegisterNewUserUseCaseError.RegistrationFailed,
        ResolveUserError.UserRegistrationFailed
    )]
    [InlineData(GetOrRegisterNewUserUseCaseError.Unknown, ResolveUserError.Unknown)]
    public async Task Execute_returns_error_for_get_or_register_use_case_failures(
        GetOrRegisterNewUserUseCaseError useCaseError,
        ResolveUserError expectedError
    )
    {
        var context = new TestContext()
            .WithGetUserNameResult(result: "Barry")
            .WithGetOrRegisterNewUserUseCaseError(useCaseError);

        var result = await context.BuildHelper().Execute(CancellationToken.None);
        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();

        errorResult.ShouldBe(expectedError);

        context.GetUserNameMock.Verify(x => x.Execute());
        context.GetOrRegisterNewUserUseCaseMock.Verify(x =>
            x.Execute(
                "Barry",
                "user@localhost",
                "Barry",
                UserIdentityProvider.LocalApplication,
                It.IsAny<CancellationToken>()
            )
        );
        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_returns_error_if_user_to_return_is_not_local()
    {
        var context = new TestContext()
            .WithGetUserNameResult(result: "Barry")
            .WithExistingUser(
                id: 42,
                name: "Barry",
                emailAddress: "user@localhost",
                identityProvider: UserIdentityProvider.GitHub,
                entriesPermission: UserPermission.ReadAndModify,
                exportsPermission: UserPermission.ReadAndModify,
                projectsPermission: UserPermission.ReadAndModify,
                usersPermission: UserPermission.ReadAndModify
            );

        var result = await context.BuildHelper().Execute(CancellationToken.None);
        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();

        errorResult.ShouldBe(ResolveUserError.UnexpectedNonLocalApplicationUser);

        context.GetUserNameMock.Verify(x => x.Execute());
        context.GetOrRegisterNewUserUseCaseMock.Verify(x =>
            x.Execute(
                "Barry",
                "user@localhost",
                "Barry",
                UserIdentityProvider.LocalApplication,
                It.IsAny<CancellationToken>()
            )
        );
        context.VerifyNoOtherCalls();
    }

    private class TestContext
    {
        public Mock<IGetUserName> GetUserNameMock { get; } = new(MockBehavior.Strict);
        public Mock<IGetOrRegisterNewUserUseCase> GetOrRegisterNewUserUseCaseMock { get; } =
            new(MockBehavior.Strict);

        public TestContext WithGetUserNameResult(string result)
        {
            GetUserNameMock.Setup(x => x.Execute()).Returns(result);

            return this;
        }

        public TestContext WithExistingUser(
            int id,
            string name,
            string emailAddress,
            UserIdentityProvider identityProvider,
            UserPermission entriesPermission,
            UserPermission exportsPermission,
            UserPermission projectsPermission,
            UserPermission usersPermission
        )
        {
            return WithGetOrRegisterNewUserUseCaseSuccess(
                id: id,
                name: name,
                emailAddress: emailAddress,
                identityProvider: identityProvider,
                entriesPermission: entriesPermission,
                exportsPermission: exportsPermission,
                projectsPermission: projectsPermission,
                usersPermission: usersPermission,
                newlyRegistered: false
            );
        }

        public TestContext WithNewlyRegisteredUser(
            int id,
            string name,
            string emailAddress,
            UserIdentityProvider identityProvider,
            UserPermission entriesPermission,
            UserPermission exportsPermission,
            UserPermission projectsPermission,
            UserPermission usersPermission
        )
        {
            return WithGetOrRegisterNewUserUseCaseSuccess(
                id: id,
                name: name,
                emailAddress: emailAddress,
                identityProvider: identityProvider,
                entriesPermission: entriesPermission,
                exportsPermission: exportsPermission,
                projectsPermission: projectsPermission,
                usersPermission: usersPermission,
                newlyRegistered: true
            );
        }

        private TestContext WithGetOrRegisterNewUserUseCaseSuccess(
            int id,
            string name,
            string emailAddress,
            UserIdentityProvider identityProvider,
            UserPermission entriesPermission,
            UserPermission exportsPermission,
            UserPermission projectsPermission,
            UserPermission usersPermission,
            bool newlyRegistered
        )
        {
            GetOrRegisterNewUserUseCaseMock
                .Setup(x =>
                    x.Execute(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<UserIdentityProvider>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(
                    Result.Success<
                        GetOrRegisterNewUserUseCaseOutput,
                        GetOrRegisterNewUserUseCaseError
                    >(
                        new GetOrRegisterNewUserUseCaseOutput(
                            User: new(
                                Id: id,
                                Name: name,
                                EmailAddress: emailAddress,
                                IdentityOrigin: identityProvider,
                                EntriesPermission: entriesPermission,
                                ExportsPermission: exportsPermission,
                                ProjectsPermission: projectsPermission,
                                UsersPermission: usersPermission
                            ),
                            NewlyRegistered: newlyRegistered
                        )
                    )
                );

            return this;
        }

        public TestContext WithGetOrRegisterNewUserUseCaseError(
            GetOrRegisterNewUserUseCaseError error
        ) =>
            WithGetOrRegisterNewUserUseCaseResult(
                Result.Failure<GetOrRegisterNewUserUseCaseOutput, GetOrRegisterNewUserUseCaseError>(
                    error
                )
            );

        private TestContext WithGetOrRegisterNewUserUseCaseResult(
            IValueOrErrorResult<
                GetOrRegisterNewUserUseCaseOutput,
                GetOrRegisterNewUserUseCaseError
            > result
        )
        {
            GetOrRegisterNewUserUseCaseMock
                .Setup(x =>
                    x.Execute(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<UserIdentityProvider>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(result);

            return this;
        }

        public ResolveUser BuildHelper() =>
            new(
                getUserName: GetUserNameMock.Object,
                getOrRegisterNewUserUseCase: GetOrRegisterNewUserUseCaseMock.Object
            );

        public void VerifyNoOtherCalls()
        {
            GetUserNameMock.VerifyNoOtherCalls();
            GetOrRegisterNewUserUseCaseMock.VerifyNoOtherCalls();
        }
    }
}
