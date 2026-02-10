using System.Threading;
using System.Threading.Tasks;
using Keepi.Core;
using Keepi.Core.Users;

namespace Keepi.App.Authorization;

internal sealed class ResolveUser(
    IGetUserName getUserName,
    IGetOrRegisterNewUserUseCase getOrRegisterNewUserUseCase
) : IResolveUser
{
    private const UserIdentityProvider identityProvider = UserIdentityProvider.LocalApplication;

    private readonly SemaphoreSlim getUserSemaphore = new(initialCount: 1, maxCount: 1);

    private bool hasCachedUser = false;
    private GetUserResult? cachedUser = null;

    public async Task<IValueOrErrorResult<ResolvedUser, ResolveUserError>> Execute(
        CancellationToken cancellationToken
    )
    {
        if (!hasCachedUser)
        {
            await getUserSemaphore.WaitAsync(cancellationToken: cancellationToken);
            try
            {
                if (!hasCachedUser)
                {
                    var result = await InternalGetUserOrNull(cancellationToken: cancellationToken);
                    if (result.TrySuccess(out var successResult, out var errorResult))
                    {
                        cachedUser = successResult.User;
                        hasCachedUser = true;
                    }
                    else
                    {
                        return Result.Failure<ResolvedUser, ResolveUserError>(errorResult);
                    }
                }
            }
            finally
            {
                getUserSemaphore.Release();
            }
        }

        // This should never happen but since the boolean and data are separate
        // fields it is technically possible.
        if (cachedUser == null)
        {
            return Result.Failure<ResolvedUser, ResolveUserError>(ResolveUserError.UserNotFound);
        }

        if (cachedUser.IdentityOrigin != UserIdentityProvider.LocalApplication)
        {
            return Result.Failure<ResolvedUser, ResolveUserError>(
                ResolveUserError.UnexpectedNonLocalApplicationUser
            );
        }

        return Result.Success<ResolvedUser, ResolveUserError>(
            new ResolvedUser(
                Id: cachedUser.Id,
                Name: cachedUser.Name,
                EmailAddress: cachedUser.EmailAddress,
                EntriesPermission: cachedUser.EntriesPermission,
                ExportsPermission: cachedUser.ExportsPermission,
                ProjectsPermission: cachedUser.ProjectsPermission,
                UsersPermission: cachedUser.UsersPermission
            )
        );
    }

    private async Task<
        IValueOrErrorResult<GetOrRegisterNewUserUseCaseOutput, ResolveUserError>
    > InternalGetUserOrNull(CancellationToken cancellationToken)
    {
        var getUserInfoResult = GetUserInfo();
        if (!getUserInfoResult.TrySuccess(out var userInfo, out var getUserInfoError))
        {
            return Result.Failure<GetOrRegisterNewUserUseCaseOutput, ResolveUserError>(
                getUserInfoError
            );
        }

        var result = await getOrRegisterNewUserUseCase.Execute(
            externalId: userInfo.Name,
            emailAddress: GetFirstAdminUserEmailAddress.LocalUserEmailAddress,
            name: userInfo.Name,
            identityProvider: identityProvider,
            cancellationToken: cancellationToken
        );

        if (result.TrySuccess(out var successResult, out var errorResult))
        {
            return Result.Success<GetOrRegisterNewUserUseCaseOutput, ResolveUserError>(
                successResult
            );
        }

        return errorResult switch
        {
            GetOrRegisterNewUserUseCaseError.RegistrationFailed => Result.Failure<
                GetOrRegisterNewUserUseCaseOutput,
                ResolveUserError
            >(ResolveUserError.UserRegistrationFailed),
            _ => Result.Failure<GetOrRegisterNewUserUseCaseOutput, ResolveUserError>(
                ResolveUserError.Unknown
            ),
        };
    }

    private IValueOrErrorResult<UserInfo, ResolveUserError> GetUserInfo()
    {
        var userName = getUserName.Execute();
        if (string.IsNullOrEmpty(userName))
        {
            return Result.Failure<UserInfo, ResolveUserError>(
                ResolveUserError.UserNotAuthenticated
            );
        }

        return Result.Success<UserInfo, ResolveUserError>(new(Name: userName));
    }

    private sealed record UserInfo(string Name);
}
