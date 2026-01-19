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
}

internal sealed class GetAllProjectsUseCase(IGetProjects getProjects) : IGetAllProjectsUseCase
{
    public async Task<IValueOrErrorResult<GetProjectsResult, GetAllProjectsUseCaseError>> Execute(
        CancellationToken cancellationToken
    )
    {
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
