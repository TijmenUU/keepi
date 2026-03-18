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
        var routeProjectId = Route<int>(paramName: "ProjectId");
        if (!ProjectId.TryFrom(value: routeProjectId, out var projectId))
        {
            await Send.ErrorsAsync(cancellation: cancellationToken);
            return;
        }

        var result = await deleteProjectUseCase.Execute(
            projectId: projectId,
            cancellationToken: cancellationToken
        );
        if (result.TrySuccess(out var errorResult))
        {
            await Send.NoContentAsync(cancellation: cancellationToken);
        }
        else
        {
            await (
                errorResult switch
                {
                    DeleteProjectUseCaseError.UnauthenticatedUser => Send.UnauthorizedAsync(
                        cancellation: cancellationToken
                    ),
                    DeleteProjectUseCaseError.UnauthorizedUser => Send.ForbiddenAsync(
                        cancellation: cancellationToken
                    ),
                    DeleteProjectUseCaseError.Unknown => Send.ErrorsAsync(
                        statusCode: 500,
                        cancellation: cancellationToken
                    ),
                    _ => Send.ErrorsAsync(cancellation: cancellationToken),
                }
            );
        }
    }
}
