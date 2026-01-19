using System.Diagnostics.CodeAnalysis;
using FastEndpoints;
using Keepi.Core.Projects;
using Keepi.Core.Users;
using Microsoft.Extensions.Logging;

namespace Keepi.Api.Projects.Update;

internal sealed class UpdateProjectEndpoint(
    IResolveUser resolveUser,
    IUpdateProjectUseCase updateProjectUseCase,
    ILogger<UpdateProjectEndpoint> logger
) : Endpoint<UpdateProjectRequest>
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
        var projectId = Route<int>(paramName: "ProjectId");
        var resolveUserResult = await resolveUser.Execute(cancellationToken: cancellationToken);
        if (!resolveUserResult.TrySuccess(out var user, out _))
        {
            logger.LogDebug(
                "Refusing to update project with ID {ProjectId} by an unknown user",
                projectId
            );
            await Send.ForbiddenAsync(cancellation: cancellationToken);
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

            switch (errorResult)
            {
                case UpdateProjectUseCaseError.Unknown:
                    await Send.ErrorsAsync(statusCode: 500, cancellation: cancellationToken);
                    break;

                case UpdateProjectUseCaseError.UnknownProjectId:
                    await Send.NotFoundAsync(cancellation: cancellationToken);
                    break;

                default:
                    await Send.ErrorsAsync(cancellation: cancellationToken);
                    break;
            }

            return;
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
        if (request.Name == null || request.Enabled == null)
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
            request.InvoiceItems == null
            || request.InvoiceItems.Any(i => i == null || string.IsNullOrEmpty(i.Name))
        )
        {
            validated = null;
            return false;
        }

        validated = new ValidatedUpdateProjectRequest(
            Name: request.Name,
            Enabled: request.Enabled.Value,
            UserIds: request.UserIds?.Select(i => i ?? 0).ToArray() ?? [],
            InvoiceItems: request
                .InvoiceItems?.Select(i => (Id: i?.Id, Name: i?.Name ?? string.Empty))
                .ToArray()
                ?? []
        );
        return true;
    }

    record ValidatedUpdateProjectRequest(
        string Name,
        bool Enabled,
        int[] UserIds,
        (int? Id, string Name)[] InvoiceItems
    );
}
