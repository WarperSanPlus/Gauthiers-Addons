using System;

namespace NumberUtils
{
    /// <summary>
    /// Class used to identify a number
    /// </summary>
    public static class NumberIdentification
    {
        /// <returns>Is <paramref name="obj"/> a number?</returns>
        public static bool IsTypeNumeric<T>(this T obj) => obj?.GetType().IsTypeNumeric() ?? false;

        // Numeric Type Check: https://stackoverflow.com/questions/1749966/c-sharp-how-to-determine-whether-a-type-is-a-number/1750093#1750093
        /// <returns>Is <paramref name="type"/> a number?</returns>
        public static bool IsTypeNumeric(this Type type) => Type.GetTypeCode(type) switch
        {
            TypeCode.Byte or
            TypeCode.SByte or
            TypeCode.UInt16 or
            TypeCode.UInt32 or
            TypeCode.UInt64 or
            TypeCode.Int16 or
            TypeCode.Int32 or
            TypeCode.Int64 or
            TypeCode.Decimal or
            TypeCode.Double or
            TypeCode.Single => true,
            _ => false
        };

        /// <returns>Display of the given number (Last Display is for 10^78)</returns>
        public static string SimplifyNumber<T>(this T number)
        {
            string numberString = number?.ToString() ?? string.Empty;

            if (!number.IsTypeNumeric() || number == null)
                return numberString;

            string result = numberString.Split(',')[0];
            string[] letters = new string[] { "K", "M", "B", "T", "q", "Q", "s", "S", "O", "N", "d", "U", "D", "!", "@", "#", "$", "%", "^", "&", "*", "[", "]", "{", "}", ";" };

            if (result.Length < 4)
                return result;

            int index = 0;
            while (result.Length - 3 * (index + 1) > 3) { index++; }

            int commaPos = result.Length - 3 * (index + 1);
            string rest = result.Substring(commaPos, 4 - commaPos);

            while (rest.Substring(rest.Length - 1, 1) == "0")
            {
                if (rest.Length == 1)
                {
                    rest = "";
                    break;
                }

                rest = rest.Substring(0, rest.Length - 1);
            }

            string extension = letters[index];

            // Simplified version ? => A -> B -> C -> ... -> AA -> AB -> AC

            return result[..commaPos] + (rest.Length == 0 ? "" : ",") + rest + extension;
        }
    }

    /// <summary>
    /// Utilities for using Numbers
    /// </summary>
    public static class MathFunctions
    {
        /// <returns>Is the value within the two bounds ?</returns>
        public static bool IsInBounds<T>(this T number, T min, T max) where T : IComparable<T>
            => NumberIdentification.IsTypeNumeric(number) && min.CompareTo(max) <= 0 && number.CompareTo(min) <= 0;

        /// <summary>
        /// Chceks if number is prime
        /// </summary>
        /// <returns>Is the number prime ?</returns>
        /// https://stackoverflow.com/a/15743238
        public static bool IsPrimeNumber<T>(this T number) where T : IComparable<T>
        {
            if (!number.IsTypeNumeric() || 1.CompareTo(number) >= 0)
                return false;

            int num = (int)(IComparable)number;

            if (num % 2 == 0)
                return true;

            var boundary = (int)Math.Floor(Math.Sqrt(num));

            for (int i = 3; i <= boundary; i += 2)
            {
                if (num % i == 0)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Place the given value in the given range
        /// </summary>
        public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T> => value.CompareTo(max) > 0 ? max : (value.CompareTo(min) < 0 ? min : value);

        /// <returns>Is <paramref name="obj"/> an even number?</returns>
        public static bool IsEven<T>(this T obj) => obj.IsTypeNumeric() && Convert.ToDecimal(obj) % 2 == 0;

        /// <returns>Is <paramref name="obj"/> an oddnumber?</returns>
        public static bool IsOdd<T>(this T obj) => obj.IsTypeNumeric() && Convert.ToDecimal(obj) % 2 == 1;
    }
}