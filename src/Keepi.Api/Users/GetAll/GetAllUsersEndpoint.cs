using FastEndpoints;
using Keepi.Api.Authorization;
using Keepi.Core.Users;
using Microsoft.Extensions.Logging;

namespace Keepi.Api.Users.Get;

public sealed class GetAllUsersEndpoint(
    IResolveUserHelper resolveUserHelper,
    IGetUsers getUsers,
    ILogger<GetAllUsersEndpoint> logger
) : EndpointWithoutRequest<GetAllUsersResponse>
{
    public override void Configure()
    {
        Get("/users");
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var user = await resolveUserHelper.GetUserOrNull(
            userClaimsPrincipal: User,
            cancellationToken: cancellationToken
        );
        if (user == null)
        {
            logger.LogDebug("Refusing to return all users for unknown user");
            await Send.ForbiddenAsync(cancellation: cancellationToken);
            return;
        }

        var result = await getUsers.Execute(cancellationToken: cancellationToken);

        if (result.TrySuccess(out var successResult, out _))
        {
            await Send.OkAsync(
                response: new GetAllUsersResponse(
                    Users: successResult
                        .Users.Select(u => new GetAllUsersResponseUser(
                            Id: u.Id,
                            Name: u.Name,
                            EmailAddress: u.EmailAddress
                        ))
                        .ToArray()
                ),
                cancellation: cancellationToken
            );
            return;
        }

        await Send.ErrorsAsync(statusCode: 500, cancellation: cancellationToken);
    }
}
