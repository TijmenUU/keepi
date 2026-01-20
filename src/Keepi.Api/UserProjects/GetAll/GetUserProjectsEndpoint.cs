using FastEndpoints;
using Keepi.Core.UserProjects;

namespace Keepi.Api.UserProjects.GetAll;

public sealed class GetUserProjectsEndpoint(IGetUserProjectsUseCase getUserProjectsUseCase)
    : EndpointWithoutRequest<GetUserProjectsResponse>
{
    public override void Configure()
    {
        Get("/user/projects");
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var result = await getUserProjectsUseCase.Execute(cancellationToken: cancellationToken);

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
