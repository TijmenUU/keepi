using System.Security.Claims;
using Keepi.Core.Users;
using Microsoft.Extensions.Logging;

namespace Keepi.Api.Authorization;

public interface IResolveUserHelper
{
    Task<ResolvedUser?> GetUserOrNull(
        ClaimsPrincipal userClaimsPrincipal,
        CancellationToken cancellationToken
    );
}

public sealed record ResolvedUser(int Id, string Name, string EmailAddress);

internal sealed class ResolveUserHelper(
    IGetUser getUser,
    IRegisterUserUseCase registerUserUseCase,
    ILogger<ResolveUserHelper> logger
) : IResolveUserHelper
{
    private const UserIdentityProvider identityProvider = UserIdentityProvider.GitHub;

    private readonly SemaphoreSlim getUserSemaphore = new(initialCount: 1, maxCount: 1);

    private bool hasCachedUser = false;
    private GetUserResult? cachedUser = null;

    public async Task<ResolvedUser?> GetUserOrNull(
        ClaimsPrincipal userClaimsPrincipal,
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
                    cachedUser = await InternalGetUserOrNull(
                        userClaimsPrincipal: userClaimsPrincipal,
                        cancellationToken: cancellationToken
                    );
                    hasCachedUser = true;
                }
            }
            finally
            {
                getUserSemaphore.Release();
            }
        }

        if (cachedUser == null)
        {
            return null;
        }

        return new ResolvedUser(
            Id: cachedUser.Id,
            Name: cachedUser.Name,
            EmailAddress: cachedUser.EmailAddress
        );
    }

    private async Task<GetUserResult?> InternalGetUserOrNull(
        ClaimsPrincipal userClaimsPrincipal,
        CancellationToken cancellationToken
    )
    {
        if (!userClaimsPrincipal.TryGetUserInfo(out var userInfo))
        {
            return null;
        }

        var getUserResult = await getUser.Execute(
            externalId: userInfo.ExternalId,
            identityProvider: identityProvider,
            cancellationToken: cancellationToken
        );

        if (getUserResult.TrySuccess(out var getUserSuccess, out var getUserError))
        {
            return getUserSuccess;
        }

        if (getUserError != GetUserError.DoesNotExist)
        {
            logger.LogError(
                "Failed to retrieve user {SubjectClaim} {Provider} due to {Error}",
                userInfo.ExternalId,
                identityProvider,
                getUserError
            );
            return null;
        }

        logger.LogInformation(
            "Registering first time user {SubjectClaim} {Provider}",
            userInfo.ExternalId,
            identityProvider
        );
        var registrationResult = await registerUserUseCase.Execute(
            externalId: userInfo.ExternalId,
            emailAddress: userInfo.EmailAddress,
            name: userInfo.Name,
            provider: identityProvider,
            cancellationToken: cancellationToken
        );

        if (registrationResult != RegisterUserUseCaseResult.UserCreated)
        {
            logger.LogError(
                "Failed to register first time user {SubjectClaim} {Provider} due to {Error}",
                userInfo.ExternalId,
                identityProvider,
                registrationResult
            );
            return null;
        }

        getUserResult = await getUser.Execute(
            externalId: userInfo.ExternalId,
            identityProvider: identityProvider,
            cancellationToken: cancellationToken
        );

        if (
            getUserResult.TrySuccess(
                out var secondGetUserResultSuccess,
                out var secondGetUserResultError
            )
        )
        {
            return secondGetUserResultSuccess;
        }

        logger.LogError(
            "Failed to retrieve first time user {SubjectClaim} {Provider} after registration due to {Error}",
            userInfo.ExternalId,
            identityProvider,
            secondGetUserResultError
        );
        return null;
    }
}
