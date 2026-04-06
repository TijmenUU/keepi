using FastEndpoints;
using Keepi.Core;
using Keepi.Core.Entries;

namespace Keepi.Api.UserEntries.GetWeek;

public sealed class GetWeekUserEntriesEndpoint(
    IGetUserEntriesForWeekUseCase getUserEntriesForWeekUseCase
) : EndpointWithoutRequest<GetWeekUserEntriesResponse>
{
    public override void Configure()
    {
        Get("/user/entries/year/{Year}/week/{WeekNumber}");
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var routeYearNumber = Route<int>(paramName: "Year");
        if (!Year.TryFrom(value: routeYearNumber, out var year))
        {
            await Send.ErrorsAsync(cancellation: cancellationToken);
            return;
        }

        var routeWeekNumber = Route<int>(paramName: "WeekNumber");
        if (!WeekNumber.TryFrom(value: routeWeekNumber, out var weekNumber))
        {
            await Send.ErrorsAsync(cancellation: cancellationToken);
            return;
        }

        var result = await getUserEntriesForWeekUseCase.Execute(
            week: new(Year: year, Number: weekNumber),
            cancellationToken: cancellationToken
        );
        if (result.TrySuccess(out var successResult, out var errorResult))
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

        await (
            errorResult switch
            {
                GetUserEntriesForWeekUseCaseError.UnauthenticatedUser => Send.UnauthorizedAsync(
                    cancellation: cancellationToken
                ),
                GetUserEntriesForWeekUseCaseError.UnauthorizedUser => Send.ForbiddenAsync(
                    cancellation: cancellationToken
                ),
                _ => Send.ErrorsAsync(statusCode: 500, cancellation: cancellationToken),
            }
        );
    }

    private static GetWeekUserEntriesResponseDay MapToResponseDay(
        GetUserEntriesForWeekUseCaseOutputDay input
    ) =>
        new GetWeekUserEntriesResponseDay(
            Entries:
            [
                .. input.Entries.Select(e => new GetWeekUserEntriesResponseDayEntry(
                    InvoiceItemId: e.InvoiceItemId.Value,
                    Minutes: e.Minutes.Value,
                    Remark: e.Remark?.Value
                )),
            ]
        );
}
