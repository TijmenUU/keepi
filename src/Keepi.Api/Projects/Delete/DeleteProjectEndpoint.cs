using FastEndpoints;
using Keepi.Api.Authorization;
using Keepi.Core.Projects;
using Microsoft.Extensions.Logging;

namespace Keepi.Api.Projects.Delete;

internal sealed class DeleteProjectEndpoint(
    IResolveUserHelper resolveUserHelper,
    IDeleteProject deleteProject,
    ILogger<DeleteProjectEndpoint> logger
) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/projects/{projectId}");
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var projectId = Route<int>(paramName: "ProjectId");

        var user = await resolveUserHelper.GetUserOrNull(
            userClaimsPrincipal: User,
            cancellationToken: cancellationToken
        );
        if (user == null)
        {
            logger.LogDebug(
                "Refusing to delete project with {ProjectId} by an unknown user",
                projectId
            );
            await Send.ForbiddenAsync(cancellation: cancellationToken);
            return;
        }

        var result = await deleteProject.Execute(
            projectId: projectId,
            cancellationToken: cancellationToken
        );
        if (result.TrySuccess(out _))
        {
            await Send.NoContentAsync(cancellation: cancellationToken);
        }
        else
        {
            await Send.ErrorsAsync(cancellation: cancellationToken);
        }
    }
}
