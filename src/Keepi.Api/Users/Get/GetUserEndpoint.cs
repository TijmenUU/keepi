using FastEndpoints;
using Keepi.Api.Authorization;
using Keepi.Core.Users;

namespace Keepi.Api.Users.Get;

public class GetUserEndpoint(IGetUserExists getUserExists) : EndpointWithoutRequest<GetUserResponse>
{
    public override void Configure()
    {
        Get("/user");
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserInfo(out var userInfo))
        {
            await Send.ForbiddenAsync(cancellation: cancellationToken);
            return;
        }

        await Send.OkAsync(
            response: new GetUserResponse(
                name: userInfo.Name,
                registered: await getUserExists.Execute(
                    externalId: userInfo.ExternalId,
                    emailAddress: userInfo.EmailAddress,
                    cancellationToken: cancellationToken
                )
            ),
            cancellation: cancellationToken
        );
    }
}
