using System.Drawing;
using StringUtils;

namespace ColorUtils;

/// <summary>
/// Class used to manage colors
/// </summary>
/// Based of https://stackoverflow.com/a/76005078
public static class ColorConsole
{
    const int STD_OUTPUT_HANDLE = -11;
    const int ENABLE_VIRTUAL_TERMINAL_PROCESSING = 4;

    private const string RESET = "\x1B[0m";
    private const string UNDERLINE = "\x1B[4m";
    private const string BOLD = "\x1B[1m";
    private const string ITALIC = "\x1B[3m";

    static ColorConsole()
    {
        var handle = GetStdHandle(STD_OUTPUT_HANDLE);
        GetConsoleMode(handle, out int mode);
        mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
        SetConsoleMode(handle, mode);
    }

    /// <returns>Equivalent of <paramref name="color"/> of type <see cref="Color"/></returns>
    public static Color ToColor(this ConsoleColor color) => color == ConsoleColor.DarkYellow ? Color.Gold :Color.FromName(color.ToString());

    public static Color HexToRgb(string hexcolor)
    {
        hexcolor = hexcolor.Remove(0, 1);

        if (hexcolor.Length != 6)
            throw new ArgumentException("Not a valid hex color");

        string[] rgb = hexcolor.Select((obj, index) => new { obj, index })
            .GroupBy(o => o.index / 2)
            .Select(g => new string(g.Select(a => a.obj)
            .ToArray())).ToArray();

        return Color.FromArgb(255, Convert.ToByte(rgb[0], 16), Convert.ToByte(rgb[1], 16), Convert.ToByte(rgb[2], 16));
    }

    /// <returns>Sequence of characters used to make the text of <paramref name="color"/></returns>
    public static string ForeColor(this Color color, string? text = null) => $"\x1B[38;2;{color.R};{color.G};{color.B}m{text}";

    /// <returns>Sequence of characters used to make the background of <paramref name="color"/></returns>
    public static string BackColor(this Color color, string? text = null) => $"\x1B[48;2;{color.R};{color.G};{color.B}m{text}";

    /// <returns>Sequence of characters used to reset the colors of the text</returns>
    public static string ResetColor(this string text) => $"{ResetColor()}{text}";

    public static string ResetColor() => RESET;

    /// <returns><paramref name="text"/> with <paramref name="style"/></returns>
    public static string ApplyStyles(this string text, FontStyle style)
    {
        string copy = text;

        if (style.HasFlag(FontStyle.Bold))
            copy = copy.Bold();

        if (style.HasFlag(FontStyle.Italic))
            copy = copy.Italic();

        if (style.HasFlag(FontStyle.Underline))
            copy = copy.Underline();

        return copy + RESET;
    }

    /// <returns>Sequence of characters used to make <paramref name="text"/> bold</returns>
    private static string Bold(this string text) => $"{BOLD}{text}";

    /// <returns>Sequence of characters used to make <paramref name="text"/> italic</returns>
    private static string Italic(this string text) => $"{ITALIC}{text}";

    /// <returns>Sequence of characters used to make <paramref name="text"/> underlined</returns>
    private static string Underline(this string text) => $"{UNDERLINE}{text}";

    #region Imports
    /// <inheritdoc/>
    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool SetConsoleMode(IntPtr hConsoleHandle, int mode);

    /// <inheritdoc/>
    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool GetConsoleMode(IntPtr handle, out int mode);

    /// <inheritdoc/>
    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr GetStdHandle(int handle);
    #endregion
}