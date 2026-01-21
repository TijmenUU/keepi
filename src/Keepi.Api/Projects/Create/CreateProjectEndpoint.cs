using System.Diagnostics.CodeAnalysis;
using FastEndpoints;
using Keepi.Core.Projects;

namespace Keepi.Api.Projects.Create;

internal sealed class CreateProjectEndpoint(ICreateProjectUseCase createProjectUseCase)
    : Endpoint<CreateProjectRequest, CreateProjectResponse>
{
    public override void Configure()
    {
        Post("/projects");
    }

    public override async Task HandleAsync(
        CreateProjectRequest request,
        CancellationToken cancellationToken
    )
    {
        if (TryGetValidatedModel(request, out var validatedRequest))
        {
            var result = await createProjectUseCase.Execute(
                name: validatedRequest.Name,
                enabled: validatedRequest.Enabled,
                userIds: validatedRequest.UserIds,
                invoiceItemNames: validatedRequest.InvoiceItemNames,
                cancellationToken: cancellationToken
            );

            if (result.TrySuccess(out var successResult, out var errorResult))
            {
                await Send.OkAsync(
                    new CreateProjectResponse(Id: successResult),
                    cancellation: cancellationToken
                );
                return;
            }

            await (
                errorResult switch
                {
                    CreateProjectUseCaseError.UnauthenticatedUser => Send.UnauthorizedAsync(
                        cancellation: cancellationToken
                    ),
                    CreateProjectUseCaseError.UnauthorizedUser => Send.ForbiddenAsync(
                        cancellation: cancellationToken
                    ),
                    CreateProjectUseCaseError.Unknown => Send.ErrorsAsync(
                        statusCode: 500,
                        cancellation: cancellationToken
                    ),
                    _ => Send.ErrorsAsync(cancellation: cancellationToken),
                }
            );
        }
        else
        {
            await Send.ErrorsAsync(cancellation: cancellationToken);
        }
    }

    private static bool TryGetValidatedModel(
        CreateProjectRequest request,
        [NotNullWhen(returnValue: true)] out ValidatedCreateProjectRequest? validated
    )
    {
        if (string.IsNullOrEmpty(request.Name) || request.Enabled == null)
        {
            validated = null;
            return false;
        }

        if (request.UserIds == null || request.UserIds.Any(i => i == null))
        {
            validated = null;
            return false;
        }

        if (
            request.InvoiceItemNames == null
            || request.InvoiceItemNames.Any(i => i == null || string.IsNullOrEmpty(i))
        )
        {
            validated = null;
            return false;
        }

        validated = new ValidatedCreateProjectRequest(
            Name: request.Name,
            Enabled: request.Enabled.Value,
            UserIds: request.UserIds?.Select(i => i ?? 0).ToArray() ?? [],
            InvoiceItemNames: request.InvoiceItemNames?.Select(i => i ?? string.Empty).ToArray()
                ?? []
        );
        return true;
    }

    record ValidatedCreateProjectRequest(
        string Name,
        bool Enabled,
        int[] UserIds,
        string[] InvoiceItemNames
    );
}
