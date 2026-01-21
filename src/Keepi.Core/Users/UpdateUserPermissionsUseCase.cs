namespace Keepi.Core.Users;

public interface IUpdateUserPermissionsUseCase
{
    Task<IMaybeErrorResult<UpdateUserPermissionsUseCaseError>> Execute(
        int userId,
        UserPermission entriesPermission,
        UserPermission exportsPermission,
        UserPermission projectsPermission,
        UserPermission usersPermission,
        CancellationToken cancellationToken
    );
}

public enum UpdateUserPermissionsUseCaseError
{
    Unknown = 0,
    UnauthenticatedUser,
    UnauthorizedUser,
    IncompatibleUserPermissionsCombination,
    CannotModifyPermissionsOfSelf,
    UnknownUserId,
}

internal sealed class UpdateUserPermissionsUseCase(
    IResolveUser resolveUser,
    IUpdateUserPermissions updateUserPermissions
) : IUpdateUserPermissionsUseCase
{
    public async Task<IMaybeErrorResult<UpdateUserPermissionsUseCaseError>> Execute(
        int userId,
        UserPermission entriesPermission,
        UserPermission exportsPermission,
        UserPermission projectsPermission,
        UserPermission usersPermission,
        CancellationToken cancellationToken
    )
    {
        var userResult = await resolveUser.Execute(cancellationToken: cancellationToken);
        if (!userResult.TrySuccess(out var userSuccessResult, out var userErrorResult))
        {
            return userErrorResult switch
            {
                ResolveUserError.UserNotAuthenticated => Result.Failure(
                    UpdateUserPermissionsUseCaseError.UnauthenticatedUser
                ),
                _ => Result.Failure(UpdateUserPermissionsUseCaseError.Unknown),
            };
        }
        if (!userSuccessResult.UsersPermission.CanModify())
        {
            return Result.Failure(UpdateUserPermissionsUseCaseError.UnauthorizedUser);
        }

        if (userSuccessResult.Id == userId)
        {
            return Result.Failure(UpdateUserPermissionsUseCaseError.CannotModifyPermissionsOfSelf);
        }

        // In order to assign users to a project when modifying it is required
        // that the full list of users can actually be retrieved. Nobody likes
        // guessing user IDs...
        if (projectsPermission.CanModify() && !usersPermission.CanRead())
        {
            return Result.Failure(
                UpdateUserPermissionsUseCaseError.IncompatibleUserPermissionsCombination
            );
        }

        var updateResult = await updateUserPermissions.Execute(
            userId: userId,
            entriesPermission: entriesPermission,
            exportsPermission: exportsPermission,
            projectsPermission: projectsPermission,
            usersPermission: usersPermission,
            cancellationToken: cancellationToken
        );
        if (updateResult.TrySuccess(out var updateErrorResult))
        {
            return Result.Success<UpdateUserPermissionsUseCaseError>();
        }

        return updateErrorResult switch
        {
            UpdateUserPermissionsError.UnknownUserId => Result.Failure(
                UpdateUserPermissionsUseCaseError.UnknownUserId
            ),
            _ => Result.Failure(UpdateUserPermissionsUseCaseError.Unknown),
        };
    }
}
