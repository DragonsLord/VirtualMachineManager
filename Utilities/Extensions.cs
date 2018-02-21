using System;
using System.Collections.Generic;
using System.IO;
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
    }
}
