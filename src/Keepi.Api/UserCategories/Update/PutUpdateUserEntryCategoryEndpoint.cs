using System.Diagnostics.CodeAnalysis;
using FastEndpoints;
using Keepi.Api.Authorization;
using Keepi.Core.UseCases;
using Microsoft.Extensions.Logging;

namespace Keepi.Api.UserCategories.Update;

public class PutUpdateUserEntryCategoryEndpoint(
  IResolveUserHelper resolveUserHelper,
  IUpdateEntryCategoryUseCase updateEntryCategoryUseCase,
  ILogger<PutUpdateUserEntryCategoryEndpoint> logger)
   : Endpoint<PutUpdateUserEntryCategoryRequest>
{
  public override void Configure()
  {
    Put("/user/entrycategories/{EntryCategoryId}");
  }

  public override async Task HandleAsync(
    PutUpdateUserEntryCategoryRequest request,
    CancellationToken cancellationToken)
  {
    var user = await resolveUserHelper.GetUserOrNull(
      userClaimsPrincipal: User,
      cancellationToken: cancellationToken);
    if (user == null)
    {
      logger.LogDebug("Refusing to create entry category for unregistered user");
      await SendForbiddenAsync(cancellation: cancellationToken);
      return;
    }

    var entryCategoryId = Route<int>(paramName: "EntryCategoryId");
    if (!TryGetValidatedModel(request, out var validatedRequest))
    {
      await SendErrorsAsync(cancellation: cancellationToken);
      return;
    }

    var result = await updateEntryCategoryUseCase.Execute(
      entryCategoryId: entryCategoryId,
      userId: user.Id,
      name: validatedRequest.Name,
      enabled: validatedRequest.Enabled,
      activeFrom: validatedRequest.ActiveFrom,
      activeTo: validatedRequest.ActiveTo,
      cancellationToken: cancellationToken);

    if (result.TrySuccess(out var error))
    {
      await SendNoContentAsync(cancellation: cancellationToken);
      return;
    }

    if (error == UpdateEntryCategoryUseCaseError.Unknown)
    {
      await SendErrorsAsync(statusCode: 500, cancellation: cancellationToken);
      return;
    }

    await SendErrorsAsync(cancellation: cancellationToken);
    return;
  }

  private static bool TryGetValidatedModel(
    PutUpdateUserEntryCategoryRequest request,
    [NotNullWhen(returnValue: true)] out ValidatedPostCreateEntryCategoryRequest? validated)
  {
    if (request == null)
    {
      validated = null;
      return false;
    }

    if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > 64 || request.Enabled == null)
    {
      validated = null;
      return false;
    }

    DateOnly? parsedActiveFrom = null;
    if (request.ActiveFrom != null)
    {
      parsedActiveFrom = request.ActiveFrom.GetIsoDateOrNull();
      if (parsedActiveFrom == null)
      {
        validated = null;
        return false;
      }
    }

    DateOnly? parsedActiveTo = null;
    if (request.ActiveTo != null)
    {
      parsedActiveTo = request.ActiveTo.GetIsoDateOrNull();
      if (parsedActiveTo == null)
      {
        validated = null;
        return false;
      }
    }

    validated = new ValidatedPostCreateEntryCategoryRequest(
      Name: request.Name,
      Enabled: request.Enabled.Value,
      ActiveFrom: parsedActiveFrom,
      ActiveTo: parsedActiveTo);
    return true;
  }

  record ValidatedPostCreateEntryCategoryRequest(string Name, bool Enabled, DateOnly? ActiveFrom, DateOnly? ActiveTo);
}