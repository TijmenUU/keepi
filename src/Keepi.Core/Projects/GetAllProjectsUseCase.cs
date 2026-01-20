using Keepi.Core.Users;

namespace Keepi.Core.Projects;

public interface IGetAllProjectsUseCase
{
    Task<IValueOrErrorResult<GetProjectsResult, GetAllProjectsUseCaseError>> Execute(
        CancellationToken cancellationToken
    );
}

public enum GetAllProjectsUseCaseError
{
    Unknown = 0,
    UnauthenticatedUser,
}

internal sealed class GetAllProjectsUseCase(IResolveUser resolveUser, IGetProjects getProjects)
    : IGetAllProjectsUseCase
{
    public async Task<IValueOrErrorResult<GetProjectsResult, GetAllProjectsUseCaseError>> Execute(
        CancellationToken cancellationToken
    )
    {
        var userResult = await resolveUser.Execute(cancellationToken: cancellationToken);
        if (!userResult.TrySuccess(out _, out var userErrorResult))
        {
            return userErrorResult switch
            {
                ResolveUserError.UserNotAuthenticated => Result.Failure<
                    GetProjectsResult,
                    GetAllProjectsUseCaseError
                >(GetAllProjectsUseCaseError.UnauthenticatedUser),
                _ => Result.Failure<GetProjectsResult, GetAllProjectsUseCaseError>(
                    GetAllProjectsUseCaseError.Unknown
                ),
            };
        }

        var result = await getProjects.Execute(cancellationToken: cancellationToken);
        if (result.TrySuccess(out var successResult, out var errorResult))
        {
            return Result.Success<GetProjectsResult, GetAllProjectsUseCaseError>(successResult);
        }

        return errorResult switch
        {
            _ => Result.Failure<GetProjectsResult, GetAllProjectsUseCaseError>(
                GetAllProjectsUseCaseError.Unknown
            ),
        };
    }
}
