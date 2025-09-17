using FastEndpoints;
using Keepi.Api.Authorization;
using Keepi.Core.UserEntryCategories;
using Microsoft.Extensions.Logging;

namespace Keepi.Api.UserEntryCategories.GetAll;

public class GetUserUserEntryCategoriesEndpoint(
    IResolveUserHelper resolveUserHelper,
    IGetUserUserEntryCategories getUserUserEntryCategories,
    ILogger<GetUserUserEntryCategoriesEndpoint> logger
) : EndpointWithoutRequest<GetUserUserEntryCategoriesResponse>
{
    public override void Configure()
    {
        Get("/user/entrycategories");
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var user = await resolveUserHelper.GetUserOrNull(
            userClaimsPrincipal: User,
            cancellationToken: cancellationToken
        );
        if (user == null)
        {
            logger.LogDebug("Refusing to return entry categories for unregistered user");
            await Send.ForbiddenAsync(cancellation: cancellationToken);
            return;
        }

        var userEntryCategories = await getUserUserEntryCategories.Execute(
            userId: user.Id,
            cancellationToken: cancellationToken
        );

        await Send.OkAsync(
            response: new GetUserUserEntryCategoriesResponse(
                Categories: userEntryCategories
                    .Select(c => new GetUserUserEntryCategoriesResponseCategory(
                        Id: c.Id,
                        Name: c.Name,
                        Ordinal: c.Ordinal,
                        Enabled: c.Enabled,
                        ActiveFrom: c.ActiveFrom,
                        ActiveTo: c.ActiveTo
                    ))
                    .ToArray()
            ),
            cancellation: cancellationToken
        );
    }
}
