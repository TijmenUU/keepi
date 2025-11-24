using System.Diagnostics.CodeAnalysis;

namespace Keepi.Core.Projects;

public static class ProjectEntity
{
    public const int NameMaxLength = 64;

    public static bool IsValidName([NotNullWhen(returnValue: true)] string? value) =>
        !string.IsNullOrWhiteSpace(value) && value.Length <= NameMaxLength;

    public static bool HasUniqueUserIds(int[] input)
    {
        return input.Distinct().Count() == input.Length;
    }

    public static bool HasUniqueInvoiceItemNames(string[] itemNames)
    {
        return itemNames.Length == itemNames.Distinct().Count();
    }
}
