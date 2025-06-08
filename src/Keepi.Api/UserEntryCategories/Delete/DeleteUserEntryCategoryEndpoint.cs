using FastEndpoints;
using Keepi.Api.Authorization;
using Keepi.Core.UserEntryCategories;
using Microsoft.Extensions.Logging;

namespace Keepi.Api.UserEntryCategories.Delete;

public class DeleteUserUserEntryCategoryEndpoint(
    IResolveUserHelper resolveUserHelper,
    IDeleteUserEntryCategoryUseCase deleteUserEntryCategoryUseCase,
    ILogger<DeleteUserUserEntryCategoryEndpoint> logger
) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/user/entrycategories/{UserEntryCategoryId}");
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var user = await resolveUserHelper.GetUserOrNull(
            userClaimsPrincipal: User,
            cancellationToken: cancellationToken
        );
        if (user == null)
        {
            logger.LogDebug("Refusing to delete entry category for unregistered user");
            await SendForbiddenAsync(cancellation: cancellationToken);
            return;
        }

        var userEntryCategoryId = Route<int>(paramName: "UserEntryCategoryId");
        var result = await deleteUserEntryCategoryUseCase.Execute(
            userEntryCategoryId: userEntryCategoryId,
            userId: user.Id,
            cancellationToken: cancellationToken
        );

        if (result.TrySuccess(out var error))
        {
            await SendNoContentAsync(cancellation: cancellationToken);
            return;
        }

        if (error == DeleteUserEntryCategoryUseCaseError.Unknown)
        {
            await SendErrorsAsync(statusCode: 500, cancellation: cancellationToken);
            return;
        }

        await SendErrorsAsync(cancellation: cancellationToken);
        return;
    }
}
