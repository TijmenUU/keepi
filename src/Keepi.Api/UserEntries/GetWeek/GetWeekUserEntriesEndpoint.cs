using FastEndpoints;
using Keepi.Api.Authorization;
using Keepi.Core.Entries;
using Microsoft.Extensions.Logging;

namespace Keepi.Api.UserEntries.GetWeek;

public class GetWeekUserEntriesEndpoint(
  IResolveUserHelper resolveUserHelper,
  IGetUserEntriesForWeekUseCase getUserEntriesForWeekUseCase,
  ILogger<GetWeekUserEntriesEndpoint> logger)
: EndpointWithoutRequest<GetWeekUserEntriesResponse>
{
  public override void Configure()
  {
    Get("/user/entries/year/{Year}/week/{WeekNumber}");
  }

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
    var user = await resolveUserHelper.GetUserOrNull(
      userClaimsPrincipal: User,
      cancellationToken: cancellationToken);
    if (user == null)
    {
      logger.LogDebug("Refusing to get week entries for unregistered user");
      await SendForbiddenAsync(cancellation: cancellationToken);
      return;
    }

    var result = await getUserEntriesForWeekUseCase.Execute(
      userId: user.Id,
      year: Route<int>(paramName: "Year"),
      weekNumber: Route<int>(paramName: "WeekNumber"),
      cancellationToken: cancellationToken);

    await SendOkAsync(
      response: new GetWeekUserEntriesResponse(
        Monday: MapToResponseDay(result.Monday),
        Tuesday: MapToResponseDay(result.Tuesday),
        Wednesday: MapToResponseDay(result.Wednesday),
        Thursday: MapToResponseDay(result.Thursday),
        Friday: MapToResponseDay(result.Friday),
        Saturday: MapToResponseDay(result.Saturday),
        Sunday: MapToResponseDay(result.Sunday)),
      cancellation: cancellationToken);
  }

  private static GetWeekUserEntriesResponseDay MapToResponseDay(GetUserEntriesForWeekUseCaseOutputDay input)
    => new GetWeekUserEntriesResponseDay(
      Entries: input.Entries
        .Select(e => new GetWeekUserEntriesResponseDayEntry(
          EntryCategoryId: e.EntryCategoryId,
          Minutes: e.Minutes,
          Remark: e.Remark))
        .ToArray());
}