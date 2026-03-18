using FastEndpoints;
using Keepi.Core.Projects;

namespace Keepi.Api.Projects.GetAll;

public sealed class GetAllProjectsEndpoint(IGetAllProjectsUseCase getAllProjectsUseCase)
    : EndpointWithoutRequest<GetAllProjectsResponse>
{
    public override void Configure()
    {
        Get("/projects");
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
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
                                Id: p.Id.Value,
                                Name: p.Name.Value,
                                Enabled: p.Enabled,
                                Users:
                                [
                                    .. p.Users.Select(u => new GetAllProjectsResponseProjectUser(
                                        Id: u.Id.Value,
                                        Name: u.Name.Value
                                    )),
                                ],
                                InvoiceItems:
                                [
                                    .. p
                                        .InvoiceItems.OrderBy(i => i.Name)
                                        .Select(i => new GetAllProjectsResponseProjectInvoiceItem(
                                            Id: i.Id.Value,
                                            Name: i.Name.Value
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
            await (
                errorResult switch
                {
                    GetAllProjectsUseCaseError.UnauthenticatedUser => Send.UnauthorizedAsync(
                        cancellation: cancellationToken
                    ),
                    GetAllProjectsUseCaseError.UnauthorizedUser => Send.ForbiddenAsync(
                        cancellation: cancellationToken
                    ),
                    _ => Send.ErrorsAsync(statusCode: 500, cancellation: cancellationToken),
                }
            );
        }
    }
}
