using System;
using System.Collections.Generic;

namespace AggieMove.Helpers
{
    public static class Extensions
    {
        public static void AddRange<T>(this ICollection<T> col, IEnumerable<T> range)
        {
            foreach (T item in range)
                col.Add(item);
        }

        public static void RemoveAll<T>(this IList<T> col, Predicate<T> predicate)
        {
            int i = 0;
            while (i < col.Count)
            {
                var item = col[i];
                if (predicate(item))
                    col.RemoveAt(i);
                else
                    i++;
            }
        }

        public static IEnumerable<(double x_prime, double y_prime)> BackwardFiniteDifference(this IEnumerable<(double x, double y)> src)
        {
            var en = src.GetEnumerator();
            en.MoveNext();
            (double x_prev, double y_prev) = en.Current;

            while (en.MoveNext())
            {
                (double x_cur, double y_cur) = en.Current;

                var dx = x_cur - x_prev;
                var dy = y_cur - y_prev;
                var x_i_prime = (x_cur + x_prev) / 2;

                x_prev = x_cur;
                y_prev = y_cur;

                yield return (x_i_prime, dy / dx);
            }
        }

        public static IEnumerable<(long x_prime, double y_prime)> BackwardFiniteDifference(this IEnumerable<(long x, double y)> src)
        {
            return src.BackwardFiniteDifference((left, right) => left - right, (dy, dx) => dy / dx);
        }

        public static IEnumerable<(long x_prime, TYp y_prime)> BackwardFiniteDifference<TY, TYp>(this IEnumerable<(long x, TY y)> src,
            Func<TY, TY, TYp> differenceFunction, Func<TYp, double, TYp> divisionFunction)
        {
            var en = src.GetEnumerator();
            en.MoveNext();
            (long x_prev, TY y_prev) = en.Current;

            while (en.MoveNext())
            {
                (long x_cur, TY y_cur) = en.Current;

                var dx = x_cur - x_prev;
                var dy = differenceFunction(y_cur, y_prev);
                var x_i_prime = (x_cur + x_prev) / 2;

                x_prev = x_cur;
                y_prev = y_cur;

                yield return (x_i_prime, divisionFunction(dy, dx));
            }
        }
    }
}
