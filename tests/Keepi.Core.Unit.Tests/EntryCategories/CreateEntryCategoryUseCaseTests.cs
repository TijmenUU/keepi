using Keepi.Core.EntryCategories;

namespace Keepi.Core.Unit.Tests.EntryCategories;

public class CreateEntryCategoryUseCaseTests
{
  [Fact]
  public async Task Execute_stores_expected_entity_for_valid_input()
  {
    var context = new TestContext()
      .WithSuccesfulEntryCategoryStore(new EntryCategoryEntity(
          id: 13,
          name: "Dev",
          enabled: true,
          activeFrom: new DateOnly(2024, 4, 25),
          activeTo: new DateOnly(2024, 12, 31)));

    var useCase = context.BuildUseCase();

    var result = await useCase.Execute(
      userId: 42,
      name: "Dev",
      enabled: true,
      activeFrom: new DateOnly(2024, 4, 25),
      activeTo: new DateOnly(2024, 12, 31),
      cancellationToken: CancellationToken.None);

    result.Succeeded.ShouldBeTrue();
    result.SuccessOrThrow.EntryCategoryId.ShouldBe(13);

    context.StoreEntryCategoryMock.Verify(x => x.Execute(
      42,
      "Dev",
      true,
      new DateOnly(2024, 4, 25),
      new DateOnly(2024, 12, 31),
      It.IsAny<CancellationToken>()));
  }

  [Fact]
  public async Task Execute_returns_expected_error_for_duplicate_name()
  {
    var context = new TestContext()
      .WithFailedEntryCategoryStore(StoreEntryCategoryError.DuplicateName);

    var useCase = context.BuildUseCase();

    var result = await useCase.Execute(
      userId: 42,
      name: "Dev",
      enabled: true,
      activeFrom: new DateOnly(2024, 4, 25),
      activeTo: new DateOnly(2024, 12, 31),
      cancellationToken: CancellationToken.None);

    result.Succeeded.ShouldBeFalse();
    result.ErrorOrNull.ShouldBe(CreateEntryCategoryUseCaseError.DuplicateName);

    context.StoreEntryCategoryMock.Verify(x => x.Execute(
      42,
      "Dev",
      true,
      new DateOnly(2024, 4, 25),
      new DateOnly(2024, 12, 31),
      It.IsAny<CancellationToken>()));
  }

  [Fact]
  public async Task Execute_returns_expected_error_for_unknown_failure()
  {
    var context = new TestContext()
      .WithFailedEntryCategoryStore(StoreEntryCategoryError.Unknown);

    var useCase = context.BuildUseCase();

    var result = await useCase.Execute(
      userId: 42,
      name: "Dev",
      enabled: true,
      activeFrom: new DateOnly(2024, 4, 25),
      activeTo: new DateOnly(2024, 12, 31),
      cancellationToken: CancellationToken.None);

    result.Succeeded.ShouldBeFalse();
    result.ErrorOrNull.ShouldBe(CreateEntryCategoryUseCaseError.Unknown);

    context.StoreEntryCategoryMock.Verify(x => x.Execute(
      42,
      "Dev",
      true,
      new DateOnly(2024, 4, 25),
      new DateOnly(2024, 12, 31),
      It.IsAny<CancellationToken>()));
  }

  class TestContext
  {
    public Mock<IStoreEntryCategory> StoreEntryCategoryMock { get; } = new(MockBehavior.Strict);

    public TestContext WithSuccesfulEntryCategoryStore(EntryCategoryEntity returnedEntity)
    {
      StoreEntryCategoryMock
        .Setup(x => x.Execute(
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<bool>(),
          It.IsAny<DateOnly?>(),
          It.IsAny<DateOnly?>(),
          It.IsAny<CancellationToken>()))
        .ReturnsAsync(ValueOrErrorResult<EntryCategoryEntity, StoreEntryCategoryError>.CreateSuccess(returnedEntity));

      return this;
    }

    public TestContext WithFailedEntryCategoryStore(StoreEntryCategoryError error)
    {
      StoreEntryCategoryMock
        .Setup(x => x.Execute(
          It.IsAny<int>(),
          It.IsAny<string>(),
          It.IsAny<bool>(),
          It.IsAny<DateOnly?>(),
          It.IsAny<DateOnly?>(),
          It.IsAny<CancellationToken>()))
        .ReturnsAsync(ValueOrErrorResult<EntryCategoryEntity, StoreEntryCategoryError>.CreateFailure(error));

      return this;
    }

    public CreateEntryCategoryUseCase BuildUseCase()
      => new(storeEntryCategory: StoreEntryCategoryMock.Object);
  }
}