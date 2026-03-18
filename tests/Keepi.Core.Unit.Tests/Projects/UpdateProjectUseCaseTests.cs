using Keepi.Core.Entries;
using Keepi.Core.InvoiceItems;
using Keepi.Core.Projects;
using Keepi.Core.Unit.Tests.Builders;
using Keepi.Core.Users;
using Keepi.Generators;

namespace Keepi.Core.Unit.Tests.Projects;

public class UpdateProjectUseCaseTests
{
    [Fact]
    public async Task Execute_returns_created_project_ID()
    {
        var context = new UpdateProjectUseCaseTestContext()
            .WithResolveUserSuccess(ResolvedUserBuilder.CreateAdministratorBob())
            .WithUpdateProjectSuccess();

        var result = await context
            .BuildTarget()
            .Execute(
                id: ProjectId.From(1),
                name: ProjectName.From("Algemeen"),
                enabled: true,
                userIds: [UserId.From(42), UserId.From(43)],
                invoiceItems:
                [
                    (InvoiceItemId.From(10), InvoiceItemName.From("Dev")),
                    (null, InvoiceItemName.From("Planning")),
                ],
                cancellationToken: CancellationToken.None
            );

        result.TrySuccess(out _).ShouldBeTrue();

        context.ResolveUserMock.Verify(x => x.Execute(It.IsAny<CancellationToken>()));
        context.UpdateProjectMock.Verify(x =>
            x.Execute(
                ProjectId.From(1),
                ProjectName.From("Algemeen"),
                true,
                It.Is<UserId[]>(u => u.Length == 2 && u[0] == 42 && u[1] == 43),
                It.Is<(InvoiceItemId?, InvoiceItemName)[]>(i =>
                    i.Length == 2
                    && i[0].Item1 == InvoiceItemId.From(10)
                    && i[0].Item2 == "Dev"
                    && i[1].Item1 == null
                    && i[1].Item2 == "Planning"
                ),
                It.IsAny<CancellationToken>()
            )
        );
        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_returns_error_for_duplicate_user_ids()
    {
        var context = new UpdateProjectUseCaseTestContext().WithResolveUserSuccess(
            ResolvedUserBuilder.CreateAdministratorBob()
        );

        var result = await context
            .BuildTarget()
            .Execute(
                id: ProjectId.From(1),
                name: ProjectName.From("Algemeen"),
                enabled: true,
                userIds: [UserId.From(42), UserId.From(42)],
                invoiceItems:
                [
                    (InvoiceItemId.From(10), InvoiceItemName.From("Dev")),
                    (null, InvoiceItemName.From("Planning")),
                ],
                cancellationToken: CancellationToken.None
            );

        result.TrySuccess(out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(UpdateProjectUseCaseError.DuplicateUserIds);
    }

    [Fact]
    public async Task Execute_returns_error_for_duplicate_invoice_item_names()
    {
        var context = new UpdateProjectUseCaseTestContext().WithResolveUserSuccess(
            ResolvedUserBuilder.CreateAdministratorBob()
        );

        var result = await context
            .BuildTarget()
            .Execute(
                id: ProjectId.From(1),
                name: ProjectName.From("Algemeen"),
                enabled: true,
                userIds: [UserId.From(42), UserId.From(43)],
                invoiceItems:
                [
                    (InvoiceItemId.From(10), InvoiceItemName.From("Dev")),
                    (null, InvoiceItemName.From("Dev")),
                ],
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
        var context = new UpdateProjectUseCaseTestContext()
            .WithResolveUserSuccess(ResolvedUserBuilder.CreateAdministratorBob())
            .WithUpdateProjectError(updateError);

        var result = await context
            .BuildTarget()
            .Execute(
                id: ProjectId.From(1),
                name: ProjectName.From("Algemeen"),
                enabled: true,
                userIds: [UserId.From(42), UserId.From(43)],
                invoiceItems:
                [
                    (InvoiceItemId.From(10), InvoiceItemName.From("Dev")),
                    (null, InvoiceItemName.From("Planning")),
                ],
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
        var context = new UpdateProjectUseCaseTestContext().WithResolveUserError(resolveUserError);

        var result = await context
            .BuildTarget()
            .Execute(
                id: ProjectId.From(1),
                name: ProjectName.From("Algemeen"),
                enabled: true,
                userIds: [UserId.From(42), UserId.From(43)],
                invoiceItems:
                [
                    (InvoiceItemId.From(10), InvoiceItemName.From("Dev")),
                    (null, InvoiceItemName.From("Planning")),
                ],
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
        var context = new UpdateProjectUseCaseTestContext().WithResolveUserSuccess(
            ResolvedUserBuilder
                .AsAdministratorBob()
                .WithProjectsPermission(projectsPermission)
                .Build()
        );

        var result = await context
            .BuildTarget()
            .Execute(
                id: ProjectId.From(1),
                name: ProjectName.From("Algemeen"),
                enabled: true,
                userIds: [UserId.From(42), UserId.From(43)],
                invoiceItems:
                [
                    (InvoiceItemId.From(10), InvoiceItemName.From("Dev")),
                    (null, InvoiceItemName.From("Planning")),
                ],
                cancellationToken: CancellationToken.None
            );
        result.TrySuccess(out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(UpdateProjectUseCaseError.UnauthorizedUser);
    }
}

[GenerateTestContext(TargetType = typeof(UpdateProjectUseCase), GenerateWithMethods = true)]
internal partial class UpdateProjectUseCaseTestContext { }
