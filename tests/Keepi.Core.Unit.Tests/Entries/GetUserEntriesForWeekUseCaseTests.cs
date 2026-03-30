using Keepi.Core.Entries;
using Keepi.Core.InvoiceItems;
using Keepi.Core.Unit.Tests.Builders;
using Keepi.Core.Users;
using Keepi.Generators;

namespace Keepi.Core.Unit.Tests.Entries;

public class GetUserEntriesForWeekUseCaseTests
{
    [Fact]
    public async Task Execute_stores_expected_entities()
    {
        var context = new GetUserEntriesForWeekUseCaseTestContext()
            .WithResolveUserSuccess(ResolvedUserBuilder.CreateAdministratorBob())
            .WithGetUserEntriesForDatesSuccess(
                new GetUserEntriesForDatesResultEntry(
                    Id: UserEntryId.From(1),
                    InvoiceItemId: InvoiceItemId.From(101),
                    Date: new DateOnly(2025, 6, 16),
                    Minutes: UserEntryMinutes.From(60),
                    Remark: UserEntryRemark.From("Nieuwe feature")
                ),
                new GetUserEntriesForDatesResultEntry(
                    Id: UserEntryId.From(2),
                    InvoiceItemId: InvoiceItemId.From(102),
                    Date: new DateOnly(2025, 6, 16),
                    Minutes: UserEntryMinutes.From(45),
                    Remark: UserEntryRemark.From("Project Flyby")
                ),
                new GetUserEntriesForDatesResultEntry(
                    Id: UserEntryId.From(3),
                    InvoiceItemId: InvoiceItemId.From(101),
                    Date: new DateOnly(2025, 6, 17),
                    Minutes: UserEntryMinutes.From(30),
                    Remark: null
                ),
                new GetUserEntriesForDatesResultEntry(
                    Id: UserEntryId.From(4),
                    InvoiceItemId: InvoiceItemId.From(102),
                    Date: new DateOnly(2025, 6, 18),
                    Minutes: UserEntryMinutes.From(15),
                    Remark: null
                )
            );

        var result = await context
            .BuildTarget()
            .Execute(
                year: Year.From(2025),
                weekNumber: WeekNumber.From(25),
                cancellationToken: CancellationToken.None
            );

        result.TrySuccess(out var successResult, out _).ShouldBeTrue();

        successResult.ShouldBeEquivalentTo(
            new GetUserEntriesForWeekUseCaseOutput(
                Monday: new GetUserEntriesForWeekUseCaseOutputDay(
                    Entries:
                    [
                        new GetUserEntriesForWeekUseCaseOutputDayEntry(
                            InvoiceItemId: InvoiceItemId.From(101),
                            Minutes: UserEntryMinutes.From(60),
                            Remark: UserEntryRemark.From("Nieuwe feature")
                        ),
                        new GetUserEntriesForWeekUseCaseOutputDayEntry(
                            InvoiceItemId: InvoiceItemId.From(102),
                            Minutes: UserEntryMinutes.From(45),
                            Remark: UserEntryRemark.From("Project Flyby")
                        ),
                    ]
                ),
                Tuesday: new GetUserEntriesForWeekUseCaseOutputDay(
                    Entries:
                    [
                        new GetUserEntriesForWeekUseCaseOutputDayEntry(
                            InvoiceItemId: InvoiceItemId.From(101),
                            Minutes: UserEntryMinutes.From(30),
                            Remark: null
                        ),
                    ]
                ),
                Wednesday: new GetUserEntriesForWeekUseCaseOutputDay(
                    Entries:
                    [
                        new GetUserEntriesForWeekUseCaseOutputDayEntry(
                            InvoiceItemId: InvoiceItemId.From(102),
                            Minutes: UserEntryMinutes.From(15),
                            Remark: null
                        ),
                    ]
                ),
                Thursday: new GetUserEntriesForWeekUseCaseOutputDay(Entries: []),
                Friday: new GetUserEntriesForWeekUseCaseOutputDay(Entries: []),
                Saturday: new GetUserEntriesForWeekUseCaseOutputDay(Entries: []),
                Sunday: new GetUserEntriesForWeekUseCaseOutputDay(Entries: [])
            )
        );

        context.ResolveUserMock.Verify(x => x.Execute(It.IsAny<CancellationToken>()));

        DateOnly[] expectedDates =
        [
            new DateOnly(2025, 6, 16),
            new DateOnly(2025, 6, 17),
            new DateOnly(2025, 6, 18),
            new DateOnly(2025, 6, 19),
            new DateOnly(2025, 6, 20),
            new DateOnly(2025, 6, 21),
            new DateOnly(2025, 6, 22),
        ];
        context.GetUserEntriesForDatesMock.Verify(x =>
            x.Execute(
                UserId.From(42),
                It.Is<DateOnly[]>(a => a.SequenceEqual(expectedDates)),
                It.IsAny<CancellationToken>()
            )
        );

        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_returns_unknown_get_user_entries_error()
    {
        var context = new GetUserEntriesForWeekUseCaseTestContext()
            .WithResolveUserSuccess(ResolvedUserBuilder.CreateAdministratorBob())
            .WithGetUserEntriesForDatesError(GetUserEntriesForDatesError.Unknown);

        var result = await context
            .BuildTarget()
            .Execute(
                year: Year.From(2025),
                weekNumber: WeekNumber.From(25),
                cancellationToken: CancellationToken.None
            );

        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(GetUserEntriesForWeekUseCaseError.Unknown);
    }

    [Theory]
    [InlineData(
        ResolveUserError.UserNotAuthenticated,
        GetUserEntriesForWeekUseCaseError.UnauthenticatedUser
    )]
    [InlineData(ResolveUserError.MalformedUserClaims, GetUserEntriesForWeekUseCaseError.Unknown)]
    [InlineData(
        ResolveUserError.UnsupportedIdentityProvider,
        GetUserEntriesForWeekUseCaseError.Unknown
    )]
    [InlineData(ResolveUserError.UserNotFound, GetUserEntriesForWeekUseCaseError.Unknown)]
    [InlineData(ResolveUserError.UserRegistrationFailed, GetUserEntriesForWeekUseCaseError.Unknown)]
    public async Task Execute_returns_error_for_user_resolve_error(
        ResolveUserError resolveUserError,
        GetUserEntriesForWeekUseCaseError expectedError
    )
    {
        var context = new GetUserEntriesForWeekUseCaseTestContext().WithResolveUserError(
            resolveUserError
        );

        var result = await context
            .BuildTarget()
            .Execute(
                year: Year.From(2025),
                weekNumber: WeekNumber.From(25),
                cancellationToken: CancellationToken.None
            );

        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(expectedError);
    }

    [Fact]
    public async Task Execute_returns_error_for_unauthorized_user()
    {
        var context = new GetUserEntriesForWeekUseCaseTestContext().WithResolveUserSuccess(
            ResolvedUserBuilder
                .AsAdministratorBob()
                .WithEntriesPermission(UserPermission.None)
                .Build()
        );

        var result = await context
            .BuildTarget()
            .Execute(
                year: Year.From(2025),
                weekNumber: WeekNumber.From(25),
                cancellationToken: CancellationToken.None
            );

        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(GetUserEntriesForWeekUseCaseError.UnauthorizedUser);
    }
}

[GenerateTestContext(target: typeof(GetUserEntriesForWeekUseCase), GenerateWithMethods = true)]
internal partial class GetUserEntriesForWeekUseCaseTestContext
{
    public GetUserEntriesForWeekUseCaseTestContext WithGetUserEntriesForDatesSuccess(
        params GetUserEntriesForDatesResultEntry[] entities
    ) => WithGetUserEntriesForDatesSuccess((GetUserEntriesForDatesResult)new(Entries: entities));
}
