using Microsoft.Extensions.Logging;

namespace Keepi.Core.Users;

public interface IRegisterUserUseCase
{
    Task<RegisterUserUseCaseResult> Execute(
        string externalId,
        string emailAddress,
        string name,
        UserIdentityProvider provider,
        CancellationToken cancellationToken
    );
}

public enum RegisterUserUseCaseResult
{
    Unknown,
    UserAlreadyExists,
    UserCreated,
};

internal sealed class RegisterUserUseCase(
    IGetUserExists getUserExists,
    ISaveNewUser saveNewUser,
    ILogger<RegisterUserUseCase> logger
) : IRegisterUserUseCase
{
    public async Task<RegisterUserUseCaseResult> Execute(
        string externalId,
        string emailAddress,
        string name,
        UserIdentityProvider provider,
        CancellationToken cancellationToken
    )
    {
        var getUserExistsResult = await getUserExists.Execute(
            externalId: externalId,
            userIdentityProvider: provider,
            emailAddress: emailAddress,
            cancellationToken: cancellationToken
        );
        if (!getUserExistsResult.TrySuccess(out var successResult, out var errorResult))
        {
            logger.LogError(
                "Unexpected error {Error} whilst checking if user {ExternalId} {IdentityProvider} already exists",
                errorResult,
                externalId,
                provider
            );
            return RegisterUserUseCaseResult.Unknown;
        }

        if (successResult)
        {
            return RegisterUserUseCaseResult.UserAlreadyExists;
        }

        var saveResult = await saveNewUser.Execute(
            externalId: externalId,
            emailAddress: emailAddress,
            name: name,
            userIdentityProvider: provider,
            cancellationToken: cancellationToken
        );

        if (saveResult.TrySuccess(out var saveErrorResult))
        {
            return RegisterUserUseCaseResult.UserCreated;
        }

        if (saveErrorResult == SaveNewUserError.DuplicateUser)
        {
            return RegisterUserUseCaseResult.UserAlreadyExists;
        }

        logger.LogError(
            "Unexpected error {Error} whilst saving new user {ExternalId} {IdentityProvider}",
            saveErrorResult,
            externalId,
            provider
        );
        return RegisterUserUseCaseResult.Unknown;
    }
}
