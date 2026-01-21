using Keepi.Core.Projects;
using Keepi.Core.Unit.Tests.Builders;
using Keepi.Core.Users;
using Microsoft.Extensions.Logging;

namespace Keepi.Core.Unit.Tests.Projects;

public class UpdateProjectUseCaseTests
{
    [Fact]
    public async Task Execute_returns_created_project_ID()
    {
        var context = new TestContext()
            .WithResolvedUser(user: ResolvedUserBuilder.CreateAdministratorBob())
            .WithUpdateProjectSuccessResult();

        var result = await context
            .BuildUseCase()
            .Execute(
                id: 1,
                name: "Algemeen",
                enabled: true,
                userIds: [42, 43],
                invoiceItems: [(10, "Dev"), (null, "Planning")],
                cancellationToken: CancellationToken.None
            );

        result.TrySuccess(out _).ShouldBeTrue();

        context.ResolveUserMock.Verify(x => x.Execute(It.IsAny<CancellationToken>()));
        context.UpdateProjectMock.Verify(x =>
            x.Execute(
                1,
                "Algemeen",
                true,
                It.Is<int[]>(u => u.Length == 2 && u[0] == 42 && u[1] == 43),
                It.Is<(int?, string)[]>(i =>
                    i.Length == 2
                    && i[0].Item1 == 10
                    && i[0].Item2 == "Dev"
                    && i[1].Item1 == null
                    && i[1].Item2 == "Planning"
                ),
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
                id: 1,
                name: projectName,
                enabled: true,
                userIds: [42, 43],
                invoiceItems: [(10, "Dev"), (null, "Planning")],
                cancellationToken: CancellationToken.None
            );

        result.TrySuccess(out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(UpdateProjectUseCaseError.InvalidProjectName);
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
                id: 1,
                name: "Algemeen",
                enabled: true,
                userIds: [42, 42],
                invoiceItems: [(10, "Dev"), (null, "Planning")],
                cancellationToken: CancellationToken.None
            );

        result.TrySuccess(out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(UpdateProjectUseCaseError.DuplicateUserIds);
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
                id: 1,
                name: "Algemeen",
                enabled: true,
                userIds: [42, 43],
                invoiceItems: [(10, "Dev"), (null, invoiceItemName)],
                cancellationToken: CancellationToken.None
            );

        result.TrySuccess(out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(UpdateProjectUseCaseError.InvalidInvoiceItemName);
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
                id: 1,
                name: "Algemeen",
                enabled: true,
                userIds: [42, 43],
                invoiceItems: [(10, "Dev"), (null, "Dev")],
                cancellationToken: CancellationToken.None
            );

        result.TrySuccess(out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(UpdateProjectUseCaseError.DuplicateInvoiceItemNames);
    }

    [Theory]
    [InlineData(
        UpdateProjectError.DuplicateProjectName,
        UpdateProjectUseCaseError.DuplicateProjectName
    )]
    [InlineData(UpdateProjectError.UnknownProjectId, UpdateProjectUseCaseError.UnknownProjectId)]
    [InlineData(UpdateProjectError.UnknownUserId, UpdateProjectUseCaseError.UnknownUserId)]
    [InlineData(UpdateProjectError.Unknown, UpdateProjectUseCaseError.Unknown)]
    public async Task Execute_returns_save_error(
        UpdateProjectError updateError,
        UpdateProjectUseCaseError expectedError
    )
    {
        var context = new TestContext()
            .WithResolvedUser(user: ResolvedUserBuilder.CreateAdministratorBob())
            .WithUpdateProjectFailureResult(updateError);

        var result = await context
            .BuildUseCase()
            .Execute(
                id: 1,
                name: "Algemeen",
                enabled: true,
                userIds: [42, 43],
                invoiceItems: [(10, "Dev"), (null, "Planning")],
                cancellationToken: CancellationToken.None
            );

        result.TrySuccess(out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(expectedError);
    }

    [Theory]
    [InlineData(
        ResolveUserError.UserNotAuthenticated,
        UpdateProjectUseCaseError.UnauthenticatedUser
    )]
    [InlineData(ResolveUserError.MalformedUserClaims, UpdateProjectUseCaseError.Unknown)]
    [InlineData(ResolveUserError.UnsupportedIdentityProvider, UpdateProjectUseCaseError.Unknown)]
    [InlineData(ResolveUserError.UserNotFound, UpdateProjectUseCaseError.Unknown)]
    [InlineData(ResolveUserError.UserRegistrationFailed, UpdateProjectUseCaseError.Unknown)]
    public async Task Execute_returns_error_for_user_resolve_error(
        ResolveUserError resolveUserError,
        UpdateProjectUseCaseError expectedError
    )
    {
        var context = new TestContext().WithResolveUserError(resolveUserError);

        var result = await context
            .BuildUseCase()
            .Execute(
                id: 1,
                name: "Algemeen",
                enabled: true,
                userIds: [42, 43],
                invoiceItems: [(10, "Dev"), (null, "Planning")],
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
        var context = new TestContext().WithResolvedUser(
            user: ResolvedUserBuilder
                .AsAdministratorBob()
                .WithProjectsPermission(projectsPermission)
                .Build()
        );

        var result = await context
            .BuildUseCase()
            .Execute(
                id: 1,
                name: "Algemeen",
                enabled: true,
                userIds: [42, 43],
                invoiceItems: [(10, "Dev"), (null, "Planning")],
                cancellationToken: CancellationToken.None
            );
        result.TrySuccess(out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(UpdateProjectUseCaseError.UnauthorizedUser);
    }

    private class TestContext
    {
        public Mock<IResolveUser> ResolveUserMock { get; } = new(MockBehavior.Strict);
        public Mock<IUpdateProject> UpdateProjectMock { get; } = new(MockBehavior.Strict);
        public Mock<ILogger<UpdateProjectUseCase>> LoggerMock { get; } = new(MockBehavior.Loose);

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

        public TestContext WithUpdateProjectSuccessResult()
        {
            UpdateProjectMock
                .Setup(x =>
                    x.Execute(
                        It.IsAny<int>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>(),
                        It.IsAny<int[]>(),
                        It.IsAny<(int?, string)[]>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(Result.Success<UpdateProjectError>());

            return this;
        }

        public TestContext WithUpdateProjectFailureResult(UpdateProjectError result)
        {
            UpdateProjectMock
                .Setup(x =>
                    x.Execute(
                        It.IsAny<int>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>(),
                        It.IsAny<int[]>(),
                        It.IsAny<(int?, string)[]>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(Result.Failure(result));

            return this;
        }

        public UpdateProjectUseCase BuildUseCase() =>
            new(
                resolveUser: ResolveUserMock.Object,
                updateProject: UpdateProjectMock.Object,
                logger: LoggerMock.Object
            );

        public void VerifyNoOtherCalls()
        {
            ResolveUserMock.VerifyNoOtherCalls();
            UpdateProjectMock.VerifyNoOtherCalls();
        }
    }
}
