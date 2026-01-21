namespace Keepi.Core.Users;

public interface IGetUserUseCase
{
    Task<IValueOrErrorResult<GetUserUseCaseOutput, GetUserUseCaseError>> Execute(
        CancellationToken cancellationToken
    );
}

public enum GetUserUseCaseError
{
    Unknown,
    UnauthenticatedUser,
}

public sealed record GetUserUseCaseOutput(
    int Id,
    string Name,
    string EmailAddress,
    UserPermission EntriesPermission,
    UserPermission ExportsPermission,
    UserPermission ProjectsPermission,
    UserPermission UsersPermission
);

internal sealed class GetUserUseCase(IResolveUser resolveUser) : IGetUserUseCase
{
    public async Task<IValueOrErrorResult<GetUserUseCaseOutput, GetUserUseCaseError>> Execute(
        CancellationToken cancellationToken
    )
    {
        var result = await resolveUser.Execute(cancellationToken: cancellationToken);
        if (!result.TrySuccess(out var successResult, out var errorResult))
        {
            return errorResult switch
            {
                ResolveUserError.UserNotAuthenticated => Result.Failure<
                    GetUserUseCaseOutput,
                    GetUserUseCaseError
                >(GetUserUseCaseError.UnauthenticatedUser),
                _ => Result.Failure<GetUserUseCaseOutput, GetUserUseCaseError>(
                    GetUserUseCaseError.Unknown
                ),
            };
        }

        return Result.Success<GetUserUseCaseOutput, GetUserUseCaseError>(
            new(
                Id: successResult.Id,
                Name: successResult.Name,
                EmailAddress: successResult.EmailAddress,
                EntriesPermission: successResult.EntriesPermission,
                ExportsPermission: successResult.ExportsPermission,
                ProjectsPermission: successResult.ProjectsPermission,
                UsersPermission: successResult.UsersPermission
            )
        );
    }
}
