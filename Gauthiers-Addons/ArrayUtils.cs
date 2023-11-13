using ColorUtils;
using StringUtils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ArrayUtils
{
    /// <summary> Functions that compare two arrays </summary>
    public static class ArrayComparaison
    {
        // https://stackoverflow.com/a/22173807
        /// <returns> Do the given <see cref="IEnumerable{T}"/> contains the same items? </returns>
        public static bool IsSame<T>(this IEnumerable<T> array1, IEnumerable<T> array2)
        {
            if (array1.Count() != array2.Count())
                return false;

            // Group by items to KeyValuePair(item, # of this items)
#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
            var counts = array1.GroupBy(v => v).ToDictionary(g => g.Key, g => g.Count());
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.

            foreach (var n in array2)
            {
                if (!counts.TryGetValue(n, out var result) || result == 0)
                    return false;

                counts[n] = result - 1;
            }

            // Check if there are items remainings
            return counts.Values.All(c => c == 0);
        }

        /// <returns> Is <paramref name="array1"/> sorted? </returns>
        public static bool IsSorted<T>(this IList<T> array1) where T : IComparable<T>
        {
            for (int i = 1; i < array1.Count; ++i)
            {
                if (array1[i - 1].CompareTo(array1[i]) > 0)
                    return false;
            }
            return true;
        }
    }

    /// <summary> Functions that manipulate an array </summary>
    public static class ArrayManipulation
    {
        #region IEnumerable
        /// <returns> Shuffled version of <paramref name="array"/> </returns>
        public static IOrderedEnumerable<T> Shuffle<T>(this IEnumerable<T> array) => array.OrderBy(x => Guid.NewGuid());

        /// <summary>
        /// Clones <paramref name="array"/> in a new list. Each element is clone based of <paramref name="cloning"/>
        /// </summary>
        public static IList<T> Clone<T>(this IEnumerable<T> array, Func<T, T> cloning)
        {
            var clone = new List<T>();

            array.ForEach(item => clone.Add(cloning.Invoke(item)));

            return clone;
        }

        /// <summary> Clones <paramref name="array"/> in a new list </summary>
        public static IList<T> Clone<T>(this IEnumerable<T> array) where T : ICloneable => array.Clone(x => (T)x.Clone());

        /// <summary>
        /// Replaces all items that respect <paramref name="pred"/> with <paramref name="newValue"/>
        /// </summary>
        public static IList<T> Replace<T>(this IEnumerable<T> array, Predicate<T> pred, T newValue)
        {
            var copy = array.Clone(x => x);

            copy.ForEach((i, item) =>
            {
                if (pred(item))
                    copy[i] = newValue;
            });

            return copy;
        }

        /// <summary> Replaces all items equal to <paramref name="target"/> with <paramref name="newValue"/> </summary>
        public static IEnumerable<T> Replace<T>(this IEnumerable<T> array, T target, T newValue) => array.Replace(x => x?.Equals(target) ?? false, newValue);

        /// <returns> Sum of all the elements in <paramref name="src"/> </returns>
        public static U? Sum<T, U>(this IEnumerable<T> src, Func<U?, T, U?> sumFct, U? init = default)
        {
            U? sum = init;

            src.ForEach(item => sum = sumFct(sum, item));

            return sum;
        }

        /// <returns>
        /// Fused <see cref="IEnumerable{T}"/> containing <paramref name="src"/> and <paramref name="enumerables"/>
        /// </returns>
        public static IEnumerable<T> Combine<T>(this IEnumerable<T> src, params IEnumerable<T>[] enumerables)
        {
            var copy = src.Clone(x => x);

            foreach (var list in enumerables)
            {
                foreach (var item in list)
                {
                    copy.Add(item);
                }
            }
            return copy;
        }

        /// <summary> Performs the specified action on each element of the <see cref="IEnumerable{T}{T}"/>. </summary>
        /// <param name="action"> The action to perform on each element of the <see cref="IEnumerable{T}"/>. </param>
        public static void ForEach<T>(this IEnumerable<T> array, Action<T> action)
        {
            foreach (var item in array)
                action(item);
        }
        #endregion IEnumerable

        #region IList
        /// <summary> Finds all the items that respect <paramref name="pred"/> </summary>
        /// <typeparam name="T"> Test </typeparam>
        /// <param name="array"> </param>
        /// <param name="pred"> </param>
        /// <param name="indexes"> Test </param>
        /// <returns> </returns>
        public static IList<T> FindAllMatchings<T>(this IList<T> array, Predicate<T> pred, out List<int> indexes)
        {
            var items = new List<T>();

            var tempIndexes = new List<int>(); // For the lamdba

            array.ForEach((i, item) =>
            {
                if (!pred(item))
                    return;

                items.Add(item);
                tempIndexes.Add(i);
            });

            indexes = tempIndexes;
            return items;
        }

        /// <returns>
        /// Copy of <paramref name="array"/> where each element when through <paramref name="func"/>
        /// </returns>
        public static IList<T1> Transform<T0, T1>(this IList<T0> array, Func<T0, T1> func)
        {
            var copy = new List<T1>();

            array.ForEach((_, item) => copy.Add(func(item)));

            return copy;
        }

        /// <summary> Performs the specified action on each element of the <see cref="IList{T}"/>. </summary>
        /// <param name="action"> The action to perform on each element of the <see cref="IList{T}"/>. </param>
        public static void ForEach<T>(this IList<T> array, Action<int, T> action, int startIndex = 0)
        {
            for (int i = startIndex; i < array.Count; ++i)
                action?.Invoke(i, array[i]);
        }
        #endregion IList

        public static void ForEach<T>(this T[,] array, Action<int, int, T> action)
        {
            for (int i = 0; i != array.GetLength(0); ++i)
            {
                for (int j = 0; j != array.GetLength(1); ++j)
                {
                    action(i, j, array[i, j]);
                }
            }
        }
    }

    /// <summary> Function that can display an array </summary>
    public static class ArrayDisplay
    {
        /// <param name="array"> </param>
        /// <param name="arrayName">
        /// Name of the array. If not set, it will be the name of the caller
        /// </param>
        /// <param name="itemColor"> Color of the items </param>
        /// <param name="linePerItem"> </param>
        /// <returns> <see cref="string"/> representing <paramref name="array"/> as text </returns>
        public static string ToStr<T>(
            this IList<T> array,
            [System.Runtime.CompilerServices.CallerMemberName] string? arrayName = null,
            Color? itemColor = null,
            bool linePerItem = false)
        {
            string content = string.Empty;

            string colorSequence = (itemColor ?? Color.Blue).ForeColor();

            array.ForEach((i, item) =>
            {
                content += colorSequence;

                if (linePerItem)
                    content += "\n "; // Tabulation

                content += item?.ToString() ?? string.Empty;

                if (i != array.Count - 1)
                {
                    content += ",".ResetColor();

                    if (!linePerItem)
                        content += " ";
                }
                else
                    content += linePerItem ? "\n" : " ";
            });

            return arrayName?.ApplyStyles(FontStyle.Bold) + $" {{ {content}" + "}".ResetColor();
        }
    }
}