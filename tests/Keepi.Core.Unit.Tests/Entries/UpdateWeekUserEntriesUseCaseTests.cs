using System.Text;
using Keepi.Core.Entries;
using Keepi.Core.UserEntryCategories;

namespace Keepi.Core.Unit.Tests.Entries;

public class UpdateWeekUserEntriesUseCaseTests
{
  [Fact]
  public async Task Execute_stores_expected_entities()
  {
    var context = new TestContext()
      .WithUserEntryCategories(
        new UserEntryCategoryEntity(id: 1, name: "Dev", enabled: true, activeFrom: null, activeTo: null),
        new UserEntryCategoryEntity(id: 2, name: "Administratie", enabled: true, activeFrom: null, activeTo: null))
      .WithOverwriteUserEntriesResult(
        MaybeErrorResult<OverwriteUserEntriesForDatesError>.CreateSuccess());

    var useCase = context.BuildUseCase();

    var result = await useCase.Execute(
      userId: 42,
      year: 2025,
      weekNumber: 25,
      input: new UpdateWeekUserEntriesUseCaseInput(
        Monday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: [
          new UpdateWeekUserEntriesUseCaseInputDayEntry(EntryCategoryId: 1, Minutes: 60, Remark: "Nieuwe feature"),
        ]),
        Tuesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: [
          new UpdateWeekUserEntriesUseCaseInputDayEntry(EntryCategoryId: 1, Minutes: 60, Remark: "Nieuwe feature"),
          new UpdateWeekUserEntriesUseCaseInputDayEntry(EntryCategoryId: 2, Minutes: 30, Remark: "Project Flyby"),
        ]),
        Wednesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: [
          new UpdateWeekUserEntriesUseCaseInputDayEntry(EntryCategoryId: 2, Minutes: 15, Remark: null),
        ]),
        Thursday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
        Friday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
        Saturday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
        Sunday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: [])),
      cancellationToken: CancellationToken.None);

    result.Succeeded.ShouldBeTrue();

    context.GetUserUserEntryCategoriesMock.Verify(x => x.Execute(
      42,
      It.Is<int[]>(a => a.Length == 2 && a[0] == 1 && a[1] == 2),
      It.IsAny<CancellationToken>()));
    context.OverwriteUserEntriesForDatesMock.Verify(x => x.Execute(
      42,
      It.Is<DateOnly[]>(a =>
        a.Length == 7 &&
        a[0] == new DateOnly(2025, 6, 16) &&
        a[1] == new DateOnly(2025, 6, 17) &&
        a[2] == new DateOnly(2025, 6, 18) &&
        a[3] == new DateOnly(2025, 6, 19) &&
        a[4] == new DateOnly(2025, 6, 20) &&
        a[5] == new DateOnly(2025, 6, 21) &&
        a[6] == new DateOnly(2025, 6, 22)),
      It.Is<UserEntryEntity[]>(a =>
        a.Length == 4 &&
        a[0].UserEntryCategoryId == 1 &&
        a[0].Date == new DateOnly(2025, 6, 16) &&
        a[0].UserId == 42 &&
        a[0].Minutes == 60 &&
        a[0].Remark == "Nieuwe feature" &&
        a[1].UserEntryCategoryId == 1 &&
        a[1].Date == new DateOnly(2025, 6, 17) &&
        a[1].UserId == 42 &&
        a[1].Minutes == 60 &&
        a[1].Remark == "Nieuwe feature" &&
        a[2].UserEntryCategoryId == 2 &&
        a[2].Date == new DateOnly(2025, 6, 17) &&
        a[2].UserId == 42 &&
        a[2].Minutes == 30 &&
        a[2].Remark == "Project Flyby" &&
        a[3].UserEntryCategoryId == 2 &&
        a[3].Date == new DateOnly(2025, 6, 18) &&
        a[3].UserId == 42 &&
        a[3].Minutes == 15 &&
        a[3].Remark == null),
      It.IsAny<CancellationToken>()));
    context.VerifyNoOtherCalls();
  }

  [Fact]
  public async Task Execute_returns_error_for_non_existing_user_entry_category()
  {
    var context = new TestContext()
      .WithUserEntryCategories(
        new UserEntryCategoryEntity(id: 1, name: "Dev", enabled: true, activeFrom: null, activeTo: null));

    var useCase = context.BuildUseCase();

    var result = await useCase.Execute(
      userId: 42,
      year: 2025,
      weekNumber: 25,
      input: new UpdateWeekUserEntriesUseCaseInput(
        Monday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: [
          new UpdateWeekUserEntriesUseCaseInputDayEntry(EntryCategoryId: 2, Minutes: 60, Remark: null),
        ]),
        Tuesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
        Wednesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
        Thursday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
        Friday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
        Saturday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
        Sunday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: [])),
      cancellationToken: CancellationToken.None);

    result.Succeeded.ShouldBeFalse();
    result.ErrorOrNull.ShouldBe(UpdateWeekUserEntriesUseCaseError.UnknownUserEntryCategory);

    context.OverwriteUserEntriesForDatesMock.VerifyNoOtherCalls();
  }

  [Fact]
  public async Task Execute_returns_error_for_disabled_user_entry_category()
  {
    var context = new TestContext()
      .WithUserEntryCategories(
        new UserEntryCategoryEntity(id: 1, name: "Dev", enabled: false, activeFrom: null, activeTo: null));

    var useCase = context.BuildUseCase();

    var result = await useCase.Execute(
      userId: 42,
      year: 2025,
      weekNumber: 25,
      input: new UpdateWeekUserEntriesUseCaseInput(
        Monday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: [
          new UpdateWeekUserEntriesUseCaseInputDayEntry(EntryCategoryId: 1, Minutes: 60, Remark: null),
        ]),
        Tuesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
        Wednesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
        Thursday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
        Friday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
        Saturday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
        Sunday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: [])),
      cancellationToken: CancellationToken.None);

    result.Succeeded.ShouldBeFalse();
    result.ErrorOrNull.ShouldBe(UpdateWeekUserEntriesUseCaseError.InvalidUserEntryCategory);

    context.OverwriteUserEntriesForDatesMock.VerifyNoOtherCalls();
  }

  [Fact]
  public async Task Execute_returns_error_using_existing_user_entry_category_out_of_active_date_range()
  {
    var context = new TestContext()
      .WithUserEntryCategories(
        new UserEntryCategoryEntity(
          id: 1,
          name: "Dev",
          enabled: true,
          activeFrom: new DateOnly(year: 2025, month: 1, day: 1),
          activeTo: new DateOnly(year: 2025, month: 2, day: 28)));

    var useCase = context.BuildUseCase();

    var result = await useCase.Execute(
      userId: 42,
      year: 2025,
      weekNumber: 25,
      input: new UpdateWeekUserEntriesUseCaseInput(
        Monday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: [
          new UpdateWeekUserEntriesUseCaseInputDayEntry(EntryCategoryId: 1, Minutes: 60, Remark: null),
        ]),
        Tuesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
        Wednesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
        Thursday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
        Friday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
        Saturday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
        Sunday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: [])),
      cancellationToken: CancellationToken.None);

    result.Succeeded.ShouldBeFalse();
    result.ErrorOrNull.ShouldBe(UpdateWeekUserEntriesUseCaseError.InvalidUserEntryCategory);

    context.OverwriteUserEntriesForDatesMock.VerifyNoOtherCalls();
  }

  [Fact]
  public async Task Execute_returns_error_for_invalid_minutes()
  {
    var context = new TestContext()
      .WithUserEntryCategories(
        new UserEntryCategoryEntity(id: 1, name: "Dev", enabled: true, activeFrom: null, activeTo: null));

    var useCase = context.BuildUseCase();

    var result = await useCase.Execute(
      userId: 42,
      year: 2025,
      weekNumber: 25,
      input: new UpdateWeekUserEntriesUseCaseInput(
        Monday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: [
          new UpdateWeekUserEntriesUseCaseInputDayEntry(EntryCategoryId: 1, Minutes: -45, Remark: null),
        ]),
        Tuesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
        Wednesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
        Thursday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
        Friday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
        Saturday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
        Sunday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: [])),
      cancellationToken: CancellationToken.None);

    result.Succeeded.ShouldBeFalse();
    result.ErrorOrNull.ShouldBe(UpdateWeekUserEntriesUseCaseError.InvalidMinutes);

    context.OverwriteUserEntriesForDatesMock.VerifyNoOtherCalls();
  }

  [Fact]
  public async Task Execute_returns_error_for_invalid_remark()
  {
    var context = new TestContext()
      .WithUserEntryCategories(
        new UserEntryCategoryEntity(id: 1, name: "Dev", enabled: true, activeFrom: null, activeTo: null));

    var useCase = context.BuildUseCase();

    var result = await useCase.Execute(
      userId: 42,
      year: 2025,
      weekNumber: 25,
      input: new UpdateWeekUserEntriesUseCaseInput(
        Monday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: [
          new UpdateWeekUserEntriesUseCaseInputDayEntry(
            EntryCategoryId: 1,
            Minutes: 60,
            Remark: new StringBuilder().Append(value: 'a', repeatCount: 260).ToString()),
        ]),
        Tuesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
        Wednesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
        Thursday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
        Friday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
        Saturday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
        Sunday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: [])),
      cancellationToken: CancellationToken.None);

    result.Succeeded.ShouldBeFalse();
    result.ErrorOrNull.ShouldBe(UpdateWeekUserEntriesUseCaseError.InvalidRemark);

    context.OverwriteUserEntriesForDatesMock.VerifyNoOtherCalls();
  }

  [Fact]
  public async Task Execute_returns_overwrite_user_entries_error()
  {
    var context = new TestContext()
      .WithUserEntryCategories(
        new UserEntryCategoryEntity(id: 1, name: "Dev", enabled: true, activeFrom: null, activeTo: null))
      .WithOverwriteUserEntriesResult(MaybeErrorResult<OverwriteUserEntriesForDatesError>.CreateFailure(OverwriteUserEntriesForDatesError.Unknown));

    var useCase = context.BuildUseCase();

    var result = await useCase.Execute(
      userId: 42,
      year: 2025,
      weekNumber: 25,
      input: new UpdateWeekUserEntriesUseCaseInput(
        Monday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: [
          new UpdateWeekUserEntriesUseCaseInputDayEntry(
            EntryCategoryId: 1,
            Minutes: 60,
            Remark: null),
        ]),
        Tuesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
        Wednesday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
        Thursday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
        Friday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
        Saturday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: []),
        Sunday: new UpdateWeekUserEntriesUseCaseInputDay(Entries: [])),
      cancellationToken: CancellationToken.None);

    result.Succeeded.ShouldBeFalse();
    result.ErrorOrNull.ShouldBe(UpdateWeekUserEntriesUseCaseError.Unknown);
  }

  class TestContext
  {
    public Mock<IGetUserUserEntryCategories> GetUserUserEntryCategoriesMock { get; } = new(MockBehavior.Strict);
    public Mock<IOverwriteUserEntriesForDates> OverwriteUserEntriesForDatesMock { get; } = new(MockBehavior.Strict);

    public TestContext WithUserEntryCategories(params UserEntryCategoryEntity[] entities)
    {
      GetUserUserEntryCategoriesMock
        .Setup(x => x.Execute(It.IsAny<int>(), It.IsAny<int[]>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(entities);

      return this;
    }

    public TestContext WithOverwriteUserEntriesResult(IMaybeErrorResult<OverwriteUserEntriesForDatesError> result)
    {
      OverwriteUserEntriesForDatesMock
        .Setup(x => x.Execute(It.IsAny<int>(), It.IsAny<DateOnly[]>(), It.IsAny<UserEntryEntity[]>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(result);

      return this;
    }

    public UpdateWeekUserEntriesUseCase BuildUseCase() => new(
      getUserUserEntryCategories: GetUserUserEntryCategoriesMock.Object,
      overwriteUserEntriesForDates: OverwriteUserEntriesForDatesMock.Object);

    public void VerifyNoOtherCalls()
    {
      GetUserUserEntryCategoriesMock.VerifyNoOtherCalls();
      OverwriteUserEntriesForDatesMock.VerifyNoOtherCalls();
    }
  }
}