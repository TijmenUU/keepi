using FastEndpoints;
using Keepi.Core.Users;

namespace Keepi.Api.Users.GetAll;

public sealed class GetAllUsersEndpoint(IGetAllUsersUseCase getAllUsersUseCase)
    : EndpointWithoutRequest<GetAllUsersResponse>
{
    public override void Configure()
    {
        Get("/users");
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var result = await getAllUsersUseCase.Execute(cancellationToken: cancellationToken);

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
