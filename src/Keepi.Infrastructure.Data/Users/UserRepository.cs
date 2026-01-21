using EntityFramework.Exceptions.Common;
using Keepi.Core;
using Keepi.Core.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Keepi.Infrastructure.Data.Users;

internal sealed class UserRepository(
    DatabaseContext databaseContext,
    ILogger<UserRepository> logger
) : IGetUserExists, IGetUser, ISaveNewUser, IUpdateUserInfo, IGetUsers, IUpdateUserPermissions
{
    async Task<IValueOrErrorResult<bool, GetUserExistsError>> IGetUserExists.Execute(
        string externalId,
        Core.Users.UserIdentityProvider userIdentityProvider,
        string emailAddress,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var databaseIdentityProvider = ToDatabaseEnum(userIdentityProvider);
            var result = await databaseContext
                .Users.Where(u =>
                    u.EmailAddress == emailAddress
                    || (u.IdentityOrigin == databaseIdentityProvider && u.ExternalId == externalId)
                )
                .AnyAsync(cancellationToken: cancellationToken);

            return Result.Success<bool, GetUserExistsError>(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error whilst checking for existing user");
            return Result.Failure<bool, GetUserExistsError>(GetUserExistsError.Unknown);
        }
    }

    async Task<IMaybeErrorResult<SaveNewUserError>> ISaveNewUser.Execute(
        string externalId,
        string emailAddress,
        string name,
        Core.Users.UserIdentityProvider userIdentityProvider,
        Core.Users.UserPermission entriesPermission,
        Core.Users.UserPermission exportsPermission,
        Core.Users.UserPermission projectsPermission,
        Core.Users.UserPermission usersPermission,
        CancellationToken cancellationToken
    )
    {
        try
        {
            databaseContext.Add(
                new UserEntity
                {
                    ExternalId = externalId,
                    EmailAddress = emailAddress,
                    Name = name,
                    IdentityOrigin = ToDatabaseEnum(userIdentityProvider),
                    EntriesPermission = ToDatabaseEnum(entriesPermission),
                    ExportsPermission = ToDatabaseEnum(exportsPermission),
                    ProjectsPermission = ToDatabaseEnum(projectsPermission),
                    UsersPermission = ToDatabaseEnum(usersPermission),
                }
            );
            await databaseContext.SaveChangesAsync(cancellationToken);

            return Result.Success<SaveNewUserError>();
        }
        // This is a bit of a rough catch as it is not known what caused the
        // exception. Sqlite does not provide the exact constraint nor column name
        // so for now this seems all that can be done.
        catch (UniqueConstraintException)
        {
            return Result.Failure(SaveNewUserError.DuplicateUser);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error whilst storing new user");
            return Result.Failure(SaveNewUserError.Unknown);
        }
    }

    private static UserPermission ToDatabaseEnum(Core.Users.UserPermission permission)
    {
        return permission switch
        {
            Core.Users.UserPermission.None => UserPermission.None,
            Core.Users.UserPermission.Read => UserPermission.Read,
            Core.Users.UserPermission.ReadAndModify => UserPermission.ReadAndModify,
            _ => throw new ArgumentOutOfRangeException(
                paramName: nameof(permission),
                message: $"Value {permission} does not exist in the domain"
            ),
        };
    }

    async Task<IMaybeErrorResult<UpdateUserInfoError>> IUpdateUserInfo.Execute(
        int userId,
        string emailAddress,
        string name,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var entity = await databaseContext.Users.SingleOrDefaultAsync(
                u => u.Id == userId,
                cancellationToken: cancellationToken
            );
            if (entity == null)
            {
                return Result.Failure(UpdateUserInfoError.UnknownUserId);
            }

            entity.EmailAddress = emailAddress;
            entity.Name = name;
            await databaseContext.SaveChangesAsync(cancellationToken: cancellationToken);

            return Result.Success<UpdateUserInfoError>();
        }
        // This is a bit of a rough catch as it is not known what caused the
        // exception. Sqlite does not provide the exact constraint nor column name
        // so for now this seems all that can be done.
        catch (UniqueConstraintException)
        {
            return Result.Failure(UpdateUserInfoError.DuplicateUser);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error whilst updating user");
            return Result.Failure(UpdateUserInfoError.Unknown);
        }
    }

    async Task<IValueOrErrorResult<GetUserResult, GetUserError>> IGetUser.Execute(
        string externalId,
        Core.Users.UserIdentityProvider identityProvider,
        CancellationToken cancellationToken
    )
    {
        var identityOrigin = ToDatabaseEnum(identityProvider);
        try
        {
            var user = await databaseContext
                .Users.AsNoTracking()
                .FirstOrDefaultAsync(
                    predicate: u =>
                        u.ExternalId == externalId && u.IdentityOrigin == identityOrigin,
                    cancellationToken: cancellationToken
                );

            if (user == null)
            {
                return Result.Failure<GetUserResult, GetUserError>(GetUserError.DoesNotExist);
            }

            return Result.Success<GetUserResult, GetUserError>(
                new GetUserResult(
                    Id: user.Id,
                    EmailAddress: user.EmailAddress,
                    Name: user.Name,
                    IdentityOrigin: ToResultEnum(user.IdentityOrigin),
                    EntriesPermission: ToResultEnum(user.EntriesPermission),
                    ExportsPermission: ToResultEnum(user.ExportsPermission),
                    ProjectsPermission: ToResultEnum(user.ProjectsPermission),
                    UsersPermission: ToResultEnum(user.UsersPermission)
                )
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error whilst trying to get user");
            return Result.Failure<GetUserResult, GetUserError>(GetUserError.Unknown);
        }
    }

    async Task<IMaybeErrorResult<UpdateUserPermissionsError>> IUpdateUserPermissions.Execute(
        int userId,
        Core.Users.UserPermission entriesPermission,
        Core.Users.UserPermission exportsPermission,
        Core.Users.UserPermission projectsPermission,
        Core.Users.UserPermission usersPermission,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var entity = await databaseContext.Users.SingleOrDefaultAsync(
                u => u.Id == userId,
                cancellationToken: cancellationToken
            );
            if (entity == null)
            {
                return Result.Failure(UpdateUserPermissionsError.UnknownUserId);
            }

            entity.EntriesPermission = ToDatabaseEnum(entriesPermission);
            entity.ExportsPermission = ToDatabaseEnum(exportsPermission);
            entity.ProjectsPermission = ToDatabaseEnum(projectsPermission);
            entity.UsersPermission = ToDatabaseEnum(usersPermission);
            await databaseContext.SaveChangesAsync(cancellationToken: cancellationToken);

            return Result.Success<UpdateUserPermissionsError>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error whilst updating user");
            return Result.Failure(UpdateUserPermissionsError.Unknown);
        }
    }

    private static Core.Users.UserPermission ToResultEnum(UserPermission value)
    {
        return value switch
        {
            UserPermission.None => Core.Users.UserPermission.None,
            UserPermission.Read => Core.Users.UserPermission.Read,
            UserPermission.ReadAndModify => Core.Users.UserPermission.ReadAndModify,
            _ => throw new ArgumentOutOfRangeException(
                paramName: nameof(value),
                message: $"Value {value} does not exist in the domain"
            ),
        };
    }

    private static UserIdentityProvider ToDatabaseEnum(
        Core.Users.UserIdentityProvider userIdentityProvider
    )
    {
        return userIdentityProvider switch
        {
            Core.Users.UserIdentityProvider.GitHub => UserIdentityProvider.GitHub,
            _ => throw new ArgumentOutOfRangeException(
                paramName: nameof(userIdentityProvider),
                message: $"Value {userIdentityProvider} does not exist in the domain"
            ),
        };
    }

    async Task<IValueOrErrorResult<GetUsersResult, GetUsersError>> IGetUsers.Execute(
        CancellationToken cancellationToken
    )
    {
        try
        {
            return Result.Success<GetUsersResult, GetUsersError>(
                new(
                    Users: (
                        await databaseContext.Users.ToArrayAsync(
                            cancellationToken: cancellationToken
                        )
                    )
                        .Select(u => new GetUsersResultUser(
                            Id: u.Id,
                            Name: u.Name,
                            EmailAddress: u.EmailAddress,
                            IdentityOrigin: ToResultEnum(u.IdentityOrigin),
                            EntriesPermission: ToResultEnum(u.EntriesPermission),
                            ExportsPermission: ToResultEnum(u.ExportsPermission),
                            ProjectsPermission: ToResultEnum(u.ProjectsPermission),
                            UsersPermission: ToResultEnum(u.UsersPermission)
                        ))
                        .ToArray()
                )
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error whilst trying to get all users");
            return Result.Failure<GetUsersResult, GetUsersError>(GetUsersError.Unknown);
        }
    }

    private static Core.Users.UserIdentityProvider ToResultEnum(UserIdentityProvider value)
    {
        return value switch
        {
            UserIdentityProvider.GitHub => Core.Users.UserIdentityProvider.GitHub,
            _ => throw new ArgumentOutOfRangeException(
                paramName: nameof(value),
                message: $"Value {value} does not exist in the domain"
            ),
        };
    }
}
