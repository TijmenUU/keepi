using Keepi.Core.Users;

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
    UnauthenticatedUser,
    UnknownProjectId,
}

internal sealed class DeleteProjectUseCase(IResolveUser resolveUser, IDeleteProject deleteProject)
    : IDeleteProjectUseCase
{
    public async Task<IMaybeErrorResult<DeleteProjectUseCaseError>> Execute(
        int projectId,
        CancellationToken cancellationToken
    )
    {
        var userResult = await resolveUser.Execute(cancellationToken: cancellationToken);
        if (!userResult.TrySuccess(out _, out var userErrorResult))
        {
            return userErrorResult switch
            {
                ResolveUserError.UserNotAuthenticated => Result.Failure(
                    DeleteProjectUseCaseError.UnauthenticatedUser
                ),
                _ => Result.Failure(DeleteProjectUseCaseError.Unknown),
            };
        }

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
