using EntityFramework.Exceptions.Common;
using Keepi.Core;
using Keepi.Core.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Keepi.Infrastructure.Data.Users;

internal sealed class UserRepository(
    DatabaseContext databaseContext,
    ILogger<UserRepository> logger
) : IGetUserExists, IGetUser, IStoreNewUser
{
    async Task<bool> IGetUserExists.Execute(
        string externalId,
        string emailAddress,
        CancellationToken cancellationToken
    )
    {
        return await databaseContext.Users.AnyAsync(
            u => u.ExternalId == externalId || u.EmailAddress == emailAddress,
            cancellationToken
        );
    }

    async Task<IMaybeErrorResult<StoreNewUserError>> IStoreNewUser.Execute(
        string externalId,
        string emailAddress,
        string name,
        UserIdentityProvider userIdentityProvider,
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
                }
            );
            await databaseContext.SaveChangesAsync(cancellationToken);

            return Result.Success<StoreNewUserError>();
        }
        // This is a bit of a rough catch as it is not known what caused the
        // exception. Sqlite does not provide the exact constraint nor column name
        // so for now this seems all that can be done.
        catch (UniqueConstraintException)
        {
            return Result.Failure(StoreNewUserError.DuplicateUser);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error whilst storing new user");
            return Result.Failure(StoreNewUserError.Unknown);
        }
    }

    async Task<IValueOrErrorResult<Core.Users.UserEntity, GetUserError>> IGetUser.Execute(
        string externalId,
        UserIdentityProvider identityProvider,
        CancellationToken cancellationToken
    )
    {
        var identityOrigin = ToDatabaseEnum(identityProvider);
        try
        {
            var user = await databaseContext.Users.FirstOrDefaultAsync(
                predicate: u => u.ExternalId == externalId && u.IdentityOrigin == identityOrigin,
                cancellationToken: cancellationToken
            );

            if (user == null)
            {
                return Result.Failure<Core.Users.UserEntity, GetUserError>(
                    GetUserError.DoesNotExist
                );
            }

            return Result.Success<Core.Users.UserEntity, GetUserError>(
                new Core.Users.UserEntity(
                    id: user.Id,
                    emailAddress: user.EmailAddress,
                    name: user.Name,
                    identityOrigin: user.IdentityOrigin.MapToDomainModel()
                )
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error whilst trying to get user");
            return Result.Failure<Core.Users.UserEntity, GetUserError>(GetUserError.Unknown);
        }
    }

    private static UserIdentityOrigin ToDatabaseEnum(UserIdentityProvider userIdentityProvider)
    {
        return userIdentityProvider switch
        {
            UserIdentityProvider.GitHub => UserIdentityOrigin.GitHub,
            _ => throw new ArgumentOutOfRangeException(paramName: nameof(userIdentityProvider)),
        };
    }
}
