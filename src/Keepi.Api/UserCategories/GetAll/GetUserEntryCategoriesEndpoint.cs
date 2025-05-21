using FastEndpoints;
using Keepi.Api.Authorization;
using Keepi.Core.EntryCategories;
using Microsoft.Extensions.Logging;

namespace Keepi.Api.UserCategories.GetAll;

public class GetUserEntryCategoriesEndpoint(
  IResolveUserHelper resolveUserHelper,
  IGetUserEntryCategories getUserEntryCategories,
  ILogger<GetUserEntryCategoriesEndpoint> logger)
 : EndpointWithoutRequest<GetUserEntryCategoriesResponse>
{
  public override void Configure()
  {
    Get("/user/entrycategories");
  }

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
    var user = await resolveUserHelper.GetUserOrNull(
      userClaimsPrincipal: User,
      cancellationToken: cancellationToken);
    if (user == null)
    {
      logger.LogDebug("Refusing to return entry categories for unregistered user");
      await SendForbiddenAsync(cancellation: cancellationToken);
      return;
    }

    var entryCategories = await getUserEntryCategories.Execute(
      userId: user.Id,
      cancellationToken: cancellationToken);

    await SendAsync(
      response: new GetUserEntryCategoriesResponse(Categories: entryCategories
        .Select(c => new GetUserEntryCategoriesResponseCategory(
          Id: c.Id,
          Name: c.Name,
          Enabled: c.Enabled,
          ActiveFrom: c.ActiveFrom,
          ActiveTo: c.ActiveTo
        ))
        .ToArray()),
      cancellation: cancellationToken);
  }
}