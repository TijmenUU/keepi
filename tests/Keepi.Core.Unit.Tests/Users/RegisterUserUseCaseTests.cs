using Keepi.Core.Users;
using Microsoft.Extensions.Logging;

namespace Keepi.Core.Unit.Tests.Users;

public class RegisterUserUseCaseTests
{
    [Fact]
    public async Task Execute_creates_user_for_unknown_user_identity()
    {
        var context = new TestContext()
            .WithExistingUserSuccessResult(
                externalId: "external ID",
                emailAddress: "test@example.com",
                result: false
            )
            .WithSuccesfulStoreNewUserResult(
                externalId: "external ID",
                emailAddress: "test@example.com",
                name: "Piet Hein",
                identityProvider: UserIdentityProvider.GitHub
            );

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            externalId: "external ID",
            emailAddress: "test@example.com",
            name: "Piet Hein",
            provider: UserIdentityProvider.GitHub,
            cancellationToken: CancellationToken.None
        );

        result.ShouldBe(RegisterUserUseCaseResult.UserCreated);

        context.SaveNewUserMock.Verify(x =>
            x.Execute(
                "external ID",
                "test@example.com",
                "Piet Hein",
                UserIdentityProvider.GitHub,
                It.IsAny<CancellationToken>()
            )
        );
    }

    [Fact]
    public async Task Execute_does_not_create_user_for_already_known_user_identity()
    {
        var context = new TestContext()
            .WithExistingUserSuccessResult(
                externalId: "external ID",
                emailAddress: "test@example.com",
                result: true
            )
            .WithSuccesfulStoreNewUserResult(
                externalId: "external ID",
                emailAddress: "test@example.com",
                name: "Piet Hein",
                identityProvider: UserIdentityProvider.GitHub
            );

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            externalId: "external ID",
            emailAddress: "test@example.com",
            name: "Piet Hein",
            provider: UserIdentityProvider.GitHub,
            cancellationToken: CancellationToken.None
        );

        result.ShouldBe(RegisterUserUseCaseResult.UserAlreadyExists);

        context.SaveNewUserMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_returns_error_on_duplicate_user_error()
    {
        var context = new TestContext()
            .WithExistingUserSuccessResult(
                externalId: "external ID",
                emailAddress: "test@example.com",
                result: false
            )
            .WithErrorStoreNewUserResult(error: SaveNewUserError.DuplicateUser);

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            externalId: "external ID",
            emailAddress: "test@example.com",
            name: "Piet Hein",
            provider: UserIdentityProvider.GitHub,
            cancellationToken: CancellationToken.None
        );

        result.ShouldBe(RegisterUserUseCaseResult.UserAlreadyExists);

        context.SaveNewUserMock.Verify(x =>
            x.Execute(
                "external ID",
                "test@example.com",
                "Piet Hein",
                UserIdentityProvider.GitHub,
                It.IsAny<CancellationToken>()
            )
        );
    }

    [Fact]
    public async Task Execute_returns_error_on_unknown_get_existing_user_error()
    {
        var context = new TestContext().WithExistingUserFailureResult(
            externalId: "external ID",
            emailAddress: "test@example.com",
            result: GetUserExistsError.Unknown
        );

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            externalId: "external ID",
            emailAddress: "test@example.com",
            name: "Piet Hein",
            provider: UserIdentityProvider.GitHub,
            cancellationToken: CancellationToken.None
        );

        result.ShouldBe(RegisterUserUseCaseResult.Unknown);
    }

    [Fact]
    public async Task Execute_returns_error_on_unknown_store_new_user_error()
    {
        var context = new TestContext()
            .WithExistingUserSuccessResult(
                externalId: "external ID",
                emailAddress: "test@example.com",
                result: false
            )
            .WithErrorStoreNewUserResult(error: SaveNewUserError.Unknown);

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            externalId: "external ID",
            emailAddress: "test@example.com",
            name: "Piet Hein",
            provider: UserIdentityProvider.GitHub,
            cancellationToken: CancellationToken.None
        );

        result.ShouldBe(RegisterUserUseCaseResult.Unknown);

        context.SaveNewUserMock.Verify(x =>
            x.Execute(
                "external ID",
                "test@example.com",
                "Piet Hein",
                UserIdentityProvider.GitHub,
                It.IsAny<CancellationToken>()
            )
        );
    }

    class TestContext
    {
        public Mock<IGetUserExists> GetUserExistsMock { get; } = new(MockBehavior.Strict);
        public Mock<ISaveNewUser> SaveNewUserMock { get; } = new(MockBehavior.Strict);
        public Mock<ILogger<RegisterUserUseCase>> LoggerMock { get; } = new(MockBehavior.Loose);

        public TestContext WithExistingUserSuccessResult(
            string externalId,
            string emailAddress,
            bool result
        )
        {
            GetUserExistsMock
                .Setup(x =>
                    x.Execute(
                        externalId,
                        UserIdentityProvider.GitHub,
                        emailAddress,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(Result.Success<bool, GetUserExistsError>(result));

            return this;
        }

        public TestContext WithExistingUserFailureResult(
            string externalId,
            string emailAddress,
            GetUserExistsError result
        )
        {
            GetUserExistsMock
                .Setup(x =>
                    x.Execute(
                        externalId,
                        UserIdentityProvider.GitHub,
                        emailAddress,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(Result.Failure<bool, GetUserExistsError>(result));

            return this;
        }

        public TestContext WithSuccesfulStoreNewUserResult(
            string externalId,
            string emailAddress,
            string name,
            UserIdentityProvider identityProvider
        )
        {
            SaveNewUserMock
                .Setup(x =>
                    x.Execute(
                        externalId,
                        emailAddress,
                        name,
                        identityProvider,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(Result.Success<SaveNewUserError>());

            return this;
        }

        public TestContext WithErrorStoreNewUserResult(SaveNewUserError error)
        {
            SaveNewUserMock
                .Setup(x =>
                    x.Execute(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<UserIdentityProvider>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(Result.Failure(error));

            return this;
        }

        public RegisterUserUseCase BuildUseCase() =>
            new(
                getUserExists: GetUserExistsMock.Object,
                saveNewUser: SaveNewUserMock.Object,
                logger: LoggerMock.Object
            );
    }
}
