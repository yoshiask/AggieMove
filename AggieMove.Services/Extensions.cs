using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AggieMove.Helpers
{
    public static class Extensions
    {
        public static void AddRange<T>(this ObservableCollection<T> col, IEnumerable<T> range)
        {
            foreach (T item in range)
                col.Add(item);
        }
    }
}
