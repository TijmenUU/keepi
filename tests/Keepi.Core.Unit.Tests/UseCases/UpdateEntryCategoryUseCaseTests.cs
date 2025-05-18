using System.Text;
using Keepi.Core.Repositories;
using Keepi.Core.UseCases;

namespace Keepi.Core.Unit.Tests.UseCases;

public class UpdateEntryCategoryUseCaseTests
{
  [Fact]
  public async Task Execute_stores_updated_entry_category()
  {
    var context = new TestContext()
      .WithSuccesfulEntryCategoryUpdate();

    var useCase = context.BuildUseCase();

    var result = await useCase.Execute(
      entryCategoryId: 42,
      userId: 981,
      name: "Development",
      enabled: true,
      activeFrom: new DateOnly(2020, 12, 31),
      activeTo: new DateOnly(2030, 12, 31),
      cancellationToken: CancellationToken.None);

    result.Succeeded.ShouldBeTrue();

    context.UpdateEntryCategoryMock.Verify(x => x.Execute(
      42,
      981,
      "Development",
      true,
      new DateOnly(2020, 12, 31),
      new DateOnly(2030, 12, 31),
      It.IsAny<CancellationToken>()));
  }

  [Fact]
  public async Task Execute_returns_expected_error_for_non_existing_entity()
  {
    var context = new TestContext()
      .WithFailedEntryCategoryUpdate(UpdateEntryCategoryError.EntryCategoryDoesNotExist);

    var useCase = context.BuildUseCase();

    var result = await useCase.Execute(
      entryCategoryId: 42,
      userId: 981,
      name: "Development",
      enabled: true,
      activeFrom: new DateOnly(2020, 12, 31),
      activeTo: new DateOnly(2030, 12, 31),
      cancellationToken: CancellationToken.None);

    result.Succeeded.ShouldBeFalse();
    result.ErrorOrNull.ShouldBe(UpdateEntryCategoryUseCaseError.UnknownEntryCategory);

    context.UpdateEntryCategoryMock.Verify(x => x.Execute(
      42,
      981,
      "Development",
      true,
      new DateOnly(2020, 12, 31),
      new DateOnly(2030, 12, 31),
      It.IsAny<CancellationToken>()));
  }

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData(" ")]
  public async Task Execute_returns_expected_error_for_empty_or_null_name(string? name)
  {
    var context = new TestContext();

    var useCase = context.BuildUseCase();

    var result = await useCase.Execute(
      entryCategoryId: 42,
      userId: 981,
      name: name,
      enabled: true,
      activeFrom: new DateOnly(2020, 12, 31),
      activeTo: new DateOnly(2030, 12, 31),
      cancellationToken: CancellationToken.None);

    result.Succeeded.ShouldBeFalse();
    result.ErrorOrNull.ShouldBe(UpdateEntryCategoryUseCaseError.MalformedName);

    context.UpdateEntryCategoryMock.VerifyNoOtherCalls();
  }

  [Fact]
  public async Task Execute_returns_expected_error_for_too_long_name()
  {
    var context = new TestContext();

    var useCase = context.BuildUseCase();

    var result = await useCase.Execute(
      entryCategoryId: 42,
      userId: 981,
      name: new StringBuilder().Append('a', repeatCount: 65).ToString(),
      enabled: true,
      activeFrom: new DateOnly(2020, 12, 31),
      activeTo: new DateOnly(2030, 12, 31),
      cancellationToken: CancellationToken.None);

    result.Succeeded.ShouldBeFalse();
    result.ErrorOrNull.ShouldBe(UpdateEntryCategoryUseCaseError.MalformedName);

    context.UpdateEntryCategoryMock.VerifyNoOtherCalls();
  }

  [Fact]
  public async Task Execute_returns_expected_error_for_invalid_range()
  {
    var context = new TestContext();

    var useCase = context.BuildUseCase();

    var result = await useCase.Execute(
      entryCategoryId: 42,
      userId: 981,
      name: "Development",
      enabled: true,
      activeFrom: new DateOnly(2030, 12, 31),
      activeTo: new DateOnly(2020, 12, 31),
      cancellationToken: CancellationToken.None);

    result.Succeeded.ShouldBeFalse();
    result.ErrorOrNull.ShouldBe(UpdateEntryCategoryUseCaseError.InvalidActiveDateRange);

    context.UpdateEntryCategoryMock.VerifyNoOtherCalls();
  }

  [Fact]
  public async Task Execute_returns_expected_error_for_entity_that_belongs_to_other_user()
  {
    var context = new TestContext()
      .WithFailedEntryCategoryUpdate(UpdateEntryCategoryError.EntryCategoryBelongsToOtherUser);

    var useCase = context.BuildUseCase();

    var result = await useCase.Execute(
      entryCategoryId: 42,
      userId: 981,
      name: "Development",
      enabled: true,
      activeFrom: new DateOnly(2020, 12, 31),
      activeTo: new DateOnly(2030, 12, 31),
      cancellationToken: CancellationToken.None);

    result.Succeeded.ShouldBeFalse();
    result.ErrorOrNull.ShouldBe(UpdateEntryCategoryUseCaseError.UnknownEntryCategory);

    context.UpdateEntryCategoryMock.Verify(x => x.Execute(
      42,
      981,
      "Development",
      true,
      new DateOnly(2020, 12, 31),
      new DateOnly(2030, 12, 31),
      It.IsAny<CancellationToken>()));
  }

  [Fact]
  public async Task Execute_returns_expected_error_for_duplicate_name()
  {
    var context = new TestContext()
      .WithFailedEntryCategoryUpdate(UpdateEntryCategoryError.DuplicateName);

    var useCase = context.BuildUseCase();

    var result = await useCase.Execute(
      entryCategoryId: 42,
      userId: 981,
      name: "Development",
      enabled: true,
      activeFrom: new DateOnly(2020, 12, 31),
      activeTo: new DateOnly(2030, 12, 31),
      cancellationToken: CancellationToken.None);

    result.Succeeded.ShouldBeFalse();
    result.ErrorOrNull.ShouldBe(UpdateEntryCategoryUseCaseError.DuplicateName);

    context.UpdateEntryCategoryMock.Verify(x => x.Execute(
      42,
      981,
      "Development",
      true,
      new DateOnly(2020, 12, 31),
      new DateOnly(2030, 12, 31),
      It.IsAny<CancellationToken>()));
  }

  class TestContext
  {
    public Mock<IUpdateEntryCategory> UpdateEntryCategoryMock { get; } = new(MockBehavior.Strict);

    public TestContext WithSuccesfulEntryCategoryUpdate()
    {
      UpdateEntryCategoryMock
        .Setup(x => x.Execute(
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<bool>(),
          It.IsAny<DateOnly?>(),
          It.IsAny<DateOnly?>(),
          It.IsAny<CancellationToken>()))
        .ReturnsAsync(MaybeErrorResult<UpdateEntryCategoryError>.CreateSuccess());

      return this;
    }

    public TestContext WithFailedEntryCategoryUpdate(UpdateEntryCategoryError error)
    {
      UpdateEntryCategoryMock
        .Setup(x => x.Execute(
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<bool>(),
          It.IsAny<DateOnly?>(),
          It.IsAny<DateOnly?>(),
          It.IsAny<CancellationToken>()))
        .ReturnsAsync(MaybeErrorResult<UpdateEntryCategoryError>.CreateFailure(error));

      return this;
    }

    public UpdateEntryCategoryUseCase BuildUseCase()
      => new(updateEntryCategory: UpdateEntryCategoryMock.Object);
  }
}