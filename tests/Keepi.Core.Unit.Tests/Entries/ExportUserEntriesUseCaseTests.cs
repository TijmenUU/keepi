using Keepi.Core.Entries;

namespace Keepi.Core.Unit.Tests.Entries;

public class ExportUserEntriesUseCaseTests
{
    [Fact]
    public async Task Execute_returns_expected_entries()
    {
        var context = new TestContext().WithExportEntries(
            new ExportUserEntry(
                Id: 1,
                Date: new DateOnly(2025, 6, 22),
                UserEntryCategoryId: 2,
                UserEntryCategoryName: "Dev",
                Minutes: 60,
                Remark: "Project Flyby"
            ),
            new ExportUserEntry(
                Id: 3,
                Date: new DateOnly(2025, 6, 23),
                UserEntryCategoryId: 4,
                UserEntryCategoryName: "Administratie",
                Minutes: 45,
                Remark: "ISO controle"
            )
        );

        var result = context
            .BuildUseCase()
            .Execute(
                userId: 42,
                start: new DateOnly(2025, 6, 22),
                stop: new DateOnly(2025, 6, 23),
                CancellationToken.None
            );
        result.Succeeded.ShouldBeTrue();

        var entries = await result.SuccessOrThrow.ToArrayAsync();
        entries.Length.ShouldBe(2);
        entries[0]
            .ShouldBeEquivalentTo(
                new ExportUserEntry(
                    Id: 1,
                    Date: new DateOnly(2025, 6, 22),
                    UserEntryCategoryId: 2,
                    UserEntryCategoryName: "Dev",
                    Minutes: 60,
                    Remark: "Project Flyby"
                )
            );
        entries[1]
            .ShouldBeEquivalentTo(
                new ExportUserEntry(
                    Id: 3,
                    Date: new DateOnly(2025, 6, 23),
                    UserEntryCategoryId: 4,
                    UserEntryCategoryName: "Administratie",
                    Minutes: 45,
                    Remark: "ISO controle"
                )
            );

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
    public void Execute_returns_error_for_start_date_greater_than_stop_date()
    {
        var context = new TestContext().WithExportEntries(
            new ExportUserEntry(
                Id: 1,
                Date: new DateOnly(2025, 6, 22),
                UserEntryCategoryId: 2,
                UserEntryCategoryName: "Dev",
                Minutes: 60,
                Remark: "Project Flyby"
            ),
            new ExportUserEntry(
                Id: 3,
                Date: new DateOnly(2025, 6, 23),
                UserEntryCategoryId: 4,
                UserEntryCategoryName: "Administratie",
                Minutes: 45,
                Remark: "ISO controle"
            )
        );

        var result = context
            .BuildUseCase()
            .Execute(
                userId: 42,
                start: new DateOnly(2025, 6, 24),
                stop: new DateOnly(2025, 6, 23),
                CancellationToken.None
            );
        result.Succeeded.ShouldBeFalse();
        result.ErrorOrNull.ShouldBe(ExportUserEntriesUseCaseError.StartGreaterThanStop);
    }

    class TestContext
    {
        public Mock<IGetExportUserEntries> GetExportUserEntriesStreamMock { get; } =
            new(MockBehavior.Strict);

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
            new(getExportUserEntries: GetExportUserEntriesStreamMock.Object);

        public void VerifyNoOtherCalls()
        {
            GetExportUserEntriesStreamMock.VerifyNoOtherCalls();
        }
    }
}
