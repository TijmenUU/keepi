using System.Diagnostics.CodeAnalysis;

namespace Keepi.Core.Entries;

public static class UserEntryEntity
{
    public const int RemarkMaxLength = 256;

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
        if (string.IsNullOrEmpty(remark) || remark.Length <= RemarkMaxLength)
        {
            return true;
        }

        return false;
    }
}
