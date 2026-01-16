using System.Security.Claims;
using Keepi.Core.Users;

namespace Keepi.Api.Authorization;

public interface IResolveUserHelper
{
    Task<ResolvedUser?> GetUserOrNull(
        ClaimsPrincipal userClaimsPrincipal,
        CancellationToken cancellationToken
    );
}

public sealed record ResolvedUser(int Id, string Name, string EmailAddress);

internal sealed class ResolveUserHelper(IGetOrRegisterNewUserUseCase getOrRegisterNewUserUseCase)
    : IResolveUserHelper
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

        var result = await getOrRegisterNewUserUseCase.Execute(
            externalId: userInfo.ExternalId,
            emailAddress: userInfo.EmailAddress,
            name: userInfo.Name,
            identityProvider: identityProvider,
            cancellationToken: cancellationToken
        );

        if (!result.TrySuccess(out var successResult, out var _))
        {
            return null;
        }

        return successResult.User;
    }
}
