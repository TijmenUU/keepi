using Keepi.Core.Entries;
using Keepi.Core.Unit.Tests.Builders;
using Keepi.Core.Users;
using Microsoft.Extensions.Logging;

namespace Keepi.Core.Unit.Tests.Entries;

public class GetUserEntriesForWeekUseCaseTests
{
    [Fact]
    public async Task Execute_stores_expected_entities()
    {
        var context = new TestContext()
            .WithResolvedUser(user: ResolvedUserBuilder.CreateAdministratorBob())
            .WithGetUserEntriesForDatesSuccessResult(
                new GetUserEntriesForDatesResultEntry(
                    Id: 1,
                    InvoiceItemId: 101,
                    Date: new DateOnly(2025, 6, 16),
                    Minutes: 60,
                    Remark: "Nieuwe feature"
                ),
                new GetUserEntriesForDatesResultEntry(
                    Id: 2,
                    InvoiceItemId: 102,
                    Date: new DateOnly(2025, 6, 16),
                    Minutes: 45,
                    Remark: "Project Flyby"
                ),
                new GetUserEntriesForDatesResultEntry(
                    Id: 3,
                    InvoiceItemId: 101,
                    Date: new DateOnly(2025, 6, 17),
                    Minutes: 30,
                    Remark: null
                ),
                new GetUserEntriesForDatesResultEntry(
                    Id: 4,
                    InvoiceItemId: 102,
                    Date: new DateOnly(2025, 6, 18),
                    Minutes: 15,
                    Remark: null
                )
            );

        var result = await context
            .BuildUseCase()
            .Execute(year: 2025, weekNumber: 25, cancellationToken: CancellationToken.None);

        result.TrySuccess(out var successResult, out _).ShouldBeTrue();

        successResult.ShouldBeEquivalentTo(
            new GetUserEntriesForWeekUseCaseOutput(
                Monday: new GetUserEntriesForWeekUseCaseOutputDay(
                    Entries:
                    [
                        new GetUserEntriesForWeekUseCaseOutputDayEntry(
                            InvoiceItemId: 101,
                            Minutes: 60,
                            Remark: "Nieuwe feature"
                        ),
                        new GetUserEntriesForWeekUseCaseOutputDayEntry(
                            InvoiceItemId: 102,
                            Minutes: 45,
                            Remark: "Project Flyby"
                        ),
                    ]
                ),
                Tuesday: new GetUserEntriesForWeekUseCaseOutputDay(
                    Entries:
                    [
                        new GetUserEntriesForWeekUseCaseOutputDayEntry(
                            InvoiceItemId: 101,
                            Minutes: 30,
                            Remark: null
                        ),
                    ]
                ),
                Wednesday: new GetUserEntriesForWeekUseCaseOutputDay(
                    Entries:
                    [
                        new GetUserEntriesForWeekUseCaseOutputDayEntry(
                            InvoiceItemId: 102,
                            Minutes: 15,
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
                42,
                It.Is<DateOnly[]>(a => a.SequenceEqual(expectedDates)),
                It.IsAny<CancellationToken>()
            )
        );

        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_returns_unknown_get_user_entries_error()
    {
        var context = new TestContext()
            .WithResolvedUser(user: ResolvedUserBuilder.CreateAdministratorBob())
            .WithGetUserEntriesForDatesFailureResult(GetUserEntriesForDatesError.Unknown);

        var result = await context
            .BuildUseCase()
            .Execute(year: 2025, weekNumber: 25, cancellationToken: CancellationToken.None);

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
        var context = new TestContext().WithResolveUserError(resolveUserError);

        var result = await context
            .BuildUseCase()
            .Execute(year: 2025, weekNumber: 25, cancellationToken: CancellationToken.None);

        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(expectedError);
    }

    [Fact]
    public async Task Execute_returns_error_for_unauthorized_user()
    {
        var context = new TestContext().WithResolvedUser(
            user: ResolvedUserBuilder
                .AsAdministratorBob()
                .WithEntriesPermission(UserPermission.None)
                .Build()
        );

        var result = await context
            .BuildUseCase()
            .Execute(year: 2025, weekNumber: 25, cancellationToken: CancellationToken.None);

        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(GetUserEntriesForWeekUseCaseError.UnauthorizedUser);
    }

    private class TestContext
    {
        public Mock<IResolveUser> ResolveUserMock { get; } = new(MockBehavior.Strict);

        public Mock<IGetUserEntriesForDates> GetUserEntriesForDatesMock { get; } =
            new(MockBehavior.Strict);

        public Mock<ILogger<GetUserEntriesForWeekUseCase>> LoggerMock { get; } =
            new(MockBehavior.Loose);

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

        public TestContext WithGetUserEntriesForDatesSuccessResult(
            params GetUserEntriesForDatesResultEntry[] entities
        )
        {
            GetUserEntriesForDatesMock
                .Setup(x =>
                    x.Execute(
                        It.IsAny<int>(),
                        It.IsAny<DateOnly[]>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(
                    Result.Success<GetUserEntriesForDatesResult, GetUserEntriesForDatesError>(
                        new(Entries: entities)
                    )
                );

            return this;
        }

        public TestContext WithGetUserEntriesForDatesFailureResult(
            GetUserEntriesForDatesError result
        )
        {
            GetUserEntriesForDatesMock
                .Setup(x =>
                    x.Execute(
                        It.IsAny<int>(),
                        It.IsAny<DateOnly[]>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(
                    Result.Failure<GetUserEntriesForDatesResult, GetUserEntriesForDatesError>(
                        result
                    )
                );

            return this;
        }

        public GetUserEntriesForWeekUseCase BuildUseCase() =>
            new(
                resolveUser: ResolveUserMock.Object,
                getUserEntriesForDates: GetUserEntriesForDatesMock.Object,
                logger: LoggerMock.Object
            );

        public void VerifyNoOtherCalls()
        {
            ResolveUserMock.VerifyNoOtherCalls();
            GetUserEntriesForDatesMock.VerifyNoOtherCalls();
        }
    }
}
