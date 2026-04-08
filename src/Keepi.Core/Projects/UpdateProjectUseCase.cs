using Keepi.Core.InvoiceItems;
using Keepi.Core.Users;
using Microsoft.Extensions.Logging;

namespace Keepi.Core.Projects;

public interface IUpdateProjectUseCase
{
    Task<IMaybeErrorResult<UpdateProjectUseCaseError>> Execute(
        ProjectId id,
        ProjectName name,
        bool enabled,
        UserId[] userIds,
        (InvoiceItemId? Id, InvoiceItemName Name)[] invoiceItems,
        CancellationToken cancellationToken
    );
}

public enum UpdateProjectUseCaseError
{
    Unknown = 0,
    UnauthenticatedUser,
    UnauthorizedUser,
    UnknownProjectId,
    DuplicateProjectName,
    UnknownUserId,
    DuplicateUserIds,
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
        ProjectId id,
        ProjectName name,
        bool enabled,
        UserId[] userIds,
        (InvoiceItemId? Id, InvoiceItemName Name)[] invoiceItems,
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

        if (userIds.Distinct().Count() != userIds.Length)
        {
            return Result.Failure(UpdateProjectUseCaseError.DuplicateUserIds);
        }

        var nonNullInvoiceItemIds = invoiceItems
            .Where(i => i.Id.HasValue)
            .Select(i => i.Id?.Value ?? 0)
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
