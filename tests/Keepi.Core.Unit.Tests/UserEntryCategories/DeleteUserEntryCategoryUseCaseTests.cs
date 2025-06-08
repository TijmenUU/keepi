using Keepi.Core.UserEntryCategories;

namespace Keepi.Core.Unit.Tests.UserEntryCategories;

public class DeleteUserEntryCategoryUseCaseTests
{
    [Fact]
    public async Task Execute_deletes_existing_entry_category()
    {
        var context = new TestContext().WithSuccesfulUserEntryCategoryDeletion();

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            userEntryCategoryId: 42,
            userId: 981,
            cancellationToken: CancellationToken.None
        );

        result.Succeeded.ShouldBeTrue();

        context.DeleteUserEntryCategoryMock.Verify(x =>
            x.Execute(42, 981, It.IsAny<CancellationToken>())
        );
    }

    [Fact]
    public async Task Execute_returns_expected_error_for_non_existing_entity()
    {
        var context = new TestContext().WithFailedUserEntryCategoryDeletion(
            DeleteUserEntryCategoryError.UserEntryCategoryDoesNotExist
        );

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            userEntryCategoryId: 42,
            userId: 981,
            cancellationToken: CancellationToken.None
        );

        result.Succeeded.ShouldBeFalse();
        result.ErrorOrNull.ShouldBe(DeleteUserEntryCategoryUseCaseError.UnknownUserEntryCategory);

        context.DeleteUserEntryCategoryMock.Verify(x =>
            x.Execute(42, 981, It.IsAny<CancellationToken>())
        );
    }

    [Fact]
    public async Task Execute_returns_expected_error_when_trying_to_delete_entry_category_of_other_user()
    {
        var context = new TestContext().WithFailedUserEntryCategoryDeletion(
            DeleteUserEntryCategoryError.UserEntryCategoryBelongsToOtherUser
        );

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            userEntryCategoryId: 42,
            userId: 981,
            cancellationToken: CancellationToken.None
        );

        result.Succeeded.ShouldBeFalse();
        result.ErrorOrNull.ShouldBe(DeleteUserEntryCategoryUseCaseError.UnknownUserEntryCategory);

        context.DeleteUserEntryCategoryMock.Verify(x =>
            x.Execute(42, 981, It.IsAny<CancellationToken>())
        );
    }

    [Fact]
    public async Task Execute_returns_unknown_error_for_unknown_failure()
    {
        var context = new TestContext().WithFailedUserEntryCategoryDeletion(
            DeleteUserEntryCategoryError.Unknown
        );

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            userEntryCategoryId: 42,
            userId: 981,
            cancellationToken: CancellationToken.None
        );

        result.Succeeded.ShouldBeFalse();
        result.ErrorOrNull.ShouldBe(DeleteUserEntryCategoryUseCaseError.Unknown);

        context.DeleteUserEntryCategoryMock.Verify(x =>
            x.Execute(42, 981, It.IsAny<CancellationToken>())
        );
    }

    class TestContext
    {
        public Mock<IDeleteUserEntryCategory> DeleteUserEntryCategoryMock { get; } =
            new(MockBehavior.Strict);

        public TestContext WithSuccesfulUserEntryCategoryDeletion()
        {
            DeleteUserEntryCategoryMock
                .Setup(x =>
                    x.Execute(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(MaybeErrorResult<DeleteUserEntryCategoryError>.CreateSuccess());

            return this;
        }

        public TestContext WithFailedUserEntryCategoryDeletion(DeleteUserEntryCategoryError error)
        {
            DeleteUserEntryCategoryMock
                .Setup(x =>
                    x.Execute(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(MaybeErrorResult<DeleteUserEntryCategoryError>.CreateFailure(error));

            return this;
        }

        public DeleteUserEntryCategoryUseCase BuildUseCase() =>
            new(deleteUserEntryCategory: DeleteUserEntryCategoryMock.Object);
    }
}
