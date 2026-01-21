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
    UnauthenticatedUser,
    UnauthorizedUser,
}

internal sealed class GetAllUsersUseCase(IResolveUser resolveUser, IGetUsers getUsers)
    : IGetAllUsersUseCase
{
    public async Task<IValueOrErrorResult<GetUsersResult, GetAllUsersUseCaseError>> Execute(
        CancellationToken cancellationToken
    )
    {
        var userResult = await resolveUser.Execute(cancellationToken: cancellationToken);
        if (!userResult.TrySuccess(out var userSuccessResult, out var userErrorResult))
        {
            return userErrorResult switch
            {
                ResolveUserError.UserNotAuthenticated => Result.Failure<
                    GetUsersResult,
                    GetAllUsersUseCaseError
                >(GetAllUsersUseCaseError.UnauthenticatedUser),
                _ => Result.Failure<GetUsersResult, GetAllUsersUseCaseError>(
                    GetAllUsersUseCaseError.Unknown
                ),
            };
        }
        if (!userSuccessResult.UsersPermission.CanRead())
        {
            return Result.Failure<GetUsersResult, GetAllUsersUseCaseError>(
                GetAllUsersUseCaseError.UnauthorizedUser
            );
        }

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
