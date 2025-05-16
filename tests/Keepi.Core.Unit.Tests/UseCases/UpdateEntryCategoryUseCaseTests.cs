using Keepi.Core.Repositories;
using Keepi.Core.UseCases;

namespace Keepi.Core.Unit.Tests.UseCases;

public class UpdateEntryCategoryUseCaseTests
{
  [Fact]
  public async Task Execute_stores_updated_entry_category()
  {
    var context = new TestContext()
      .WithSuccesfulEntryCategoryStore();

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

  class TestContext
  {
    public Mock<IUpdateEntryCategory> UpdateEntryCategoryMock { get; } = new(MockBehavior.Strict);

    public TestContext WithSuccesfulEntryCategoryStore()
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

    public UpdateEntryCategoryUseCase BuildUseCase()
      => new(updateEntryCategory: UpdateEntryCategoryMock.Object);
  }
}