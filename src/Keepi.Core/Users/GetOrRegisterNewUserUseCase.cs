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
    IUpdateUserIdentity updateUserIdentity,
    IUserWithPermissionsExists userWithPermissionsExists,
    IGetFirstAdminUserEmailAddress getFirstAdminUserEmailAddress,
    ISaveNewUser saveNewUser,
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
            if (getUserSuccess.EmailAddress != emailAddress || getUserSuccess.Name != name)
            {
                logger.LogDebug(
                    "The {Provider} user {SubjectClaim} provided a different name and/or email address known to Keepi",
                    identityProvider,
                    externalId
                );

                var result = await updateUserIdentity.Execute(
                    userId: getUserSuccess.Id,
                    emailAddress: emailAddress,
                    name: name,
                    cancellationToken: cancellationToken
                );
                if (!result.TrySuccess(out var errorResult))
                {
                    logger.LogWarning(
                        "Failed to update {Provider} user {SubjectClaim} due to {Error} error",
                        identityProvider,
                        externalId,
                        errorResult
                    );
                }
                else
                {
                    return Result.Success<
                        GetOrRegisterNewUserUseCaseOutput,
                        GetOrRegisterNewUserUseCaseError
                    >(
                        new GetOrRegisterNewUserUseCaseOutput(
                            User: new GetUserResult(
                                Id: getUserSuccess.Id,
                                Name: name,
                                EmailAddress: emailAddress,
                                IdentityOrigin: identityProvider,
                                EntriesPermission: getUserSuccess.EntriesPermission,
                                ExportsPermission: getUserSuccess.ExportsPermission,
                                ProjectsPermission: getUserSuccess.ProjectsPermission,
                                UsersPermission: getUserSuccess.UsersPermission
                            ),
                            NewlyRegistered: false
                        )
                    );
                }
            }

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
        var registrationResult = await TryRegisterNewUser(
            externalId: externalId,
            emailAddress: emailAddress,
            name: name,
            provider: identityProvider,
            cancellationToken: cancellationToken
        );

        if (!registrationResult.TrySuccess(out var _))
        {
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
                    NewlyRegistered: true
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

    private async Task<IMaybeErrorResult<RegisterUserResult>> TryRegisterNewUser(
        string externalId,
        string emailAddress,
        string name,
        UserIdentityProvider provider,
        CancellationToken cancellationToken
    )
    {
        var adminUserExistsResult = await userWithPermissionsExists.Execute(
            entriesPermission: UserPermission.ReadAndModify,
            exportsPermission: UserPermission.ReadAndModify,
            projectsPermission: UserPermission.ReadAndModify,
            usersPermission: UserPermission.ReadAndModify,
            cancellationToken: cancellationToken
        );
        if (
            !adminUserExistsResult.TrySuccess(out var adminUserExists, out var adminUserExistsError)
        )
        {
            logger.LogError(
                "Failed with error {Error} to check if admin user already exists",
                adminUserExistsError
            );
            return Result.Failure(RegisterUserResult.AdminUserCheckFailed);
        }

        IMaybeErrorResult<SaveNewUserError> saveResult;
        if (
            !adminUserExists
            && getFirstAdminUserEmailAddress
                .Execute()
                .TrySuccess(out var firstAdminUserEmailAddress, out _)
            && firstAdminUserEmailAddress == emailAddress
        )
        {
            logger.LogInformation(
                "Registering {Provider} user {ExternalId} as the first admin user",
                provider,
                externalId
            );
            saveResult = await saveNewUser.Execute(
                externalId: externalId,
                emailAddress: emailAddress,
                name: name,
                userIdentityProvider: provider,
                entriesPermission: UserPermission.ReadAndModify,
                exportsPermission: UserPermission.ReadAndModify,
                projectsPermission: UserPermission.ReadAndModify,
                usersPermission: UserPermission.ReadAndModify,
                cancellationToken: cancellationToken
            );
        }
        else
        {
            saveResult = await saveNewUser.Execute(
                externalId: externalId,
                emailAddress: emailAddress,
                name: name,
                userIdentityProvider: provider,
                entriesPermission: UserPermission.ReadAndModify,
                exportsPermission: UserPermission.None,
                projectsPermission: UserPermission.None,
                usersPermission: UserPermission.None,
                cancellationToken: cancellationToken
            );
        }

        if (saveResult.TrySuccess(out var saveErrorResult))
        {
            return Result.Success<RegisterUserResult>();
        }

        if (saveErrorResult == SaveNewUserError.DuplicateUser)
        {
            return Result.Failure(RegisterUserResult.UserAlreadyExists);
        }

        logger.LogError(
            "Unexpected error {Error} whilst saving new {Provider} user {ExternalId}",
            saveErrorResult,
            provider,
            externalId
        );
        return Result.Failure(RegisterUserResult.Unknown);
    }

    private enum RegisterUserResult
    {
        Unknown,
        UserAlreadyExists,
        AdminUserCheckFailed,
    };
}
