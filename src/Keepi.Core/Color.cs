using System.Text.RegularExpressions;

namespace Keepi.Core;

public sealed record Color(byte Red, byte Green, byte Blue)
{
    public static Color FromUint32(uint value)
    {
        if (value > 0xFFFFFF)
        {
            throw new Exception(
                "The unsigned integer color value contains unexpected data in the 32-24 bit range"
            );
        }

        return new Color(
            Red: (byte)((value >> 16) & 0xFF),
            Green: (byte)((value >> 8) & 0xFF),
            Blue: (byte)(value & 0xFF)
        );
    }

    public static uint ToUint32(Color value)
    {
        return ((uint)value.Red << 16) | ((uint)value.Green << 8) | value.Blue;
    }

    // The SYSLIB1045 solution requires the class to be declared as partial in
    // order to generate the source for it. Seems overkill for a few nanoseconds
    // of performance gains to trade readability for that.
    private static readonly Regex HexColorStringRegex = new(
        pattern: "^#[0-9A-Fa-f]{6}$",
        options: RegexOptions.Compiled
    );

    public static bool TryParseHexString(string input, out Color color)
    {
        if (!HexColorStringRegex.IsMatch(input: input))
        {
            color = new Color(Red: 0, Blue: 0, Green: 0);
            return false;
        }

        color = new Color(
            Red: byte.Parse(
                s: input.Substring(1, 2),
                style: System.Globalization.NumberStyles.HexNumber
            ),
            Green: byte.Parse(
                s: input.Substring(3, 2),
                style: System.Globalization.NumberStyles.HexNumber
            ),
            Blue: byte.Parse(
                s: input.Substring(5, 2),
                style: System.Globalization.NumberStyles.HexNumber
            )
        );
        return true;
    }

    public string ToHexColorString() => $"#{ToUint32(this):x6}";
}
