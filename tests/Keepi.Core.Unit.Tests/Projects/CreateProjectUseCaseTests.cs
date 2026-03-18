using Keepi.Core.Entries;
using Keepi.Core.InvoiceItems;
using Keepi.Core.Projects;
using Keepi.Core.Unit.Tests.Builders;
using Keepi.Core.Users;
using Keepi.Generators;

namespace Keepi.Core.Unit.Tests.Projects;

public class CreateProjectUseCaseTests
{
    [Fact]
    public async Task Execute_returns_created_project_ID()
    {
        var context = new CreateProjectUseCaseTestContext()
            .WithResolveUserSuccess(ResolvedUserBuilder.CreateAdministratorBob())
            .WithSaveNewProjectSuccess(1);

        var result = await context
            .BuildTarget()
            .Execute(
                name: ProjectName.From("Algemeen"),
                enabled: true,
                userIds: [UserId.From(42), UserId.From(43)],
                invoiceItemNames: [InvoiceItemName.From("Dev"), InvoiceItemName.From("Planning")],
                cancellationToken: CancellationToken.None
            );

        result.TrySuccess(out var successResult, out _).ShouldBeTrue();
        successResult.ShouldBe(1);

        context.ResolveUserMock.Verify(x => x.Execute(It.IsAny<CancellationToken>()));
        context.SaveNewProjectMock.Verify(x =>
            x.Execute(
                ProjectName.From("Algemeen"),
                true,
                It.Is<UserId[]>(u =>
                    u.Length == 2 && u[0] == UserId.From(42) && u[1] == UserId.From(43)
                ),
                It.Is<InvoiceItemName[]>(i =>
                    i.Length == 2 && i[0].Value == "Dev" && i[1].Value == "Planning"
                ),
                It.IsAny<CancellationToken>()
            )
        );
        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_returns_error_for_duplicate_user_ids()
    {
        var context = new CreateProjectUseCaseTestContext().WithResolveUserSuccess(
            ResolvedUserBuilder.CreateAdministratorBob()
        );

        var result = await context
            .BuildTarget()
            .Execute(
                name: ProjectName.From("Algemeen"),
                enabled: true,
                userIds: [UserId.From(42), UserId.From(42)],
                invoiceItemNames: [InvoiceItemName.From("Dev"), InvoiceItemName.From("Planning")],
                cancellationToken: CancellationToken.None
            );

        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(CreateProjectUseCaseError.DuplicateUserIds);
    }

    [Fact]
    public async Task Execute_returns_error_for_duplicate_invoice_item_names()
    {
        var context = new CreateProjectUseCaseTestContext().WithResolveUserSuccess(
            ResolvedUserBuilder.CreateAdministratorBob()
        );

        var result = await context
            .BuildTarget()
            .Execute(
                name: ProjectName.From("Algemeen"),
                enabled: true,
                userIds: [UserId.From(42), UserId.From(43)],
                invoiceItemNames: [InvoiceItemName.From("Dev"), InvoiceItemName.From("Dev")],
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
        var context = new CreateProjectUseCaseTestContext()
            .WithResolveUserSuccess(ResolvedUserBuilder.CreateAdministratorBob())
            .WithSaveNewProjectError(saveError);

        var result = await context
            .BuildTarget()
            .Execute(
                name: ProjectName.From("Algemeen"),
                enabled: true,
                userIds: [UserId.From(42), UserId.From(43)],
                invoiceItemNames: [InvoiceItemName.From("Dev"), InvoiceItemName.From("Planning")],
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
        var context = new CreateProjectUseCaseTestContext().WithResolveUserError(resolveUserError);

        var result = await context
            .BuildTarget()
            .Execute(
                name: ProjectName.From("Algemeen"),
                enabled: true,
                userIds: [UserId.From(42), UserId.From(43)],
                invoiceItemNames: [InvoiceItemName.From("Dev"), InvoiceItemName.From("Planning")],
                cancellationToken: CancellationToken.None
            );

        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(expectedError);
    }

    [Theory]
    [InlineData(UserPermission.None)]
    [InlineData(UserPermission.Read)]
    public async Task Execute_returns_error_for_unauthorized_user(UserPermission projectsPermission)
    {
        var context = new CreateProjectUseCaseTestContext().WithResolveUserSuccess(
            ResolvedUserBuilder
                .AsAdministratorBob()
                .WithProjectsPermission(projectsPermission)
                .Build()
        );

        var result = await context
            .BuildTarget()
            .Execute(
                name: ProjectName.From("Algemeen"),
                enabled: true,
                userIds: [UserId.From(42), UserId.From(43)],
                invoiceItemNames: [InvoiceItemName.From("Dev"), InvoiceItemName.From("Planning")],
                cancellationToken: CancellationToken.None
            );
        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(CreateProjectUseCaseError.UnauthorizedUser);
    }
}

[GenerateTestContext(TargetType = typeof(CreateProjectUseCase), GenerateWithMethods = true)]
internal partial class CreateProjectUseCaseTestContext { }
