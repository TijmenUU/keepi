using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Keepi.App.Models;
using Keepi.Core;
using Keepi.Core.Entries;
using Keepi.Core.UserProjects;
using Microsoft.Extensions.Logging;

namespace Keepi.App.Services;

public interface IUserEntryWeekService
{
    Task<IValueOrErrorResult<UserEntryWeekInput[], GetWeekInputsError>> GetWeekInputs(
        int year,
        int weekNumber,
        CancellationToken cancellationToken
    );
}

public enum GetWeekInputsError
{
    Unknown = 0,
    NoUserProjects = 1,
}

internal sealed class UserEntryWeekService(
    IGetUserProjectsUseCase getUserProjectsUseCase,
    IGetUserEntriesForWeekUseCase getUserEntriesForWeekUseCase,
    ILogger<UserEntryWeekService> logger
) : IUserEntryWeekService
{
    public async Task<IValueOrErrorResult<UserEntryWeekInput[], GetWeekInputsError>> GetWeekInputs(
        int year,
        int weekNumber,
        CancellationToken cancellationToken
    )
    {
        var userProjectsResult = await getUserProjectsUseCase.Execute(
            cancellationToken: cancellationToken
        );
        if (!userProjectsResult.TrySuccess(out var userProjects, out var userProjectsError))
        {
            logger.LogError(
                "Unexpected use case error {Error} when fetching user projects",
                userProjectsError
            );
            return Result.Failure<UserEntryWeekInput[], GetWeekInputsError>(
                GetWeekInputsError.Unknown
            );
        }

        if (userProjects.Projects.Length <= 0)
        {
            logger.LogWarning("User has no accessible projects");
            return Result.Failure<UserEntryWeekInput[], GetWeekInputsError>(
                GetWeekInputsError.NoUserProjects
            );
        }

        var entriesResult = await getUserEntriesForWeekUseCase.Execute(
            year: year,
            weekNumber: weekNumber,
            cancellationToken: cancellationToken
        );

        if (!entriesResult.TrySuccess(out var entries, out var userEntriesError))
        {
            logger.LogError(
                "Unexpected use case error {Error} when fetching user entries for weeknumber {WeekNumber} of year {Year}",
                userEntriesError,
                weekNumber,
                year
            );
            return Result.Failure<UserEntryWeekInput[], GetWeekInputsError>(
                GetWeekInputsError.Unknown
            );
        }

        var invoiceItems = userProjects
            .Projects.SelectMany(p =>
                p.InvoiceItems.Select(i => new
                {
                    ProjectName = p.Name,
                    ProjectEnabled = p.Enabled,
                    InvoiceItemId = i.Id,
                    InvoiceItemName = i.Name,
                    Ordinal = i.Customization.Ordinal,
                    Colour = i.Customization.Color,
                })
            )
            .OrderBy(i => !i.ProjectEnabled)
            .ThenBy(i => i.Ordinal)
            .ThenBy(i => i.ProjectName)
            .ThenBy(i => i.InvoiceItemName)
            .ToArray();

        // TODO filter out rows where the project is disabled and no minutes are logged for the week

        return Result.Success<UserEntryWeekInput[], GetWeekInputsError>(
            invoiceItems
                .Select(i => new UserEntryWeekInput(
                    enabled: i.ProjectEnabled,
                    invoiceItemId: i.InvoiceItemId,
                    invoiceItemName: i.InvoiceItemName,
                    minutesMonday: entries
                        .Monday.Entries.Where(e => e.InvoiceItemId == i.InvoiceItemId)
                        .Sum(e => e.Minutes),
                    minutesTuesday: entries
                        .Tuesday.Entries.Where(e => e.InvoiceItemId == i.InvoiceItemId)
                        .Sum(e => e.Minutes),
                    minutesWednesday: entries
                        .Wednesday.Entries.Where(e => e.InvoiceItemId == i.InvoiceItemId)
                        .Sum(e => e.Minutes),
                    minutesThursday: entries
                        .Thursday.Entries.Where(e => e.InvoiceItemId == i.InvoiceItemId)
                        .Sum(e => e.Minutes),
                    minutesFriday: entries
                        .Friday.Entries.Where(e => e.InvoiceItemId == i.InvoiceItemId)
                        .Sum(e => e.Minutes),
                    minutesSaturday: entries
                        .Saturday.Entries.Where(e => e.InvoiceItemId == i.InvoiceItemId)
                        .Sum(e => e.Minutes),
                    minutesSunday: entries
                        .Sunday.Entries.Where(e => e.InvoiceItemId == i.InvoiceItemId)
                        .Sum(e => e.Minutes)
                ))
                .ToArray()
        );
    }
}
