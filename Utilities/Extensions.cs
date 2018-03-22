using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Utilities
{
    public static class Extensions
    {
        public static string PureName(this FileInfo file)
        {
            var name = file.Name;
            return name.Remove(name.Length - file.Extension.Length);
        }

        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
            {
                action(item);
            }
        }

        public static T[] PushToEnd<T>(this T[] array, T value)
        {
            var arr = new T[array.Length + 1];

            for (int i = 0; i < arr.Length - 1; i++)
            {
                arr[i] = array[i];
            }
            arr[array.Length] = value;

            return arr;
        }

        public static T LastItem<T>(this T[] array)
        {
            return array[array.Length - 1];
        }

        /*public static T Max<T>(this IEnumerable<T> collection, Func<T, float> evaluator)
        {
            float max = float.NegativeInfinity;
            T current = default(T);
            foreach (var item in collection)
            {
                var val = evaluator(item);
                if (val > max)
                {
                    max = val;
                    current = item;
                }
            }
            return current;
        }*/
    }
}
