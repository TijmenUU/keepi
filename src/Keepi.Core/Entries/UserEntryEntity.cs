using System.Diagnostics.CodeAnalysis;

namespace Keepi.Core.Entries;

public sealed class UserEntryEntity
{
    public UserEntryEntity(
        int id,
        int userId,
        int userEntryCategoryId,
        DateOnly date,
        int minutes,
        string? remark
    )
    {
        Id = id;
        UserId = userId;
        UserEntryCategoryId = userEntryCategoryId;
        Date = date;
        Minutes = minutes;
        Remark = remark;
    }

    public int Id { get; }
    public int UserId { get; }
    public int UserEntryCategoryId { get; }
    public DateOnly Date { get; set; }
    public int Minutes { get; set; }
    public string? Remark { get; set; }

    public static bool IsValidMinutes([NotNullWhen(returnValue: true)] int? minutes)
    {
        if (minutes == null)
        {
            return false;
        }

        if (minutes < 1)
        {
            return false;
        }

        return true;
    }

    public static bool IsValidRemark(string? remark)
    {
        if (string.IsNullOrEmpty(remark) || remark.Length <= 256)
        {
            return true;
        }

        return false;
    }
}
