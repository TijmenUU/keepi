namespace Keepi.Core.Projects;

public interface IDeleteProjectUseCase
{
    Task<IMaybeErrorResult<DeleteProjectUseCaseError>> Execute(
        int projectId,
        CancellationToken cancellationToken
    );
}

public enum DeleteProjectUseCaseError
{
    Unknown = 0,
    UnknownProjectId,
}

internal sealed class DeleteProjectUseCase(IDeleteProject deleteProject) : IDeleteProjectUseCase
{
    public async Task<IMaybeErrorResult<DeleteProjectUseCaseError>> Execute(
        int projectId,
        CancellationToken cancellationToken
    )
    {
        var result = await deleteProject.Execute(
            projectId: projectId,
            cancellationToken: cancellationToken
        );
        if (result.TrySuccess(out var errorResult))
        {
            return Result.Success<DeleteProjectUseCaseError>();
        }

        return errorResult switch
        {
            DeleteProjectError.UnknownProjectId => Result.Failure(
                DeleteProjectUseCaseError.UnknownProjectId
            ),
            _ => Result.Failure(DeleteProjectUseCaseError.Unknown),
        };
    }
}
