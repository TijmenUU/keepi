using Keepi.Core.Entries;
using Microsoft.Extensions.Logging;

namespace Keepi.Core.Unit.Tests.Entries;

public class GetUserEntriesForWeekUseCaseTests
{
    [Fact]
    public async Task Execute_stores_expected_entities()
    {
        var context = new TestContext().WithGetUserEntriesForDatesSuccessResult(
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
            .Execute(
                userId: 42,
                year: 2025,
                weekNumber: 25,
                cancellationToken: CancellationToken.None
            );

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
    }

    [Fact]
    public async Task Execute_returns_unknown_get_user_entries_error()
    {
        var context = new TestContext().WithGetUserEntriesForDatesFailureResult(
            GetUserEntriesForDatesError.Unknown
        );

        var result = await context
            .BuildUseCase()
            .Execute(
                userId: 42,
                year: 2025,
                weekNumber: 25,
                cancellationToken: CancellationToken.None
            );

        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(GetUserEntriesForWeekUseCaseError.Unknown);
    }

    class TestContext
    {
        public Mock<IGetUserEntriesForDates> GetUserEntriesForDatesMock { get; } =
            new(MockBehavior.Strict);

        public Mock<ILogger<GetUserEntriesForWeekUseCase>> LoggerMock { get; } =
            new(MockBehavior.Loose);

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
                getUserEntriesForDates: GetUserEntriesForDatesMock.Object,
                logger: LoggerMock.Object
            );

        public void VerifyNoOtherCalls()
        {
            GetUserEntriesForDatesMock.VerifyNoOtherCalls();
        }
    }
}
