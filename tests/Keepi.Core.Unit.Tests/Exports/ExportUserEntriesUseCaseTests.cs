using Keepi.Core.Entries;
using Keepi.Core.Exports;
using Keepi.Core.InvoiceItems;
using Keepi.Core.Projects;
using Keepi.Core.Unit.Tests.Builders;
using Keepi.Core.Users;
using Keepi.Generators;

namespace Keepi.Core.Unit.Tests.Exports;

public class ExportUserEntriesUseCaseTests
{
    [Fact]
    public async Task Execute_returns_expected_entries()
    {
        var context = new ExportUserEntriesUseCaseTestContext()
            .WithResolveUserSuccess(ResolvedUserBuilder.CreateAdministratorBob())
            .WithExportEntries(
                new ExportUserEntry(
                    Id: UserEntryId.From(1),
                    UserId: UserId.From(10),
                    UserName: UserName.From("Jaap"),
                    Date: new DateOnly(2025, 6, 22),
                    ProjectId: ProjectId.From(5),
                    ProjectName: ProjectName.From("Ontwikkeling"),
                    InvoiceItemId: InvoiceItemId.From(2),
                    InvoiceItemName: InvoiceItemName.From("Dev"),
                    Minutes: UserEntryMinutes.From(60),
                    Remark: UserEntryRemark.From("Project Flyby")
                ),
                new ExportUserEntry(
                    Id: UserEntryId.From(3),
                    UserId: UserId.From(11),
                    UserName: UserName.From("Boris"),
                    Date: new DateOnly(2025, 6, 23),
                    ProjectId: ProjectId.From(6),
                    ProjectName: ProjectName.From("Intern"),
                    InvoiceItemId: InvoiceItemId.From(4),
                    InvoiceItemName: InvoiceItemName.From("Administratie"),
                    Minutes: UserEntryMinutes.From(45),
                    Remark: UserEntryRemark.From("ISO controle")
                )
            );

        var result = await context
            .BuildTarget()
            .Execute(
                start: new DateOnly(2025, 6, 22),
                stop: new DateOnly(2025, 6, 23),
                CancellationToken.None
            );

        result.TrySuccess(out var entriesTask, out _).ShouldBeTrue();

        var entries = await entriesTask.ToArrayAsync(
            cancellationToken: TestContext.Current.CancellationToken
        );
        entries.Length.ShouldBe(2);
        entries[0]
            .ShouldBeEquivalentTo(
                new ExportUserEntry(
                    Id: UserEntryId.From(1),
                    UserId: UserId.From(10),
                    UserName: UserName.From("Jaap"),
                    Date: new DateOnly(2025, 6, 22),
                    ProjectId: ProjectId.From(5),
                    ProjectName: ProjectName.From("Ontwikkeling"),
                    InvoiceItemId: InvoiceItemId.From(2),
                    InvoiceItemName: InvoiceItemName.From("Dev"),
                    Minutes: UserEntryMinutes.From(60),
                    Remark: UserEntryRemark.From("Project Flyby")
                )
            );
        entries[1]
            .ShouldBeEquivalentTo(
                new ExportUserEntry(
                    Id: UserEntryId.From(3),
                    UserId: UserId.From(11),
                    UserName: UserName.From("Boris"),
                    Date: new DateOnly(2025, 6, 23),
                    ProjectId: ProjectId.From(6),
                    ProjectName: ProjectName.From("Intern"),
                    InvoiceItemId: InvoiceItemId.From(4),
                    InvoiceItemName: InvoiceItemName.From("Administratie"),
                    Minutes: UserEntryMinutes.From(45),
                    Remark: UserEntryRemark.From("ISO controle")
                )
            );

        context.ResolveUserMock.Verify(x => x.Execute(It.IsAny<CancellationToken>()));
        context.GetExportUserEntriesMock.Verify(x =>
            x.Execute(
                new DateOnly(2025, 6, 22),
                new DateOnly(2025, 6, 23),
                It.IsAny<CancellationToken>()
            )
        );
        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_returns_expected_entries_for_single_day_export()
    {
        var context = new ExportUserEntriesUseCaseTestContext()
            .WithResolveUserSuccess(ResolvedUserBuilder.CreateAdministratorBob())
            .WithExportEntries(
                new ExportUserEntry(
                    Id: UserEntryId.From(1),
                    UserId: UserId.From(10),
                    UserName: UserName.From("Jaap"),
                    Date: new DateOnly(2025, 6, 22),
                    ProjectId: ProjectId.From(5),
                    ProjectName: ProjectName.From("Ontwikkeling"),
                    InvoiceItemId: InvoiceItemId.From(2),
                    InvoiceItemName: InvoiceItemName.From("Dev"),
                    Minutes: UserEntryMinutes.From(60),
                    Remark: UserEntryRemark.From("Project Flyby")
                )
            );

        var result = await context
            .BuildTarget()
            .Execute(
                start: new DateOnly(2025, 6, 22),
                stop: new DateOnly(2025, 6, 22),
                CancellationToken.None
            );

        result.TrySuccess(out var entriesTask, out _).ShouldBeTrue();

        var entries = await entriesTask.ToArrayAsync(
            cancellationToken: TestContext.Current.CancellationToken
        );
        entries.Length.ShouldBe(1);
        entries[0]
            .ShouldBeEquivalentTo(
                new ExportUserEntry(
                    Id: UserEntryId.From(1),
                    UserId: UserId.From(10),
                    UserName: UserName.From("Jaap"),
                    Date: new DateOnly(2025, 6, 22),
                    ProjectId: ProjectId.From(5),
                    ProjectName: ProjectName.From("Ontwikkeling"),
                    InvoiceItemId: InvoiceItemId.From(2),
                    InvoiceItemName: InvoiceItemName.From("Dev"),
                    Minutes: UserEntryMinutes.From(60),
                    Remark: UserEntryRemark.From("Project Flyby")
                )
            );

        context.ResolveUserMock.Verify(x => x.Execute(It.IsAny<CancellationToken>()));
        context.GetExportUserEntriesMock.Verify(x =>
            x.Execute(
                new DateOnly(2025, 6, 22),
                new DateOnly(2025, 6, 22),
                It.IsAny<CancellationToken>()
            )
        );
        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_returns_error_for_start_date_greater_than_stop_date()
    {
        var context = new ExportUserEntriesUseCaseTestContext()
            .WithResolveUserSuccess(ResolvedUserBuilder.CreateAdministratorBob())
            .WithExportEntries(
                new ExportUserEntry(
                    Id: UserEntryId.From(1),
                    UserId: UserId.From(10),
                    UserName: UserName.From("Jaap"),
                    Date: new DateOnly(2025, 6, 22),
                    ProjectId: ProjectId.From(5),
                    ProjectName: ProjectName.From("Ontwikkeling"),
                    InvoiceItemId: InvoiceItemId.From(2),
                    InvoiceItemName: InvoiceItemName.From("Dev"),
                    Minutes: UserEntryMinutes.From(60),
                    Remark: UserEntryRemark.From("Project Flyby")
                ),
                new ExportUserEntry(
                    Id: UserEntryId.From(3),
                    UserId: UserId.From(10),
                    UserName: UserName.From("Jaap"),
                    Date: new DateOnly(2025, 6, 23),
                    ProjectId: ProjectId.From(6),
                    ProjectName: ProjectName.From("Intern"),
                    InvoiceItemId: InvoiceItemId.From(4),
                    InvoiceItemName: InvoiceItemName.From("Administratie"),
                    Minutes: UserEntryMinutes.From(45),
                    Remark: UserEntryRemark.From("ISO controle")
                )
            );

        var result = await context
            .BuildTarget()
            .Execute(
                start: new DateOnly(2025, 6, 24),
                stop: new DateOnly(2025, 6, 23),
                CancellationToken.None
            );
        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(ExportUserEntriesUseCaseError.StartGreaterThanStop);
    }

    [Theory]
    [InlineData(
        ResolveUserError.UserNotAuthenticated,
        ExportUserEntriesUseCaseError.UnauthenticatedUser
    )]
    [InlineData(ResolveUserError.MalformedUserClaims, ExportUserEntriesUseCaseError.Unknown)]
    [InlineData(
        ResolveUserError.UnsupportedIdentityProvider,
        ExportUserEntriesUseCaseError.Unknown
    )]
    [InlineData(ResolveUserError.UserNotFound, ExportUserEntriesUseCaseError.Unknown)]
    [InlineData(ResolveUserError.UserRegistrationFailed, ExportUserEntriesUseCaseError.Unknown)]
    public async Task Execute_returns_error_for_user_resolve_error(
        ResolveUserError resolveUserError,
        ExportUserEntriesUseCaseError expectedError
    )
    {
        var context = new ExportUserEntriesUseCaseTestContext().WithResolveUserError(
            resolveUserError
        );

        var result = await context
            .BuildTarget()
            .Execute(
                start: new DateOnly(2025, 6, 24),
                stop: new DateOnly(2025, 6, 23),
                CancellationToken.None
            );
        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(expectedError);
    }

    [Fact]
    public async Task Execute_returns_error_for_unauthorized_user()
    {
        var context = new ExportUserEntriesUseCaseTestContext().WithResolveUserSuccess(
            ResolvedUserBuilder
                .AsAdministratorBob()
                .WithExportsPermission(UserPermission.None)
                .Build()
        );

        var result = await context
            .BuildTarget()
            .Execute(
                start: new DateOnly(2025, 6, 24),
                stop: new DateOnly(2025, 6, 23),
                CancellationToken.None
            );
        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(ExportUserEntriesUseCaseError.UnauthorizedUser);
    }
}

[GenerateTestContext(target: typeof(ExportUserEntriesUseCase), GenerateWithMethods = true)]
internal partial class ExportUserEntriesUseCaseTestContext
{
    public ExportUserEntriesUseCaseTestContext WithExportEntries(
        params ExportUserEntry[] entries
    ) => WithGetExportUserEntriesCall(entries.ToAsyncEnumerable());
}
