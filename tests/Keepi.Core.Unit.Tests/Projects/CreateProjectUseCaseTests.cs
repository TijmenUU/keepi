using Keepi.Core.Projects;
using Keepi.Core.Users;
using Microsoft.Extensions.Logging;

namespace Keepi.Core.Unit.Tests.Projects;

public class CreateProjectUseCaseTests
{
    [Fact]
    public async Task Execute_returns_created_project_ID()
    {
        var context = new TestContext()
            .WithResolvedUser(user: ResolvedUserBuilder.CreateAdministratorBob())
            .WithSaveNewProjectSuccessResult(1);

        var result = await context
            .BuildUseCase()
            .Execute(
                name: "Algemeen",
                enabled: true,
                userIds: [42, 43],
                invoiceItemNames: ["Dev", "Planning"],
                cancellationToken: CancellationToken.None
            );

        result.TrySuccess(out var successResult, out _).ShouldBeTrue();
        successResult.ShouldBe(1);

        context.ResolveUserMock.Verify(x => x.Execute(It.IsAny<CancellationToken>()));
        context.SaveNewProjectMock.Verify(x =>
            x.Execute(
                "Algemeen",
                true,
                It.Is<int[]>(u => u.Length == 2 && u[0] == 42 && u[1] == 43),
                It.Is<string[]>(i => i.Length == 2 && i[0] == "Dev" && i[1] == "Planning"),
                It.IsAny<CancellationToken>()
            )
        );
        context.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("12345678901234567890123456789012345678901234567890123456789012345")]
    public async Task Execute_returns_error_for_invalid_project_name(string projectName)
    {
        var context = new TestContext().WithResolvedUser(
            user: ResolvedUserBuilder.CreateAdministratorBob()
        );

        var result = await context
            .BuildUseCase()
            .Execute(
                name: projectName,
                enabled: true,
                userIds: [42, 43],
                invoiceItemNames: ["Dev", "Planning"],
                cancellationToken: CancellationToken.None
            );

        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(CreateProjectUseCaseError.InvalidProjectName);
    }

    [Fact]
    public async Task Execute_returns_error_for_duplicate_user_ids()
    {
        var context = new TestContext().WithResolvedUser(
            user: ResolvedUserBuilder.CreateAdministratorBob()
        );

        var result = await context
            .BuildUseCase()
            .Execute(
                name: "Algemeen",
                enabled: true,
                userIds: [42, 42],
                invoiceItemNames: ["Dev", "Planning"],
                cancellationToken: CancellationToken.None
            );

        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(CreateProjectUseCaseError.DuplicateUserIds);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("12345678901234567890123456789012345678901234567890123456789012345")]
    public async Task Execute_returns_error_for_invalid_invoice_item_name(string invoiceItemName)
    {
        var context = new TestContext().WithResolvedUser(
            user: ResolvedUserBuilder.CreateAdministratorBob()
        );

        var result = await context
            .BuildUseCase()
            .Execute(
                name: "Algemeen",
                enabled: true,
                userIds: [42, 43],
                invoiceItemNames: ["Dev", invoiceItemName],
                cancellationToken: CancellationToken.None
            );

        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(CreateProjectUseCaseError.InvalidInvoiceItemName);
    }

    [Fact]
    public async Task Execute_returns_error_for_duplicate_invoice_item_names()
    {
        var context = new TestContext().WithResolvedUser(
            user: ResolvedUserBuilder.CreateAdministratorBob()
        );

        var result = await context
            .BuildUseCase()
            .Execute(
                name: "Algemeen",
                enabled: true,
                userIds: [42, 43],
                invoiceItemNames: ["Dev", "Dev"],
                cancellationToken: CancellationToken.None
            );

        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(CreateProjectUseCaseError.DuplicateInvoiceItemNames);
    }

    [Theory]
    [InlineData(
        SaveNewProjectError.DuplicateProjectName,
        CreateProjectUseCaseError.DuplicateProjectName
    )]
    [InlineData(SaveNewProjectError.UnknownUserId, CreateProjectUseCaseError.UnknownUserId)]
    [InlineData(SaveNewProjectError.Unknown, CreateProjectUseCaseError.Unknown)]
    public async Task Execute_returns_save_error(
        SaveNewProjectError saveError,
        CreateProjectUseCaseError expectedError
    )
    {
        var context = new TestContext()
            .WithResolvedUser(user: ResolvedUserBuilder.CreateAdministratorBob())
            .WithSaveNewProjectFailureResult(saveError);

        var result = await context
            .BuildUseCase()
            .Execute(
                name: "Algemeen",
                enabled: true,
                userIds: [42, 43],
                invoiceItemNames: ["Dev", "Planning"],
                cancellationToken: CancellationToken.None
            );

        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(expectedError);
    }

    [Theory]
    [InlineData(
        ResolveUserError.UserNotAuthenticated,
        CreateProjectUseCaseError.UnauthenticatedUser
    )]
    [InlineData(ResolveUserError.MalformedUserClaims, CreateProjectUseCaseError.Unknown)]
    [InlineData(ResolveUserError.UnsupportedIdentityProvider, CreateProjectUseCaseError.Unknown)]
    [InlineData(ResolveUserError.UserNotFound, CreateProjectUseCaseError.Unknown)]
    [InlineData(ResolveUserError.UserRegistrationFailed, CreateProjectUseCaseError.Unknown)]
    public async Task Execute_returns_error_for_user_resolve_error(
        ResolveUserError resolveUserError,
        CreateProjectUseCaseError expectedError
    )
    {
        var context = new TestContext().WithResolveUserError(resolveUserError);

        var result = await context
            .BuildUseCase()
            .Execute(
                name: "Algemeen",
                enabled: true,
                userIds: [42, 43],
                invoiceItemNames: ["Dev", "Planning"],
                cancellationToken: CancellationToken.None
            );

        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(expectedError);
    }

    private class TestContext
    {
        public Mock<IResolveUser> ResolveUserMock { get; } = new(MockBehavior.Strict);
        public Mock<ISaveNewProject> SaveNewProjectMock { get; } = new(MockBehavior.Strict);
        public Mock<ILogger<CreateProjectUseCase>> LoggerMock { get; } = new(MockBehavior.Loose);

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

        public TestContext WithSaveNewProjectSuccessResult(int result)
        {
            SaveNewProjectMock
                .Setup(x =>
                    x.Execute(
                        It.IsAny<string>(),
                        It.IsAny<bool>(),
                        It.IsAny<int[]>(),
                        It.IsAny<string[]>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(Result.Success<int, SaveNewProjectError>(result));

            return this;
        }

        public TestContext WithSaveNewProjectFailureResult(SaveNewProjectError result)
        {
            SaveNewProjectMock
                .Setup(x =>
                    x.Execute(
                        It.IsAny<string>(),
                        It.IsAny<bool>(),
                        It.IsAny<int[]>(),
                        It.IsAny<string[]>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(Result.Failure<int, SaveNewProjectError>(result));

            return this;
        }

        public CreateProjectUseCase BuildUseCase() =>
            new(
                resolveUser: ResolveUserMock.Object,
                saveNewProject: SaveNewProjectMock.Object,
                logger: LoggerMock.Object
            );

        public void VerifyNoOtherCalls()
        {
            ResolveUserMock.VerifyNoOtherCalls();
            SaveNewProjectMock.VerifyNoOtherCalls();
        }
    }
}
