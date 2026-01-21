using Keepi.Core.InvoiceItems;
using Keepi.Core.Users;
using Microsoft.Extensions.Logging;

namespace Keepi.Core.Projects;

public interface IUpdateProjectUseCase
{
    Task<IMaybeErrorResult<UpdateProjectUseCaseError>> Execute(
        int id,
        string name,
        bool enabled,
        int[] userIds,
        (int? Id, string Name)[] invoiceItems,
        CancellationToken cancellationToken
    );
}

public enum UpdateProjectUseCaseError
{
    Unknown = 0,
    UnauthenticatedUser,
    UnauthorizedUser,
    UnknownProjectId,
    InvalidProjectName,
    DuplicateProjectName,
    InvalidActiveDateRange,
    UnknownUserId,
    DuplicateUserIds,
    InvalidInvoiceItemName,
    DuplicateInvoiceItemIds,
    DuplicateInvoiceItemNames,
}

internal sealed class UpdateProjectUseCase(
    IResolveUser resolveUser,
    IUpdateProject updateProject,
    ILogger<UpdateProjectUseCase> logger
) : IUpdateProjectUseCase
{
    public async Task<IMaybeErrorResult<UpdateProjectUseCaseError>> Execute(
        int id,
        string name,
        bool enabled,
        int[] userIds,
        (int? Id, string Name)[] invoiceItems,
        CancellationToken cancellationToken
    )
    {
        var userResult = await resolveUser.Execute(cancellationToken: cancellationToken);
        if (!userResult.TrySuccess(out var userSuccessResult, out var userErrorResult))
        {
            return userErrorResult switch
            {
                ResolveUserError.UserNotAuthenticated => Result.Failure(
                    UpdateProjectUseCaseError.UnauthenticatedUser
                ),
                _ => Result.Failure(UpdateProjectUseCaseError.Unknown),
            };
        }
        if (!userSuccessResult.ProjectsPermission.CanModify())
        {
            return Result.Failure(UpdateProjectUseCaseError.UnauthorizedUser);
        }

        if (!ProjectEntity.IsValidName(name))
        {
            return Result.Failure(UpdateProjectUseCaseError.InvalidProjectName);
        }

        if (!ProjectEntity.HasUniqueUserIds(userIds))
        {
            return Result.Failure(UpdateProjectUseCaseError.DuplicateUserIds);
        }

        if (invoiceItems.Any(i => !InvoiceItemEntity.IsValidName(i.Name)))
        {
            return Result.Failure(UpdateProjectUseCaseError.InvalidInvoiceItemName);
        }

        var nonNullInvoiceItemIds = invoiceItems
            .Where(i => i.Id.HasValue)
            .Select(i => i.Id ?? 0)
            .ToArray();
        if (nonNullInvoiceItemIds.Distinct().Count() != nonNullInvoiceItemIds.Length)
        {
            return Result.Failure(UpdateProjectUseCaseError.DuplicateInvoiceItemIds);
        }

        if (invoiceItems.DistinctBy(i => i.Name).Count() != invoiceItems.Length)
        {
            return Result.Failure(UpdateProjectUseCaseError.DuplicateInvoiceItemNames);
        }

        var result = await updateProject.Execute(
            id: id,
            name: name,
            enabled: enabled,
            userIds: userIds,
            invoiceItems: invoiceItems,
            cancellationToken: cancellationToken
        );

        if (result.TrySuccess(out var errorResult))
        {
            return Result.Success<UpdateProjectUseCaseError>();
        }

        switch (errorResult)
        {
            case UpdateProjectError.UnknownProjectId:
                return Result.Failure(UpdateProjectUseCaseError.UnknownProjectId);

            case UpdateProjectError.DuplicateProjectName:
                return Result.Failure(UpdateProjectUseCaseError.DuplicateProjectName);

            case UpdateProjectError.UnknownUserId:
                return Result.Failure(UpdateProjectUseCaseError.UnknownUserId);

            default:
                logger.LogError("Unexpected error {Error} in update project use case", errorResult);
                return Result.Failure(UpdateProjectUseCaseError.Unknown);
        }
    }
}
