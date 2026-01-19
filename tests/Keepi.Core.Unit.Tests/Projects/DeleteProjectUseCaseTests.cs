using Keepi.Core.Projects;

namespace Keepi.Core.Unit.Tests.Projects;

public class DeleteProjectUseCaseTests
{
    [Fact]
    public async Task Execute_returns_success()
    {
        var context = new TestContext().WithSuccessfulDeleteProject();

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            projectId: 42,
            cancellationToken: CancellationToken.None
        );
        result.TrySuccess(out _).ShouldBeTrue();

        context.DeleteProjectMock.Verify(x => x.Execute(42, It.IsAny<CancellationToken>()));
        context.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(DeleteProjectError.Unknown, DeleteProjectUseCaseError.Unknown)]
    [InlineData(DeleteProjectError.UnknownProjectId, DeleteProjectUseCaseError.UnknownProjectId)]
    public async Task Execute_returns_errors_raised_by_repository_call(
        DeleteProjectError repositoryError,
        DeleteProjectUseCaseError expectedError
    )
    {
        var context = new TestContext().WithDeleteProjectFailure(repositoryError);

        var useCase = context.BuildUseCase();

        var result = await useCase.Execute(
            projectId: 42,
            cancellationToken: CancellationToken.None
        );
        result.TrySuccess(out var errorResult).ShouldBeFalse();

        errorResult.ShouldBe(expectedError);

        context.DeleteProjectMock.Verify(x => x.Execute(42, It.IsAny<CancellationToken>()));
        context.VerifyNoOtherCalls();
    }

    private class TestContext
    {
        public Mock<IDeleteProject> DeleteProjectMock { get; } = new(MockBehavior.Strict);

        public TestContext WithSuccessfulDeleteProject()
        {
            DeleteProjectMock
                .Setup(x => x.Execute(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success<DeleteProjectError>());

            return this;
        }

        public TestContext WithDeleteProjectFailure(DeleteProjectError error)
        {
            DeleteProjectMock
                .Setup(x => x.Execute(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure(error));

            return this;
        }

        public DeleteProjectUseCase BuildUseCase() => new(deleteProject: DeleteProjectMock.Object);

        public void VerifyNoOtherCalls()
        {
            DeleteProjectMock.VerifyNoOtherCalls();
        }
    }
}
