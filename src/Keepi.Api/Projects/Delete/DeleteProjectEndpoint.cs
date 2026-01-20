using FastEndpoints;
using Keepi.Core.Projects;

namespace Keepi.Api.Projects.Delete;

internal sealed class DeleteProjectEndpoint(IDeleteProjectUseCase deleteProjectUseCase)
    : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/projects/{projectId}");
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var projectId = Route<int>(paramName: "ProjectId");

        var result = await deleteProjectUseCase.Execute(
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
