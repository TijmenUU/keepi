using FastEndpoints;
using Keepi.Core.Entries;
using Keepi.Core.Users;
using Microsoft.Extensions.Logging;

namespace Keepi.Api.UserEntries.GetWeek;

public sealed class GetWeekUserEntriesEndpoint(
    IResolveUser resolveUser,
    IGetUserEntriesForWeekUseCase getUserEntriesForWeekUseCase,
    ILogger<GetWeekUserEntriesEndpoint> logger
) : EndpointWithoutRequest<GetWeekUserEntriesResponse>
{
    public override void Configure()
    {
        Get("/user/entries/year/{Year}/week/{WeekNumber}");
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var resolveUserResult = await resolveUser.Execute(cancellationToken: cancellationToken);
        if (!resolveUserResult.TrySuccess(out var user, out _))
        {
            logger.LogDebug("Refusing to get week entries for unknown user");
            await Send.ForbiddenAsync(cancellation: cancellationToken);
            return;
        }

        var result = await getUserEntriesForWeekUseCase.Execute(
            userId: user.Id,
            year: Route<int>(paramName: "Year"),
            weekNumber: Route<int>(paramName: "WeekNumber"),
            cancellationToken: cancellationToken
        );
        if (result.TrySuccess(out var successResult, out _))
        {
            await Send.OkAsync(
                response: new GetWeekUserEntriesResponse(
                    Monday: MapToResponseDay(successResult.Monday),
                    Tuesday: MapToResponseDay(successResult.Tuesday),
                    Wednesday: MapToResponseDay(successResult.Wednesday),
                    Thursday: MapToResponseDay(successResult.Thursday),
                    Friday: MapToResponseDay(successResult.Friday),
                    Saturday: MapToResponseDay(successResult.Saturday),
                    Sunday: MapToResponseDay(successResult.Sunday)
                ),
                cancellation: cancellationToken
            );
            return;
        }

        await Send.ErrorsAsync(statusCode: 500, cancellation: cancellationToken);
    }

    private static GetWeekUserEntriesResponseDay MapToResponseDay(
        GetUserEntriesForWeekUseCaseOutputDay input
    ) =>
        new GetWeekUserEntriesResponseDay(
            Entries:
            [
                .. input.Entries.Select(e => new GetWeekUserEntriesResponseDayEntry(
                    InvoiceItemId: e.InvoiceItemId,
                    Minutes: e.Minutes,
                    Remark: e.Remark
                )),
            ]
        );
}
