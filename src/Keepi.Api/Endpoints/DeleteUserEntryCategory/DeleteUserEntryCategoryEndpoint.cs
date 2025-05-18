using FastEndpoints;
using Keepi.Api.Helpers;
using Keepi.Core.UseCases;
using Microsoft.Extensions.Logging;

namespace Keepi.Api.Endpoints.DeleteUserEntryCategory;

public class DeleteUserEntryCategoryEndpoint(
  IResolveUserHelper resolveUserHelper,
  IDeleteEntryCategoryUseCase deleteEntryCategoryUseCase,
  ILogger<DeleteUserEntryCategoryEndpoint> logger)
   : EndpointWithoutRequest
{
  public override void Configure()
  {
    Delete("/user/entrycategories/{EntryCategoryId}");
  }

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
    var user = await resolveUserHelper.GetUserOrNull(
      userClaimsPrincipal: User,
      cancellationToken: cancellationToken);
    if (user == null)
    {
      logger.LogDebug("Refusing to delete entry category for unregistered user");
      await SendForbiddenAsync(cancellation: cancellationToken);
      return;
    }

    var entryCategoryId = Route<int>(paramName: "EntryCategoryId");
    var result = await deleteEntryCategoryUseCase.Execute(
      entryCategoryId: entryCategoryId,
      userId: user.Id,
      cancellationToken: cancellationToken);

    if (result.TrySuccess(out var error))
    {
      await SendNoContentAsync(cancellation: cancellationToken);
      return;
    }

    if (error == DeleteEntryCategoryUseCaseError.Unknown)
    {
      await SendErrorsAsync(statusCode: 500, cancellation: cancellationToken);
      return;
    }

    await SendErrorsAsync(cancellation: cancellationToken);
    return;
  }
}