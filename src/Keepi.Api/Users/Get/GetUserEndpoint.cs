using FastEndpoints;
using Keepi.Core.Users;

namespace Keepi.Api.Users.Get;

public sealed class GetUserEndpoint(IGetUserUseCase getUserUseCase)
    : EndpointWithoutRequest<GetUserResponse>
{
    public override void Configure()
    {
        Get("/user");
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var result = await getUserUseCase.Execute(cancellationToken: cancellationToken);
        if (result.TrySuccess(out var successResult, out var errorResult))
        {
            await Send.OkAsync(
                response: new GetUserResponse(
                    Id: successResult.Id,
                    Name: successResult.Name,
                    EmailAddress: successResult.EmailAddress
                ),
                cancellation: cancellationToken
            );
            return;
        }

        await (
            errorResult switch
            {
                GetUserUseCaseError.UnauthenticatedUser => Send.UnauthorizedAsync(
                    cancellation: cancellationToken
                ),
                _ => Send.ErrorsAsync(statusCode: 500, cancellation: cancellationToken),
            }
        );
    }
}
