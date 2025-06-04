using System.Diagnostics.CodeAnalysis;
using FastEndpoints;
using Keepi.Api.Authorization;
using Keepi.Core.UserEntryCategories;
using Microsoft.Extensions.Logging;

namespace Keepi.Api.UserEntryCategories.Update;

public class PutUpdateUserUserEntryCategoryEndpoint(
  IResolveUserHelper resolveUserHelper,
  IUpdateUserEntryCategoryUseCase updateUserEntryCategoryUseCase,
  ILogger<PutUpdateUserUserEntryCategoryEndpoint> logger)
   : Endpoint<PutUpdateUserUserEntryCategoryRequest>
{
  public override void Configure()
  {
    Put("/user/entrycategories/{UserEntryCategoryId}");
  }

  public override async Task HandleAsync(
    PutUpdateUserUserEntryCategoryRequest request,
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

    var userEntryCategoryId = Route<int>(paramName: "UserEntryCategoryId");
    if (!TryGetValidatedModel(request, out var validatedRequest))
    {
      await SendErrorsAsync(cancellation: cancellationToken);
      return;
    }

    var result = await updateUserEntryCategoryUseCase.Execute(
      userEntryCategoryId: userEntryCategoryId,
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

    if (error == UpdateUserEntryCategoryUseCaseError.Unknown)
    {
      await SendErrorsAsync(statusCode: 500, cancellation: cancellationToken);
      return;
    }

    await SendErrorsAsync(cancellation: cancellationToken);
    return;
  }

  private static bool TryGetValidatedModel(
    PutUpdateUserUserEntryCategoryRequest request,
    [NotNullWhen(returnValue: true)] out ValidatedPostCreateUserEntryCategoryRequest? validated)
  {
    if (request == null)
    {
      validated = null;
      return false;
    }

    if (!UserEntryCategoryEntity.IsValidName(request.Name))
    {
      validated = null;
      return false;
    }

    if (request.Enabled == null)
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

    if (!UserEntryCategoryEntity.IsValidActiveDateRange(from: parsedActiveFrom, to: parsedActiveTo))
    {
      validated = null;
      return false;
    }

    validated = new ValidatedPostCreateUserEntryCategoryRequest(
      Name: request.Name,
      Enabled: request.Enabled.Value,
      ActiveFrom: parsedActiveFrom,
      ActiveTo: parsedActiveTo);
    return true;
  }

  record ValidatedPostCreateUserEntryCategoryRequest(string Name, bool Enabled, DateOnly? ActiveFrom, DateOnly? ActiveTo);
}