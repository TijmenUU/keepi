using FastEndpoints;
using Keepi.Api.Authorization;
using Keepi.Core.Projects;
using Microsoft.Extensions.Logging;

namespace Keepi.Api.Projects.GetAll;

public sealed class GetAllProjectsEndpoint(
    IResolveUserHelper resolveUserHelper,
    IGetProjects getProjects,
    ILogger<GetAllProjectsEndpoint> logger
) : EndpointWithoutRequest<GetAllProjectsResponse>
{
    public override void Configure()
    {
        Get("/projects");
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var user = await resolveUserHelper.GetUserOrNull(
            userClaimsPrincipal: User,
            cancellationToken: cancellationToken
        );
        if (user == null)
        {
            logger.LogDebug("Refusing to return all projects for unknown user");
            await Send.ForbiddenAsync(cancellation: cancellationToken);
            return;
        }

        var result = await getProjects.Execute(cancellationToken: cancellationToken);

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
