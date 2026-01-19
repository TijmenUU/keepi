using FastEndpoints;
using Keepi.Core.Projects;
using Keepi.Core.Users;
using Microsoft.Extensions.Logging;

namespace Keepi.Api.Projects.Delete;

internal sealed class DeleteProjectEndpoint(
    IResolveUser resolveUser,
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

        var resolveUserResult = await resolveUser.Execute(cancellationToken: cancellationToken);
        if (!resolveUserResult.TrySuccess(out var user, out _))
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
