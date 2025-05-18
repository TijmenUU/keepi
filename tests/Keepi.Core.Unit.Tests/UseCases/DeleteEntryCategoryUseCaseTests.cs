using Keepi.Core.Repositories;
using Keepi.Core.UseCases;

namespace Keepi.Core.Unit.Tests.UseCases;

public class DeleteEntryCategoryUseCaseTests
{
  [Fact]
  public async Task Execute_deletes_existing_entry_category()
  {
    var context = new TestContext()
      .WithSuccesfulEntryCategoryDeletion();

    var useCase = context.BuildUseCase();

    var result = await useCase.Execute(
      entryCategoryId: 42,
      userId: 981,
      cancellationToken: CancellationToken.None);

    result.Succeeded.ShouldBeTrue();

    context.DeleteEntryCategoryMock.Verify(x => x.Execute(
      42,
      981,
      It.IsAny<CancellationToken>()));
  }

  class TestContext
  {
    public Mock<IDeleteEntryCategory> DeleteEntryCategoryMock { get; } = new(MockBehavior.Strict);

    public TestContext WithSuccesfulEntryCategoryDeletion()
    {
      DeleteEntryCategoryMock
        .Setup(x => x.Execute(
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<CancellationToken>()))
        .ReturnsAsync(MaybeErrorResult<DeleteEntryCategoryError>.CreateSuccess());

      return this;
    }

    public DeleteEntryCategoryUseCase BuildUseCase()
      => new(deleteEntryCategory: DeleteEntryCategoryMock.Object);
  }
}