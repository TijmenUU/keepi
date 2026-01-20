using System.Diagnostics;
using Keepi.Core.UserProjects;
using Keepi.Core.Users;
using Microsoft.Extensions.Logging;

namespace Keepi.Core.Entries;

public interface IUpdateWeekUserEntriesUseCase
{
    Task<IMaybeErrorResult<UpdateWeekUserEntriesUseCaseError>> Execute(
        int year,
        int weekNumber,
        UpdateWeekUserEntriesUseCaseInput input,
        CancellationToken cancellationToken
    );
}

public enum UpdateWeekUserEntriesUseCaseError
{
    Unknown,
    UnauthenticatedUser,
    UnknownUserInvoiceItem,
    InvalidUserInvoiceItem,
    InvalidMinutes,
    InvalidRemark,
}

internal sealed class UpdateWeekUserEntriesUseCase(
    IResolveUser resolveUser,
    IGetUserProjects getUserProjects,
    IDeleteUserEntriesForDateRange deleteUserEntriesForDateRange,
    ISaveUserEntries saveUserEntries,
    ILogger<UpdateWeekUserEntriesUseCase> logger
) : IUpdateWeekUserEntriesUseCase
{
    public async Task<IMaybeErrorResult<UpdateWeekUserEntriesUseCaseError>> Execute(
        int year,
        int weekNumber,
        UpdateWeekUserEntriesUseCaseInput input,
        CancellationToken cancellationToken
    )
    {
        var userResult = await resolveUser.Execute(cancellationToken: cancellationToken);
        if (!userResult.TrySuccess(out var userSuccessResult, out var userErrorResult))
        {
            return userErrorResult switch
            {
                ResolveUserError.UserNotAuthenticated => Result.Failure(
                    UpdateWeekUserEntriesUseCaseError.UnauthenticatedUser
                ),
                _ => Result.Failure(UpdateWeekUserEntriesUseCaseError.Unknown),
            };
        }

        var days = new[]
        {
            input.Monday,
            input.Tuesday,
            input.Wednesday,
            input.Thursday,
            input.Friday,
            input.Saturday,
            input.Sunday,
        };

        var getUserProjectsResult = await getUserProjects.Execute(
            userId: userSuccessResult.Id,
            cancellationToken: cancellationToken
        );
        if (!getUserProjectsResult.TrySuccess(out var userProjects, out var errorResult))
        {
            logger.LogError(
                "Unexpected error {Error} whilst fetching user projects for update week user entries use case",
                errorResult
            );
            return Result.Failure(UpdateWeekUserEntriesUseCaseError.Unknown);
        }

        List<SaveUserEntriesInputEntry> entries = [];
        var dayDates = WeekNumberHelper.WeekNumberToDates(year: year, number: weekNumber);

        Debug.Assert(dayDates.Length == days.Length);

        foreach (var (dayIndex, day) in days.Index())
        {
            var date = dayDates[dayIndex];

            foreach (var entry in day.Entries)
            {
                var userProject = userProjects.Projects.SingleOrDefault(p =>
                    p.InvoiceItems.Any(i => i.Id == entry.InvoiceItemId)
                );
                if (userProject == null)
                {
                    return Result.Failure(UpdateWeekUserEntriesUseCaseError.UnknownUserInvoiceItem);
                }

                if (!userProject.Enabled)
                {
                    return Result.Failure(UpdateWeekUserEntriesUseCaseError.InvalidUserInvoiceItem);
                }

                if (!UserEntryEntity.IsValidMinutes(entry.Minutes))
                {
                    return Result.Failure(UpdateWeekUserEntriesUseCaseError.InvalidMinutes);
                }

                if (!UserEntryEntity.IsValidRemark(entry.Remark))
                {
                    return Result.Failure(UpdateWeekUserEntriesUseCaseError.InvalidRemark);
                }

                entries.Add(
                    new(
                        InvoiceItemId: entry.InvoiceItemId,
                        Date: date,
                        Minutes: entry.Minutes,
                        Remark: entry.Remark
                    )
                );
            }
        }

        var deletionResult = await deleteUserEntriesForDateRange.Execute(
            input: new(
                UserId: userSuccessResult.Id,
                From: dayDates.First(),
                ToInclusive: dayDates.Last(),
                ProjectIds: userProjects.Projects.Where(p => p.Enabled).Select(p => p.Id).ToArray()
            ),
            cancellationToken: cancellationToken
        );

        if (!deletionResult.TrySuccess(out var deletionError))
        {
            logger.LogError(
                "Unexpected error {Error} whilst deleting existing user entries for week {WeekNumber} {Year} for user {UserId}",
                errorResult,
                weekNumber,
                year,
                userSuccessResult.Id
            );
            return Result.Failure(UpdateWeekUserEntriesUseCaseError.Unknown);
        }

        var saveResult = await saveUserEntries.Execute(
            input: new(UserId: userSuccessResult.Id, Entries: [.. entries]),
            cancellationToken: cancellationToken
        );
        if (saveResult.Succeeded)
        {
            return Result.Success<UpdateWeekUserEntriesUseCaseError>();
        }

        logger.LogError(
            "Unexpected error {Error} whilst saving user entries for week {WeekNumber} {Year} for user {UserId}",
            errorResult,
            weekNumber,
            year,
            userSuccessResult.Id
        );
        return Result.Failure(UpdateWeekUserEntriesUseCaseError.Unknown);
    }
}

public record UpdateWeekUserEntriesUseCaseInput(
    UpdateWeekUserEntriesUseCaseInputDay Monday,
    UpdateWeekUserEntriesUseCaseInputDay Tuesday,
    UpdateWeekUserEntriesUseCaseInputDay Wednesday,
    UpdateWeekUserEntriesUseCaseInputDay Thursday,
    UpdateWeekUserEntriesUseCaseInputDay Friday,
    UpdateWeekUserEntriesUseCaseInputDay Saturday,
    UpdateWeekUserEntriesUseCaseInputDay Sunday
);

public record UpdateWeekUserEntriesUseCaseInputDay(
    UpdateWeekUserEntriesUseCaseInputDayEntry[] Entries
);

public record UpdateWeekUserEntriesUseCaseInputDayEntry(
    int InvoiceItemId,
    int Minutes,
    string? Remark
);
