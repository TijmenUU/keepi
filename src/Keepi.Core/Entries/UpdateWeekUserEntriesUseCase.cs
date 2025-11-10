using System.Diagnostics;
using Keepi.Core.UserEntryCategories;

namespace Keepi.Core.Entries;

public interface IUpdateWeekUserEntriesUseCase
{
    Task<IMaybeErrorResult<UpdateWeekUserEntriesUseCaseError>> Execute(
        int userId,
        int year,
        int weekNumber,
        UpdateWeekUserEntriesUseCaseInput input,
        CancellationToken cancellationToken
    );
}

internal class UpdateWeekUserEntriesUseCase(
    IGetUserUserEntryCategories getUserUserEntryCategories,
    IOverwriteUserEntriesForDates overwriteUserEntriesForDates
) : IUpdateWeekUserEntriesUseCase
{
    public async Task<IMaybeErrorResult<UpdateWeekUserEntriesUseCaseError>> Execute(
        int userId,
        int year,
        int weekNumber,
        UpdateWeekUserEntriesUseCaseInput input,
        CancellationToken cancellationToken
    )
    {
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

        var userEntryCategories = await getUserUserEntryCategories.Execute(
            userId: userId,
            userEntryCategoryIds: days.SelectMany(d => d.Entries.Select(e => e.EntryCategoryId))
                .Distinct()
                .ToArray(),
            cancellationToken: cancellationToken
        );

        List<UserEntryEntity> userEntryEntities = [];
        var dayDates = WeekNumberHelper.WeekNumberToDates(year: year, number: weekNumber);

        Debug.Assert(dayDates.Length == days.Length);

        foreach (var (dayIndex, day) in days.Index())
        {
            var date = dayDates[dayIndex];

            foreach (var entry in day.Entries)
            {
                var userEntrCategory = userEntryCategories.SingleOrDefault(uec =>
                    uec.Id == entry.EntryCategoryId
                );
                if (userEntrCategory == null)
                {
                    return Result.Failure(
                        UpdateWeekUserEntriesUseCaseError.UnknownUserEntryCategory
                    );
                }

                if (!userEntrCategory.IsEntryAllowedForDate(date))
                {
                    return Result.Failure(
                        UpdateWeekUserEntriesUseCaseError.InvalidUserEntryCategory
                    );
                }

                if (!UserEntryEntity.IsValidMinutes(entry.Minutes))
                {
                    return Result.Failure(UpdateWeekUserEntriesUseCaseError.InvalidMinutes);
                }

                if (!UserEntryEntity.IsValidRemark(entry.Remark))
                {
                    return Result.Failure(UpdateWeekUserEntriesUseCaseError.InvalidRemark);
                }

                userEntryEntities.Add(
                    new UserEntryEntity(
                        id: 0,
                        userId: userId,
                        userEntryCategoryId: entry.EntryCategoryId,
                        date: date,
                        minutes: entry.Minutes,
                        remark: entry.Remark
                    )
                );
            }
        }

        var result = await overwriteUserEntriesForDates.Execute(
            userId: userId,
            dates: dayDates,
            userEntries: userEntryEntities.ToArray(),
            cancellationToken: cancellationToken
        );

        if (result.Succeeded)
        {
            return Result.Success<UpdateWeekUserEntriesUseCaseError>();
        }
        return Result.Failure(UpdateWeekUserEntriesUseCaseError.Unknown);
    }
}

public enum UpdateWeekUserEntriesUseCaseError
{
    Unknown,
    UnknownUserEntryCategory,
    InvalidUserEntryCategory,
    InvalidMinutes,
    InvalidRemark,
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
    int EntryCategoryId,
    int Minutes,
    string? Remark
);
