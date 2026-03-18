using System.Diagnostics.CodeAnalysis;
using FastEndpoints;
using Keepi.Core.Entries;
using Keepi.Core.InvoiceItems;
using Keepi.Core.Projects;
using Keepi.Core.Users;

namespace Keepi.Api.Projects.Update;

internal sealed class UpdateProjectEndpoint(IUpdateProjectUseCase updateProjectUseCase)
    : Endpoint<UpdateProjectRequest>
{
    public override void Configure()
    {
        Put("/projects/{ProjectId}");
    }

    public override async Task HandleAsync(
        UpdateProjectRequest request,
        CancellationToken cancellationToken
    )
    {
        var routeProjectId = Route<int>(paramName: "ProjectId");
        if (!ProjectId.TryFrom(value: routeProjectId, out var projectId))
        {
            await Send.ErrorsAsync(cancellation: cancellationToken);
            return;
        }

        if (TryGetValidatedModel(request, out var validatedRequest))
        {
            var result = await updateProjectUseCase.Execute(
                id: projectId,
                name: validatedRequest.Name,
                enabled: validatedRequest.Enabled,
                userIds: validatedRequest.UserIds,
                invoiceItems: validatedRequest.InvoiceItems,
                cancellationToken: cancellationToken
            );

            if (result.TrySuccess(out var errorResult))
            {
                await Send.NoContentAsync(cancellation: cancellationToken);
                return;
            }

            await (
                errorResult switch
                {
                    UpdateProjectUseCaseError.UnauthenticatedUser => Send.UnauthorizedAsync(
                        cancellation: cancellationToken
                    ),
                    UpdateProjectUseCaseError.UnauthorizedUser => Send.ForbiddenAsync(
                        cancellation: cancellationToken
                    ),
                    UpdateProjectUseCaseError.Unknown => Send.ErrorsAsync(
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
        UpdateProjectRequest request,
        [NotNullWhen(returnValue: true)] out ValidatedUpdateProjectRequest? validated
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

        if (request.InvoiceItems == null)
        {
            validated = null;
            return false;
        }

        var invoiceItems = new List<(InvoiceItemId?, InvoiceItemName)>();
        foreach (var item in request.InvoiceItems)
        {
            if (
                item == null
                || item.Name == null
                || !InvoiceItemName.TryFrom(value: item.Name, out var invoiceItemName)
            )
            {
                validated = null;
                return false;
            }

            InvoiceItemId? invoiceItemId = null;
            if (item.Id != null)
            {
                if (InvoiceItemId.TryFrom(value: item.Id.Value, out var validatedInvoiceItemId))
                {
                    invoiceItemId = validatedInvoiceItemId;
                }
                else
                {
                    validated = null;
                    return false;
                }
            }

            invoiceItems.Add((invoiceItemId, invoiceItemName));
        }

        validated = new ValidatedUpdateProjectRequest(
            Name: projectName,
            Enabled: request.Enabled.Value,
            UserIds: [.. userIds],
            InvoiceItems: [.. invoiceItems]
        );
        return true;
    }

    record ValidatedUpdateProjectRequest(
        ProjectName Name,
        bool Enabled,
        UserId[] UserIds,
        (InvoiceItemId? Id, InvoiceItemName Name)[] InvoiceItems
    );
}
