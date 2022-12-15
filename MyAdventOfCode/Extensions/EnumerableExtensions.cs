using System;
using System.Collections.Generic;
using System.Linq;

namespace MyAdventOfCode.Extensions
{

    public static class EnumerableExtensions
    {
        public static IEnumerable<T> IntersectAll<T>(this IEnumerable<IEnumerable<T>> collections)
        {
            var set = new HashSet<T>(collections.First());
            foreach (var other in collections.Skip(1))
            {
                set.IntersectWith(other);
            }
            return set;
        }

        public static TAccumulate AggregateUntil<TSource, TAccumulate>(
            this IEnumerable<TSource> source,
            TAccumulate seed,
            Func<TAccumulate, TSource, TAccumulate> func,
            Func<TAccumulate, bool> predicate)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            var accumulate = seed;
            foreach (var item in source)
            {
                accumulate = func(accumulate, item);
                if (predicate(accumulate)) break;
            }
            return accumulate;
        }


        /// <summary>
        /// Lifted straight from <see href="https://stackoverflow.com/a/58826787/6236042"/>
        /// </summary>
        public static IEnumerable<T[]> GetPermutations<T>(this IEnumerable<T> source)
        {
            var sourceArray = source.ToArray();
            var results = new List<T[]>();
            Permute(sourceArray, 0, sourceArray.Length - 1, results);
            return results;
        }

        private static void Swap<T>(ref T a, ref T b)
        {
            T tmp = a;
            a = b;
            b = tmp;
        }

        private static void Permute<T>(T[] elements, int recursionDepth, int maxDepth, ICollection<T[]> results)
        {
            if (recursionDepth == maxDepth)
            {
                results.Add(elements.ToArray());
                return;
            }

            for (var i = recursionDepth; i <= maxDepth; i++)
            {
                Swap(ref elements[recursionDepth], ref elements[i]);
                Permute(elements, recursionDepth + 1, maxDepth, results);
                Swap(ref elements[recursionDepth], ref elements[i]);
            }
        }
    }
}