namespace Keepi.Core.Users;

public interface IGetAllUsersUseCase
{
    Task<IValueOrErrorResult<GetUsersResult, GetAllUsersUseCaseError>> Execute(
        CancellationToken cancellationToken
    );
}

public enum GetAllUsersUseCaseError
{
    Unknown,
}

internal sealed class GetAllUsersUseCase(IGetUsers getUsers) : IGetAllUsersUseCase
{
    public async Task<IValueOrErrorResult<GetUsersResult, GetAllUsersUseCaseError>> Execute(
        CancellationToken cancellationToken
    )
    {
        var result = await getUsers.Execute(cancellationToken: cancellationToken);
        if (result.TrySuccess(out var successResult, out var errorResult))
        {
            return Result.Success<GetUsersResult, GetAllUsersUseCaseError>(successResult);
        }

        return errorResult switch
        {
            _ => Result.Failure<GetUsersResult, GetAllUsersUseCaseError>(
                GetAllUsersUseCaseError.Unknown
            ),
        };
    }
}
