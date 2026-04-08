using System.Diagnostics.CodeAnalysis;
using FastEndpoints;
using Keepi.Core.InvoiceItems;
using Keepi.Core.Projects;
using Keepi.Core.Users;

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
        if (
            string.IsNullOrEmpty(request.Name)
            || !ProjectName.TryFrom(value: request.Name, out var projectName)
        )
        {
            validated = null;
            return false;
        }

        if (request.Enabled == null)
        {
            validated = null;
            return false;
        }

        if (request.UserIds == null)
        {
            validated = null;
            return false;
        }
        var userIds = new List<UserId>();
        foreach (var id in request.UserIds)
        {
            if (id == null || !UserId.TryFrom(value: id.Value, out var userId))
            {
                validated = null;
                return false;
            }

            userIds.Add(userId);
        }

        if (request.InvoiceItemNames == null)
        {
            validated = null;
            return false;
        }

        var invoiceItemNames = new List<InvoiceItemName>();
        foreach (var name in request.InvoiceItemNames)
        {
            if (name == null || !InvoiceItemName.TryFrom(value: name, out var invoiceItemName))
            {
                validated = null;
                return false;
            }

            invoiceItemNames.Add(invoiceItemName);
        }

        validated = new ValidatedCreateProjectRequest(
            Name: projectName,
            Enabled: request.Enabled.Value,
            UserIds: [.. userIds],
            InvoiceItemNames: [.. invoiceItemNames]
        );
        return true;
    }

    record ValidatedCreateProjectRequest(
        ProjectName Name,
        bool Enabled,
        UserId[] UserIds,
        InvoiceItemName[] InvoiceItemNames
    );
}
