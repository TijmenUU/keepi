using FastEndpoints;
using Keepi.Api.Authorization;
using Keepi.Core.UserProjects;
using Microsoft.Extensions.Logging;

namespace Keepi.Api.UserProjects.GetAll;

public sealed class GetUserProjectsEndpoint(
    IResolveUserHelper resolveUserHelper,
    IGetUserProjectsUseCase getUserProjectsUseCase,
    ILogger<GetUserProjectsEndpoint> logger
) : EndpointWithoutRequest<GetUserProjectsResponse>
{
    public override void Configure()
    {
        Get("/user/projects");
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var user = await resolveUserHelper.GetUserOrNull(
            userClaimsPrincipal: User,
            cancellationToken: cancellationToken
        );
        if (user == null)
        {
            logger.LogDebug("Refusing to return user projects for unknown user");
            await Send.ForbiddenAsync(cancellation: cancellationToken);
            return;
        }

        var result = await getUserProjectsUseCase.Execute(
            userId: user.Id,
            cancellationToken: cancellationToken
        );

        if (result.TrySuccess(out var successResult, out var errorResult))
        {
            await Send.OkAsync(
                response: new GetUserProjectsResponse(
                    Projects:
                    [
                        .. successResult
                            .Projects.OrderBy(p => !p.Enabled)
                            .ThenBy(p => p.Name)
                            .Select(p => new GetUserProjectsResponseProject(
                                Id: p.Id,
                                Name: p.Name,
                                Enabled: p.Enabled,
                                InvoiceItems:
                                [
                                    .. p
                                        .InvoiceItems.OrderBy(i => i.Customization.Ordinal)
                                        .ThenBy(i => i.Name)
                                        .Select(i => new GetUserProjectsResponseProjectInvoiceItem(
                                            Id: i.Id,
                                            Name: i.Name,
                                            Ordinal: i.Customization.Ordinal,
                                            Color: i.Customization.Color?.ToHexColorString()
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
