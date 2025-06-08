using Keepi.Core.Entries;

namespace Keepi.Core.Unit.Tests.Entries;

public class GetUserEntriesForWeekUseCaseTests
{
    [Fact]
    public async Task Execute_stores_expected_entities()
    {
        var context = new TestContext().WithGetUserEntriesForDatesResult(
            new UserEntryEntity(
                id: 1,
                userId: 42,
                userEntryCategoryId: 101,
                date: new DateOnly(2025, 6, 16),
                minutes: 60,
                remark: "Nieuwe feature"
            ),
            new UserEntryEntity(
                id: 2,
                userId: 42,
                userEntryCategoryId: 102,
                date: new DateOnly(2025, 6, 16),
                minutes: 45,
                remark: "Project Flyby"
            ),
            new UserEntryEntity(
                id: 3,
                userId: 42,
                userEntryCategoryId: 101,
                date: new DateOnly(2025, 6, 17),
                minutes: 30,
                remark: null
            ),
            new UserEntryEntity(
                id: 4,
                userId: 42,
                userEntryCategoryId: 102,
                date: new DateOnly(2025, 6, 18),
                minutes: 15,
                remark: null
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

        result.ShouldBeEquivalentTo(
            new GetUserEntriesForWeekUseCaseOutput(
                Monday: new GetUserEntriesForWeekUseCaseOutputDay(
                    Entries:
                    [
                        new GetUserEntriesForWeekUseCaseOutputDayEntry(
                            EntryCategoryId: 101,
                            Minutes: 60,
                            Remark: "Nieuwe feature"
                        ),
                        new GetUserEntriesForWeekUseCaseOutputDayEntry(
                            EntryCategoryId: 102,
                            Minutes: 45,
                            Remark: "Project Flyby"
                        ),
                    ]
                ),
                Tuesday: new GetUserEntriesForWeekUseCaseOutputDay(
                    Entries:
                    [
                        new GetUserEntriesForWeekUseCaseOutputDayEntry(
                            EntryCategoryId: 101,
                            Minutes: 30,
                            Remark: null
                        ),
                    ]
                ),
                Wednesday: new GetUserEntriesForWeekUseCaseOutputDay(
                    Entries:
                    [
                        new GetUserEntriesForWeekUseCaseOutputDayEntry(
                            EntryCategoryId: 102,
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

    class TestContext
    {
        public Mock<IGetUserEntriesForDates> GetUserEntriesForDatesMock { get; } =
            new(MockBehavior.Strict);

        public TestContext WithGetUserEntriesForDatesResult(params UserEntryEntity[] entities)
        {
            GetUserEntriesForDatesMock
                .Setup(x =>
                    x.Execute(
                        It.IsAny<int>(),
                        It.IsAny<DateOnly[]>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(entities);

            return this;
        }

        public GetUserEntriesForWeekUseCase BuildUseCase() =>
            new(getUserEntriesForDates: GetUserEntriesForDatesMock.Object);

        public void VerifyNoOtherCalls()
        {
            GetUserEntriesForDatesMock.VerifyNoOtherCalls();
        }
    }
}
