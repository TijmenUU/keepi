using FastEndpoints;
using Keepi.Api.Authorization;
using Microsoft.Extensions.Logging;

namespace Keepi.Api.Users.Get;

public sealed class GetUserEndpoint(
    IResolveUserHelper resolveUserHelper,
    ILogger<GetUserEndpoint> logger
) : EndpointWithoutRequest<GetUserResponse>
{
    public override void Configure()
    {
        Get("/user");
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var user = await resolveUserHelper.GetUserOrNull(
            userClaimsPrincipal: User,
            cancellationToken: cancellationToken
        );
        if (user == null)
        {
            logger.LogDebug("Refusing to return user for unknown user");
            await Send.ForbiddenAsync(cancellation: cancellationToken);
            return;
        }

        await Send.OkAsync(
            response: new GetUserResponse(
                Id: user.Id,
                Name: user.Name,
                EmailAddress: user.EmailAddress
            ),
            cancellation: cancellationToken
        );
    }
}
