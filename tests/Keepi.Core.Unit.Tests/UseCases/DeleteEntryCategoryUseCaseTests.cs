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

  [Fact]
  public async Task Execute_returns_expected_error_for_non_existing_entity()
  {
    var context = new TestContext()
      .WithFailedEntryCategoryDeletion(DeleteEntryCategoryError.EntryCategoryDoesNotExist);

    var useCase = context.BuildUseCase();

    var result = await useCase.Execute(
      entryCategoryId: 42,
      userId: 981,
      cancellationToken: CancellationToken.None);

    result.Succeeded.ShouldBeFalse();
    result.ErrorOrNull.ShouldBe(DeleteEntryCategoryUseCaseError.UnknownEntryCategory);

    context.DeleteEntryCategoryMock.Verify(x => x.Execute(
      42,
      981,
      It.IsAny<CancellationToken>()));
  }

  [Fact]
  public async Task Execute_returns_expected_error_when_trying_to_delete_entry_category_of_other_user()
  {
    var context = new TestContext()
      .WithFailedEntryCategoryDeletion(DeleteEntryCategoryError.EntryCategoryBelongsToOtherUser);

    var useCase = context.BuildUseCase();

    var result = await useCase.Execute(
      entryCategoryId: 42,
      userId: 981,
      cancellationToken: CancellationToken.None);

    result.Succeeded.ShouldBeFalse();
    result.ErrorOrNull.ShouldBe(DeleteEntryCategoryUseCaseError.UnknownEntryCategory);

    context.DeleteEntryCategoryMock.Verify(x => x.Execute(
      42,
      981,
      It.IsAny<CancellationToken>()));
  }

  [Fact]
  public async Task Execute_returns_unknown_error_for_unknown_failure()
  {
    var context = new TestContext()
      .WithFailedEntryCategoryDeletion(DeleteEntryCategoryError.Unknown);

    var useCase = context.BuildUseCase();

    var result = await useCase.Execute(
      entryCategoryId: 42,
      userId: 981,
      cancellationToken: CancellationToken.None);

    result.Succeeded.ShouldBeFalse();
    result.ErrorOrNull.ShouldBe(DeleteEntryCategoryUseCaseError.Unknown);

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

    public TestContext WithFailedEntryCategoryDeletion(DeleteEntryCategoryError error)
    {
      DeleteEntryCategoryMock
        .Setup(x => x.Execute(
          It.IsAny<int>(),
          It.IsAny<int>(),
          It.IsAny<CancellationToken>()))
        .ReturnsAsync(MaybeErrorResult<DeleteEntryCategoryError>.CreateFailure(error));

      return this;
    }

    public DeleteEntryCategoryUseCase BuildUseCase()
      => new(deleteEntryCategory: DeleteEntryCategoryMock.Object);
  }
}