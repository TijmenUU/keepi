using FastEndpoints;
using Keepi.Core.Users;
using Microsoft.Extensions.Logging;

namespace Keepi.Api.Users.Get;

public sealed class GetUserEndpoint(IResolveUser resolveUser, ILogger<GetUserEndpoint> logger)
    : EndpointWithoutRequest<GetUserResponse>
{
    public override void Configure()
    {
        Get("/user");
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var resolveUserResult = await resolveUser.Execute(cancellationToken: cancellationToken);
        if (!resolveUserResult.TrySuccess(out var user, out _))
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
