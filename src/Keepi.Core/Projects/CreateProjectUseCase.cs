using Keepi.Core.InvoiceItems;
using Keepi.Core.Users;
using Microsoft.Extensions.Logging;

namespace Keepi.Core.Projects;

public interface ICreateProjectUseCase
{
    Task<IValueOrErrorResult<int, CreateProjectUseCaseError>> Execute(
        ProjectName name,
        bool enabled,
        UserId[] userIds,
        InvoiceItemName[] invoiceItemNames,
        CancellationToken cancellationToken
    );
}

public enum CreateProjectUseCaseError
{
    Unknown = 0,
    UnauthenticatedUser,
    UnauthorizedUser,
    DuplicateProjectName,
    InvalidActiveDateRange,
    UnknownUserId,
    DuplicateUserIds,
    DuplicateInvoiceItemNames,
}

internal sealed class CreateProjectUseCase(
    IResolveUser resolveUser,
    ISaveNewProject saveNewProject,
    ILogger<CreateProjectUseCase> logger
) : ICreateProjectUseCase
{
    public async Task<IValueOrErrorResult<int, CreateProjectUseCaseError>> Execute(
        ProjectName name,
        bool enabled,
        UserId[] userIds,
        InvoiceItemName[] invoiceItemNames,
        CancellationToken cancellationToken
    )
    {
        var userResult = await resolveUser.Execute(cancellationToken: cancellationToken);
        if (!userResult.TrySuccess(out var userSuccessResult, out var userErrorResult))
        {
            return userErrorResult switch
            {
                ResolveUserError.UserNotAuthenticated => Result.Failure<
                    int,
                    CreateProjectUseCaseError
                >(CreateProjectUseCaseError.UnauthenticatedUser),
                _ => Result.Failure<int, CreateProjectUseCaseError>(
                    CreateProjectUseCaseError.Unknown
                ),
            };
        }
        if (!userSuccessResult.ProjectsPermission.CanModify())
        {
            return Result.Failure<int, CreateProjectUseCaseError>(
                CreateProjectUseCaseError.UnauthorizedUser
            );
        }

        if (userIds.Distinct().Count() != userIds.Length)
        {
            return Result.Failure<int, CreateProjectUseCaseError>(
                CreateProjectUseCaseError.DuplicateUserIds
            );
        }

        if (invoiceItemNames.Distinct().Count() != invoiceItemNames.Length)
        {
            return Result.Failure<int, CreateProjectUseCaseError>(
                CreateProjectUseCaseError.DuplicateInvoiceItemNames
            );
        }

        var result = await saveNewProject.Execute(
            name: name,
            enabled: enabled,
            userIds: userIds,
            invoiceItemNames: invoiceItemNames,
            cancellationToken: cancellationToken
        );

        if (result.TrySuccess(out var successResult, out var errorResult))
        {
            return Result.Success<int, CreateProjectUseCaseError>(successResult);
        }

        switch (errorResult)
        {
            case SaveNewProjectError.DuplicateProjectName:
                return Result.Failure<int, CreateProjectUseCaseError>(
                    CreateProjectUseCaseError.DuplicateProjectName
                );

            case SaveNewProjectError.UnknownUserId:
                return Result.Failure<int, CreateProjectUseCaseError>(
                    CreateProjectUseCaseError.UnknownUserId
                );

            default:
                logger.LogError("Unexpected error {Error} in create project use case", errorResult);
                return Result.Failure<int, CreateProjectUseCaseError>(
                    CreateProjectUseCaseError.Unknown
                );
        }
    }
}
