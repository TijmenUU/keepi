using System.Security.Claims;
using Keepi.Core;
using Keepi.Core.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Keepi.Api.Authorization;

internal sealed class ResolveUser(
    IHttpContextAccessor httpContextAccessor,
    IGetOrRegisterNewUserUseCase getOrRegisterNewUserUseCase,
    ILogger<ResolveUser> logger
) : IResolveUser
{
    private const UserIdentityProvider identityProvider = UserIdentityProvider.GitHub;

    private readonly SemaphoreSlim getUserSemaphore = new(initialCount: 1, maxCount: 1);

    private bool hasCachedUser = false;
    private GetUserResult? cachedUser = null;

    public async Task<IValueOrErrorResult<ResolvedUser, ResolveUserError>> Execute(
        CancellationToken cancellationToken
    )
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            logger.LogError("HTTP context is null");
            return Result.Failure<ResolvedUser, ResolveUserError>(ResolveUserError.Unknown);
        }

        if (!hasCachedUser)
        {
            await getUserSemaphore.WaitAsync(cancellationToken: cancellationToken);
            try
            {
                if (!hasCachedUser)
                {
                    var result = await InternalGetUserOrNull(
                        claimsPrincipal: httpContext.User,
                        cancellationToken: cancellationToken
                    );

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
    > InternalGetUserOrNull(ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken)
    {
        var getUserInfoResult = GetUserInfo(claimsPrincipal: claimsPrincipal);
        if (!getUserInfoResult.TrySuccess(out var userInfo, out var getUserInfoError))
        {
            return Result.Failure<GetOrRegisterNewUserUseCaseOutput, ResolveUserError>(
                getUserInfoError
            );
        }

        var result = await getOrRegisterNewUserUseCase.Execute(
            externalId: userInfo.ExternalId,
            emailAddress: userInfo.EmailAddress,
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

    private static IValueOrErrorResult<UserInfo, ResolveUserError> GetUserInfo(
        ClaimsPrincipal claimsPrincipal
    )
    {
        if (claimsPrincipal.Identity == null || !claimsPrincipal.Claims.Any())
        {
            return Result.Failure<UserInfo, ResolveUserError>(
                ResolveUserError.UserNotAuthenticated
            );
        }

        if (claimsPrincipal.Identity.AuthenticationType != "GitHub")
        {
            return Result.Failure<UserInfo, ResolveUserError>(
                ResolveUserError.UnsupportedIdentityProvider
            );
        }

        var externalIdClaim = claimsPrincipal.Claims.FirstOrDefault(c =>
            c.Type == ClaimTypes.NameIdentifier
        );
        string? userName = claimsPrincipal.Identity.Name;
        string? emailAddress = claimsPrincipal
            .Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)
            ?.Value;

        if (
            string.IsNullOrWhiteSpace(externalIdClaim?.Value)
            || string.IsNullOrWhiteSpace(userName)
            || string.IsNullOrWhiteSpace(emailAddress)
        )
        {
            return Result.Failure<UserInfo, ResolveUserError>(ResolveUserError.MalformedUserClaims);
        }

        return Result.Success<UserInfo, ResolveUserError>(
            new(
                ExternalId: externalIdClaim.Value,
                Name: userName,
                EmailAddress: emailAddress,
                Origin: UserIdentityProvider.GitHub
            )
        );
    }

    private sealed record UserInfo(
        string ExternalId,
        string Name,
        string EmailAddress,
        UserIdentityProvider Origin
    );
}
