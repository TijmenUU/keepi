using Keepi.Core.Entries;
using Keepi.Core.Unit.Tests.Builders;
using Keepi.Core.Users;

namespace Keepi.Core.Unit.Tests.Entries;

public class ExportUserEntriesUseCaseTests
{
    [Fact]
    public async Task Execute_returns_expected_entries()
    {
        var context = new TestContext()
            .WithResolvedUser(user: ResolvedUserBuilder.CreateAdministratorBob())
            .WithExportEntries(
                new ExportUserEntry(
                    Id: 1,
                    Date: new DateOnly(2025, 6, 22),
                    ProjectId: 5,
                    ProjectName: "Ontwikkeling",
                    InvoiceItemId: 2,
                    InvoiceItemName: "Dev",
                    Minutes: 60,
                    Remark: "Project Flyby"
                ),
                new ExportUserEntry(
                    Id: 3,
                    Date: new DateOnly(2025, 6, 23),
                    ProjectId: 6,
                    ProjectName: "Intern",
                    InvoiceItemId: 4,
                    InvoiceItemName: "Administratie",
                    Minutes: 45,
                    Remark: "ISO controle"
                )
            );

        var result = await context
            .BuildUseCase()
            .Execute(
                start: new DateOnly(2025, 6, 22),
                stop: new DateOnly(2025, 6, 23),
                CancellationToken.None
            );

        result.TrySuccess(out var entriesTask, out _).ShouldBeTrue();

        var entries = await entriesTask.ToArrayAsync();
        entries.Length.ShouldBe(2);
        entries[0]
            .ShouldBeEquivalentTo(
                new ExportUserEntry(
                    Id: 1,
                    Date: new DateOnly(2025, 6, 22),
                    ProjectId: 5,
                    ProjectName: "Ontwikkeling",
                    InvoiceItemId: 2,
                    InvoiceItemName: "Dev",
                    Minutes: 60,
                    Remark: "Project Flyby"
                )
            );
        entries[1]
            .ShouldBeEquivalentTo(
                new ExportUserEntry(
                    Id: 3,
                    Date: new DateOnly(2025, 6, 23),
                    ProjectId: 6,
                    ProjectName: "Intern",
                    InvoiceItemId: 4,
                    InvoiceItemName: "Administratie",
                    Minutes: 45,
                    Remark: "ISO controle"
                )
            );

        context.ResolveUserMock.Verify(x => x.Execute(It.IsAny<CancellationToken>()));
        context.GetExportUserEntriesStreamMock.Verify(x =>
            x.Execute(
                42,
                new DateOnly(2025, 6, 22),
                new DateOnly(2025, 6, 23),
                It.IsAny<CancellationToken>()
            )
        );
        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_returns_error_for_start_date_greater_than_stop_date()
    {
        var context = new TestContext()
            .WithResolvedUser(user: ResolvedUserBuilder.CreateAdministratorBob())
            .WithExportEntries(
                new ExportUserEntry(
                    Id: 1,
                    Date: new DateOnly(2025, 6, 22),
                    ProjectId: 5,
                    ProjectName: "Ontwikkeling",
                    InvoiceItemId: 2,
                    InvoiceItemName: "Dev",
                    Minutes: 60,
                    Remark: "Project Flyby"
                ),
                new ExportUserEntry(
                    Id: 3,
                    Date: new DateOnly(2025, 6, 23),
                    ProjectId: 6,
                    ProjectName: "Intern",
                    InvoiceItemId: 4,
                    InvoiceItemName: "Administratie",
                    Minutes: 45,
                    Remark: "ISO controle"
                )
            );

        var result = await context
            .BuildUseCase()
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
        var context = new TestContext().WithResolveUserError(resolveUserError);

        var result = await context
            .BuildUseCase()
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
        var context = new TestContext().WithResolvedUser(
            user: ResolvedUserBuilder
                .AsAdministratorBob()
                .WithExportsPermission(UserPermission.None)
                .Build()
        );

        var result = await context
            .BuildUseCase()
            .Execute(
                start: new DateOnly(2025, 6, 24),
                stop: new DateOnly(2025, 6, 23),
                CancellationToken.None
            );
        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(ExportUserEntriesUseCaseError.UnauthorizedUser);
    }

    private class TestContext
    {
        public Mock<IResolveUser> ResolveUserMock { get; } = new(MockBehavior.Strict);
        public Mock<IGetExportUserEntries> GetExportUserEntriesStreamMock { get; } =
            new(MockBehavior.Strict);

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

        public TestContext WithExportEntries(params ExportUserEntry[] entries)
        {
            GetExportUserEntriesStreamMock
                .Setup(x =>
                    x.Execute(
                        It.IsAny<int>(),
                        It.IsAny<DateOnly>(),
                        It.IsAny<DateOnly>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(entries.ToAsyncEnumerable());

            return this;
        }

        public ExportUserEntriesUseCase BuildUseCase() =>
            new(
                resolveUser: ResolveUserMock.Object,
                getExportUserEntries: GetExportUserEntriesStreamMock.Object
            );

        public void VerifyNoOtherCalls()
        {
            ResolveUserMock.VerifyNoOtherCalls();
            GetExportUserEntriesStreamMock.VerifyNoOtherCalls();
        }
    }
}
