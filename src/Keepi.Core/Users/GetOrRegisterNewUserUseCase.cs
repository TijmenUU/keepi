using Microsoft.Extensions.Logging;

namespace Keepi.Core.Users;

public interface IGetOrRegisterNewUserUseCase
{
    Task<
        IValueOrErrorResult<GetOrRegisterNewUserUseCaseOutput, GetOrRegisterNewUserUseCaseError>
    > Execute(
        string externalId,
        string emailAddress,
        string name,
        UserIdentityProvider identityProvider,
        CancellationToken cancellationToken
    );
}

public sealed record GetOrRegisterNewUserUseCaseOutput(GetUserResult User, bool NewlyRegistered);

public enum GetOrRegisterNewUserUseCaseError
{
    Unknown = 0,
    RegistrationFailed,
}

internal sealed class GetOrRegisterNewUserUseCase(
    IGetUser getUser,
    IRegisterUserUseCase registerUserUseCase,
    ILogger<GetOrRegisterNewUserUseCase> logger
) : IGetOrRegisterNewUserUseCase
{
    public async Task<
        IValueOrErrorResult<GetOrRegisterNewUserUseCaseOutput, GetOrRegisterNewUserUseCaseError>
    > Execute(
        string externalId,
        string emailAddress,
        string name,
        UserIdentityProvider identityProvider,
        CancellationToken cancellationToken
    )
    {
        var getUserResult = await getUser.Execute(
            externalId: externalId,
            identityProvider: identityProvider,
            cancellationToken: cancellationToken
        );

        if (getUserResult.TrySuccess(out var getUserSuccess, out var getUserError))
        {
            return Result.Success<
                GetOrRegisterNewUserUseCaseOutput,
                GetOrRegisterNewUserUseCaseError
            >(new GetOrRegisterNewUserUseCaseOutput(User: getUserSuccess, NewlyRegistered: false));
        }

        if (getUserError != GetUserError.DoesNotExist)
        {
            logger.LogError(
                "Failed to retrieve {Provider} user {SubjectClaim} due to {Error}",
                identityProvider,
                externalId,
                getUserError
            );
            return Result.Failure<
                GetOrRegisterNewUserUseCaseOutput,
                GetOrRegisterNewUserUseCaseError
            >(GetOrRegisterNewUserUseCaseError.Unknown);
        }

        logger.LogInformation(
            "Attempting registration of first time {Provider} user {SubjectClaim}",
            identityProvider,
            externalId
        );
        var registrationResult = await registerUserUseCase.Execute(
            externalId: externalId,
            emailAddress: emailAddress,
            name: name,
            provider: identityProvider,
            cancellationToken: cancellationToken
        );

        if (registrationResult != RegisterUserUseCaseResult.UserCreated)
        {
            logger.LogError(
                "Failed to register first time {Provider} user {SubjectClaim} due to {Error}",
                identityProvider,
                externalId,
                registrationResult
            );

            return Result.Failure<
                GetOrRegisterNewUserUseCaseOutput,
                GetOrRegisterNewUserUseCaseError
            >(GetOrRegisterNewUserUseCaseError.RegistrationFailed);
        }

        getUserResult = await getUser.Execute(
            externalId: externalId,
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
            return Result.Success<
                GetOrRegisterNewUserUseCaseOutput,
                GetOrRegisterNewUserUseCaseError
            >(
                new GetOrRegisterNewUserUseCaseOutput(
                    User: secondGetUserResultSuccess,
                    NewlyRegistered: false
                )
            );
        }

        logger.LogError(
            "Failed to retrieve first time {Provider} user {SubjectClaim} after registration due to {Error}",
            identityProvider,
            externalId,
            secondGetUserResultError
        );
        return Result.Failure<GetOrRegisterNewUserUseCaseOutput, GetOrRegisterNewUserUseCaseError>(
            GetOrRegisterNewUserUseCaseError.Unknown
        );
    }
}
