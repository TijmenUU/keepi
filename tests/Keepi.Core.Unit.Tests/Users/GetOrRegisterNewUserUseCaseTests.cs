using Keepi.Core.Users;
using Microsoft.Extensions.Logging;

namespace Keepi.Core.Unit.Tests.Users;

public class GetOrRegisterNewUserUseCaseTests
{
    [Fact]
    public async Task Execute_returns_user_if_it_already_exists()
    {
        var context = new TestContext().WithFirstGetUserResult(
            new GetUserResult(
                Id: 42,
                Name: "Bob",
                EmailAddress: "bob@example.com",
                IdentityOrigin: UserIdentityProvider.GitHub
            )
        );
        var helper = context.BuildUseCase();

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
                    IdentityOrigin: UserIdentityProvider.GitHub
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
    public async Task Execute_registers_user_if_it_does_not_yet_exist()
    {
        var context = new TestContext()
            .WithFirstGetUserErrorAndSecondWithResult(
                error: GetUserError.DoesNotExist,
                result: new GetUserResult(
                    Id: 42,
                    Name: "Bob",
                    EmailAddress: "bob@example.com",
                    IdentityOrigin: UserIdentityProvider.GitHub
                )
            )
            .WithRegisterUserResult(RegisterUserUseCaseResult.UserCreated);

        var helper = context.BuildUseCase();

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
                    IdentityOrigin: UserIdentityProvider.GitHub
                ),
                NewlyRegistered: false
            )
        );

        context.GetUserMock.Verify(x =>
            x.Execute("github-33", UserIdentityProvider.GitHub, It.IsAny<CancellationToken>())
        );
        context.GetUserMock.Verify(x =>
            x.Execute("github-33", UserIdentityProvider.GitHub, It.IsAny<CancellationToken>())
        );
        context.RegisterUserUseCaseMock.Verify(x =>
            x.Execute(
                "github-33",
                "bob@example.com",
                "Bob",
                UserIdentityProvider.GitHub,
                It.IsAny<CancellationToken>()
            )
        );
        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_updates_user_if_the_name_changed()
    {
        var context = new TestContext()
            .WithFirstGetUserResult(
                new GetUserResult(
                    Id: 42,
                    Name: "Bob",
                    EmailAddress: "bob@example.com",
                    IdentityOrigin: UserIdentityProvider.GitHub
                )
            )
            .WithUserUpdateSuccess();
        var helper = context.BuildUseCase();

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
                    IdentityOrigin: UserIdentityProvider.GitHub
                ),
                NewlyRegistered: false
            )
        );

        context.GetUserMock.Verify(x =>
            x.Execute("github-33", UserIdentityProvider.GitHub, It.IsAny<CancellationToken>())
        );
        context.UpdateUserMock.Verify(x =>
            x.Execute(42, "bob@example.com", "Bobby", It.IsAny<CancellationToken>())
        );
        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_updates_user_if_the_email_address_changed()
    {
        var context = new TestContext()
            .WithFirstGetUserResult(
                new GetUserResult(
                    Id: 42,
                    Name: "Bob",
                    EmailAddress: "bob@example.com",
                    IdentityOrigin: UserIdentityProvider.GitHub
                )
            )
            .WithUserUpdateSuccess();
        var helper = context.BuildUseCase();

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
                    IdentityOrigin: UserIdentityProvider.GitHub
                ),
                NewlyRegistered: false
            )
        );

        context.GetUserMock.Verify(x =>
            x.Execute("github-33", UserIdentityProvider.GitHub, It.IsAny<CancellationToken>())
        );
        context.UpdateUserMock.Verify(x =>
            x.Execute(42, "bobby@example.com", "Bob", It.IsAny<CancellationToken>())
        );
        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_logs_user_update_failure_and_returns_non_updated_values()
    {
        var context = new TestContext()
            .WithFirstGetUserResult(
                new GetUserResult(
                    Id: 42,
                    Name: "Bob",
                    EmailAddress: "bob@example.com",
                    IdentityOrigin: UserIdentityProvider.GitHub
                )
            )
            .WithUserUpdateFailure();
        var helper = context.BuildUseCase();

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
                    IdentityOrigin: UserIdentityProvider.GitHub
                ),
                NewlyRegistered: false
            )
        );

        context.GetUserMock.Verify(x =>
            x.Execute("github-33", UserIdentityProvider.GitHub, It.IsAny<CancellationToken>())
        );
        context.UpdateUserMock.Verify(x =>
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
        var context = new TestContext().WithFirstGetUserError(GetUserError.Unknown);

        var helper = context.BuildUseCase();

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
    [InlineData(RegisterUserUseCaseResult.Unknown)]
    [InlineData(RegisterUserUseCaseResult.UserAlreadyExists)]
    public async Task Execute_returns_error_for_user_registration_failure(
        RegisterUserUseCaseResult registrationResult
    )
    {
        var context = new TestContext()
            .WithFirstGetUserErrorAndSecondWithResult(
                error: GetUserError.DoesNotExist,
                result: new GetUserResult(
                    Id: 42,
                    Name: "Bob",
                    EmailAddress: "bob@example.com",
                    IdentityOrigin: UserIdentityProvider.GitHub
                )
            )
            .WithRegisterUserResult(registrationResult);

        var helper = context.BuildUseCase();

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
        context.RegisterUserUseCaseMock.Verify(x =>
            x.Execute(
                "github-33",
                "bob@example.com",
                "Bob",
                UserIdentityProvider.GitHub,
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
        var context = new TestContext()
            .WithFirstGetUserErrorAndSecondError(
                firstError: GetUserError.DoesNotExist,
                secondError: secondGetUserErrror
            )
            .WithRegisterUserResult(RegisterUserUseCaseResult.UserCreated);

        var helper = context.BuildUseCase();

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
        context.RegisterUserUseCaseMock.Verify(x =>
            x.Execute(
                "github-33",
                "bob@example.com",
                "Bob",
                UserIdentityProvider.GitHub,
                It.IsAny<CancellationToken>()
            )
        );
        context.VerifyNoOtherCalls();
    }

    private class TestContext
    {
        public Mock<IGetUser> GetUserMock { get; } = new(MockBehavior.Strict);
        public Mock<IUpdateUser> UpdateUserMock { get; } = new(MockBehavior.Strict);
        public Mock<IRegisterUserUseCase> RegisterUserUseCaseMock { get; } =
            new(MockBehavior.Strict);
        public Mock<ILogger<GetOrRegisterNewUserUseCase>> LoggerMock { get; } =
            new(MockBehavior.Loose);

        public TestContext WithFirstGetUserResult(GetUserResult result)
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

        public TestContext WithFirstGetUserError(GetUserError error)
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

        public TestContext WithFirstGetUserErrorAndSecondWithResult(
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

        public TestContext WithFirstGetUserErrorAndSecondError(
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

        public TestContext WithUserUpdateSuccess() =>
            WithUserUpdateResult(Result.Success<UpdateUserError>());

        public TestContext WithUserUpdateFailure() =>
            WithUserUpdateResult(Result.Failure(UpdateUserError.DuplicateUser));

        public TestContext WithUserUpdateResult(IMaybeErrorResult<UpdateUserError> result)
        {
            UpdateUserMock
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

        public TestContext WithRegisterUserResult(RegisterUserUseCaseResult result)
        {
            RegisterUserUseCaseMock
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

        public GetOrRegisterNewUserUseCase BuildUseCase() =>
            new(
                getUser: GetUserMock.Object,
                updateUser: UpdateUserMock.Object,
                registerUserUseCase: RegisterUserUseCaseMock.Object,
                logger: LoggerMock.Object
            );

        public void VerifyNoOtherCalls()
        {
            GetUserMock.VerifyNoOtherCalls();
            UpdateUserMock.VerifyNoOtherCalls();
            RegisterUserUseCaseMock.VerifyNoOtherCalls();
        }
    }
}
