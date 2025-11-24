using System.Diagnostics.CodeAnalysis;

namespace Keepi.Core.InvoiceItems;

public static class InvoiceItemEntity
{
    public const int NameMaxLength = 64;

    public static bool IsValidName([NotNullWhen(returnValue: true)] string? value) =>
        !string.IsNullOrWhiteSpace(value) && value.Length <= NameMaxLength;
}
