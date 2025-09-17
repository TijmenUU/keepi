using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using FastEndpoints;
using Keepi.Api.Authorization;
using Keepi.Api.UserEntries.GetWeek;
using Keepi.Core.Entries;
using Microsoft.Extensions.Logging;

namespace Keepi.Api.UserEntries.UpdateWeek;

public class PutUpdateWeekUserEntriesEndpoint(
    IResolveUserHelper resolveUserHelper,
    IUpdateWeekUserEntriesUseCase updateWeekUserEntriesUseCase,
    ILogger<PutUpdateWeekUserEntriesEndpoint> logger
) : Endpoint<PutUpdateWeekUserEntriesRequest>
{
    public override void Configure()
    {
        Put("/user/entries/year/{Year}/week/{WeekNumber}");
    }

    public override async Task HandleAsync(
        PutUpdateWeekUserEntriesRequest request,
        CancellationToken cancellationToken
    )
    {
        var user = await resolveUserHelper.GetUserOrNull(
            userClaimsPrincipal: User,
            cancellationToken: cancellationToken
        );
        if (user == null)
        {
            logger.LogDebug("Refusing to update week entries for unregistered user");
            await Send.ForbiddenAsync(cancellation: cancellationToken);
            return;
        }

        if (!TryGetValidatedModel(request: request, out var validatedRequest))
        {
            await Send.ErrorsAsync(cancellation: cancellationToken);
            return;
        }

        var year = Route<int>(paramName: "Year");
        var weekNumber = Route<int>(paramName: "WeekNumber");

        var result = await updateWeekUserEntriesUseCase.Execute(
            userId: user.Id,
            year: year,
            weekNumber: weekNumber,
            input: validatedRequest,
            cancellationToken: cancellationToken
        );

        if (result.TrySuccess(out var error))
        {
            await Send.CreatedAtAsync<GetWeekUserEntriesEndpoint>(
                routeValues: new { Year = year, WeekNumber = weekNumber },
                cancellation: cancellationToken
            );
            return;
        }

        if (error == UpdateWeekUserEntriesUseCaseError.Unknown)
        {
            await Send.ErrorsAsync(statusCode: 500, cancellation: cancellationToken);
            return;
        }

        await Send.ErrorsAsync(cancellation: cancellationToken);
        return;
    }

    private static bool TryGetValidatedModel(
        PutUpdateWeekUserEntriesRequest request,
        [NotNullWhen(returnValue: true)] out UpdateWeekUserEntriesUseCaseInput? validated
    )
    {
        if (request == null)
        {
            validated = null;
            return false;
        }

        if (
            request.Monday == null
            || request.Tuesday == null
            || request.Wednesday == null
            || request.Thursday == null
            || request.Friday == null
            || request.Saturday == null
            || request.Sunday == null
        )
        {
            validated = null;
            return false;
        }

        PutUpdateWeekUserEntriesRequestDay[] days =
        [
            request.Monday,
            request.Tuesday,
            request.Wednesday,
            request.Thursday,
            request.Friday,
            request.Saturday,
            request.Sunday,
        ];
        List<UpdateWeekUserEntriesUseCaseInputDay> validatedDays = [];
        foreach (var day in days)
        {
            if (day.Entries == null)
            {
                validated = null;
                return false;
            }

            List<UpdateWeekUserEntriesUseCaseInputDayEntry> validatedEntries = [];
            foreach (var entry in day.Entries)
            {
                if (entry == null)
                {
                    validated = null;
                    return false;
                }

                if (entry.EntryCategoryId == null)
                {
                    validated = null;
                    return false;
                }

                if (!UserEntryEntity.IsValidMinutes(entry.Minutes))
                {
                    validated = null;
                    return false;
                }

                if (!UserEntryEntity.IsValidRemark(entry.Remark))
                {
                    validated = null;
                    return false;
                }

                validatedEntries.Add(
                    new UpdateWeekUserEntriesUseCaseInputDayEntry(
                        EntryCategoryId: entry.EntryCategoryId.Value,
                        Minutes: entry.Minutes.Value,
                        Remark: entry.Remark
                    )
                );
            }

            Debug.Assert(day.Entries.Length == validatedEntries.Count);

            validatedDays.Add(
                new UpdateWeekUserEntriesUseCaseInputDay(Entries: [.. validatedEntries])
            );
        }

        Debug.Assert(days.Length == 7 && days.Length == validatedDays.Count);

        validated = new UpdateWeekUserEntriesUseCaseInput(
            Monday: validatedDays[0],
            Tuesday: validatedDays[1],
            Wednesday: validatedDays[2],
            Thursday: validatedDays[3],
            Friday: validatedDays[4],
            Saturday: validatedDays[5],
            Sunday: validatedDays[6]
        );
        return true;
    }
}
