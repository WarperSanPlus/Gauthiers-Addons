namespace StringUtils
{
    /// <summary>
    /// Class used for manipulation of <see cref="string"/>
    /// </summary>
    public static class StringManipulation
    {
        /// <returns>Reversed string</returns>
        public static string GetReverse(this string message) => new(message.Reverse().ToArray());

        /// <returns>Formatted lines</returns>
        public static string[] ConvertToWarpText(this string message, int width, int height)
        {
            var chars = message.ToCharArray();

            var lines = new List<string>();
            string line = "";

            foreach (var item in chars)
            {
                if (item == '\n' || line.Length + 1 >= width)
                {
                    if (lines.Count > height)
                        break;

                    lines.Add(line);
                    line = "";
                    continue;
                }

                line += item;
            }

            if (!string.IsNullOrEmpty(line))
                lines.Add(line);
            return lines.ToArray();
        }

        /// <summary>
        /// Aligns the given value in the given alignment
        /// </summary>
        public static string FormatStringToAlign(this string value, int maxLength, StringAlignment align) => align switch
        {
            StringAlignment.Center => string.Format($"{{0,-{maxLength}}}", string.Format("{0," + ((maxLength + value.Length) / 2).ToString() + "}", value)),
            StringAlignment.Far => string.Format($"{{0, {maxLength}}}", value),
            _ => string.Format($"{{0, -{maxLength}}}", value),
        };
    }

    /// <summary>
    /// Class used to identify <see cref="string"/>
    /// </summary>
    public static class StringIdentification
    {
        /// <returns>Checks if the given text is a palindrome</returns>
        public static bool IsPalindrome(this string text)
        {
            if (text.Length <= 1)
                return false;

            char[] characters = text.ToCharArray();

            for (int i = 0; i < characters.Length / 2; ++i)
            {
                if (characters[i] != characters[characters.Length - 1 - i])
                    return false;
            }
            return true;
        }

#nullable enable
        /// <summary>
        /// Finds the similitaries and differences between <paramref name="a"/> and <paramref name="b"/>
        /// </summary>
        public static void CompareString(
            this string a, string b,
            Action<char, char?>? OnDifferenceFound = null,
            Action<char>? OnSimilitaryFound = null)
        {
            string longestString = a.Length > b.Length ? a : b;

            int shortestLength = Math.Min(a.Length, b.Length);

            for (int i = 0; i != shortestLength; ++i)
            {
                char cA = a[i];
                char cB = b[i];

                if (cA == cB)
                    OnSimilitaryFound?.Invoke(cA);
                else
                    OnDifferenceFound?.Invoke(cA, cB);
            }

            for (int i = shortestLength; i != longestString.Length; ++i)
            {
                OnDifferenceFound?.Invoke(longestString[i], null);
            }
        }
#nullable restore
    }

    public enum StringAlignment
    {
        //
        // Summary:
        //     Specifies the text be aligned near the layout. In a left-to-right layout, the
        //     near position is left. In a right-to-left layout, the near position is right.
        Near,
        //
        // Summary:
        //     Specifies that text is aligned in the center of the layout rectangle.
        Center,
        //
        // Summary:
        //     Specifies that text is aligned far from the origin position of the layout rectangle.
        //     In a left-to-right layout, the far position is right. In a right-to-left layout,
        //     the far position is left.
        Far
    }

    [Flags]
    public enum FontStyle
    {
        Regular = 0x0,
        Bold = 0x1,
        Italic = 0x2,
        Underline = 0x4,
        Strikeout = 0x8
    }
}
