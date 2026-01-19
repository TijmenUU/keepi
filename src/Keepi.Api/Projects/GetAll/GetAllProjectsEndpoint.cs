using FastEndpoints;
using Keepi.Core.Projects;
using Keepi.Core.Users;
using Microsoft.Extensions.Logging;

namespace Keepi.Api.Projects.GetAll;

public sealed class GetAllProjectsEndpoint(
    IResolveUser resolveUser,
    IGetAllProjectsUseCase getAllProjectsUseCase,
    ILogger<GetAllProjectsEndpoint> logger
) : EndpointWithoutRequest<GetAllProjectsResponse>
{
    public override void Configure()
    {
        Get("/projects");
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var resolveUserResult = await resolveUser.Execute(cancellationToken: cancellationToken);
        if (!resolveUserResult.TrySuccess(out var user, out _))
        {
            logger.LogDebug("Refusing to return all projects for unknown user");
            await Send.ForbiddenAsync(cancellation: cancellationToken);
            return;
        }

        var result = await getAllProjectsUseCase.Execute(cancellationToken: cancellationToken);

        if (result.TrySuccess(out var successResult, out var errorResult))
        {
            await Send.OkAsync(
                response: new GetAllProjectsResponse(
                    Projects:
                    [
                        .. successResult
                            .Projects.OrderBy(p => !p.Enabled)
                            .ThenBy(p => p.Name)
                            .Select(p => new GetAllProjectsResponseProject(
                                Id: p.Id,
                                Name: p.Name,
                                Enabled: p.Enabled,
                                Users:
                                [
                                    .. p.Users.Select(u => new GetAllProjectsResponseProjectUser(
                                        Id: u.Id,
                                        Name: u.Name
                                    )),
                                ],
                                InvoiceItems:
                                [
                                    .. p
                                        .InvoiceItems.OrderBy(i => i.Name)
                                        .Select(i => new GetAllProjectsResponseProjectInvoiceItem(
                                            Id: i.Id,
                                            Name: i.Name
                                        )),
                                ]
                            )),
                    ]
                ),
                cancellation: cancellationToken
            );
        }
        else
        {
            await Send.ErrorsAsync(statusCode: 500, cancellation: cancellationToken);
        }
    }
}
