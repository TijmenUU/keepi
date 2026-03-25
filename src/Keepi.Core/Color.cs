using System.Text.RegularExpressions;
using Vogen;

namespace Keepi.Core;

[ValueObject<uint>]
public readonly partial struct Color
{
    [GeneratedRegex("^#[0-9A-Fa-f]{6}$", RegexOptions.IgnoreCase)]
    private static partial Regex HexColorStringRegex();

    public static Validation Validate(uint value)
    {
        if (value > 0xFFFFFF)
        {
            return Validation.Invalid(
                "The unsigned integer color value contains unexpected data in the 32-24 bit range"
            );
        }

        return Validation.Ok;
    }

    public static Color FromBytes(byte red, byte green, byte blue)
    {
        return From(value: ((uint)red << 16) + ((uint)green << 8) + (uint)blue);
    }

    public static bool TryParseHexColorString(string input, out Color color)
    {
        if (!HexColorStringRegex().IsMatch(input: input))
        {
            color = From(value: 0);
            return false;
        }

        color = FromBytes(
            red: byte.Parse(
                s: input.Substring(1, 2),
                style: System.Globalization.NumberStyles.HexNumber
            ),
            green: byte.Parse(
                s: input.Substring(3, 2),
                style: System.Globalization.NumberStyles.HexNumber
            ),
            blue: byte.Parse(
                s: input.Substring(5, 2),
                style: System.Globalization.NumberStyles.HexNumber
            )
        );
        return true;
    }

    public string ToHexColorString() => $"#{Value:x6}";
}
