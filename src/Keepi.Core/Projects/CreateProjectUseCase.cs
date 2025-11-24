using Keepi.Core.InvoiceItems;
using Microsoft.Extensions.Logging;

namespace Keepi.Core.Projects;

public interface ICreateProjectUseCase
{
    Task<IValueOrErrorResult<int, CreateProjectUseCaseError>> Execute(
        string name,
        bool enabled,
        int[] userIds,
        string[] invoiceItemNames,
        CancellationToken cancellationToken
    );
}

public enum CreateProjectUseCaseError
{
    Unknown = 0,
    InvalidProjectName,
    DuplicateProjectName,
    InvalidActiveDateRange,
    UnknownUserId,
    DuplicateUserIds,
    InvalidInvoiceItemName,
    DuplicateInvoiceItemNames,
}

internal sealed class CreateProjectUseCase(
    ISaveNewProject saveNewProject,
    ILogger<CreateProjectUseCase> logger
) : ICreateProjectUseCase
{
    public async Task<IValueOrErrorResult<int, CreateProjectUseCaseError>> Execute(
        string name,
        bool enabled,
        int[] userIds,
        string[] invoiceItemNames,
        CancellationToken cancellationToken
    )
    {
        {
            if (!ProjectEntity.IsValidName(name))
            {
                return Result.Failure<int, CreateProjectUseCaseError>(
                    CreateProjectUseCaseError.InvalidProjectName
                );
            }

            if (!ProjectEntity.HasUniqueUserIds(userIds))
            {
                return Result.Failure<int, CreateProjectUseCaseError>(
                    CreateProjectUseCaseError.DuplicateUserIds
                );
            }

            if (invoiceItemNames.Any(i => !InvoiceItemEntity.IsValidName(i)))
            {
                return Result.Failure<int, CreateProjectUseCaseError>(
                    CreateProjectUseCaseError.InvalidInvoiceItemName
                );
            }

            if (!ProjectEntity.HasUniqueInvoiceItemNames(invoiceItemNames))
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
                    logger.LogError(
                        "Unexpected error {Error} in create project use case",
                        errorResult
                    );
                    return Result.Failure<int, CreateProjectUseCaseError>(
                        CreateProjectUseCaseError.Unknown
                    );
            }
        }
    }
}
