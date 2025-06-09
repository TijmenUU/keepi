using System.Diagnostics.CodeAnalysis;

namespace Keepi.Core.UserEntryCategories;

public sealed class UserEntryCategoryEntity
{
    public UserEntryCategoryEntity(
        int id,
        string name,
        int ordinal,
        bool enabled,
        DateOnly? activeFrom,
        DateOnly? activeTo
    )
    {
        Id = id;
        Name = name;
        Ordinal = ordinal;
        Enabled = enabled;
        ActiveFrom = activeFrom;
        ActiveTo = activeTo;
    }

    public int Id { get; }
    public string Name { get; set; }
    public int Ordinal { get; set; }
    public bool Enabled { get; set; }
    public DateOnly? ActiveFrom { get; set; }
    public DateOnly? ActiveTo { get; set; }

    public static bool IsValidName([NotNullWhen(returnValue: true)] string? name)
    {
        return !string.IsNullOrWhiteSpace(name) && name.Length <= 64;
    }

    public static bool IsValidOrdinal([NotNullWhen(returnValue: true)] int? ordinal)
    {
        return ordinal.HasValue && ordinal.Value >= 0;
    }

    public static bool IsValidActiveDateRange(DateOnly? from, DateOnly? to)
    {
        return from == null || to == null || from < to;
    }

    public bool IsEntryAllowedForDate(DateOnly entryDate)
    {
        if (!Enabled)
        {
            return false;
        }

        if (ActiveTo.HasValue && ActiveTo < entryDate)
        {
            return false;
        }

        if (ActiveFrom.HasValue && ActiveFrom > entryDate)
        {
            return false;
        }

        return true;
    }
}
