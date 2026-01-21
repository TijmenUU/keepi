using FastEndpoints;
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
        var result = await getUserEntriesForWeekUseCase.Execute(
            year: Route<int>(paramName: "Year"),
            weekNumber: Route<int>(paramName: "WeekNumber"),
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
                    InvoiceItemId: e.InvoiceItemId,
                    Minutes: e.Minutes,
                    Remark: e.Remark
                )),
            ]
        );
}
