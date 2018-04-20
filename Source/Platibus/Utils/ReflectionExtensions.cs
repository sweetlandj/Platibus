using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Platibus.Utils
{
    internal static class ReflectionExtensions
    {
        public static bool Has<TAttribute>(this Type type) where TAttribute : Attribute
        {
            return type.GetCustomAttributes(typeof(TAttribute), false).Any();
        }

        public static bool Has<TAttribute>(this Type type, Func<TAttribute, bool> where) where TAttribute : Attribute
        {
            return type.GetCustomAttributes(typeof(TAttribute), false)
                .OfType<TAttribute>()
                .Any(where);
        }

        public static IEnumerable<Type> With<TAttribute>(this IEnumerable<Type> source) where TAttribute : Attribute
        {
            return source.Where(t => t.Has<TAttribute>());
        }

        public static IEnumerable<Type> With<TAttribute>(this IEnumerable<Type> source, Func<TAttribute, bool> where)
            where TAttribute : Attribute
        {
            return source.Where(t => t.Has(where));
        }

        public static IEnumerable<Type> OrderBy<TAttribute>(this IEnumerable<Type> source,
            Func<TAttribute, object> attributeMember) where TAttribute : Attribute
        {
            return source.Select(t => new
            {
                Type = t,
                Attribute = t.GetCustomAttribute<TAttribute>()
            })
                .Where(x => x.Attribute != null)
                .OrderBy(x => attributeMember(x.Attribute))
                .Select(x => x.Type);
        }

        public static IEnumerable<Type> OrderByDescending<TAttribute>(this IEnumerable<Type> source,
            Func<TAttribute, object> attributeMember) where TAttribute : Attribute
        {
            return source.Select(t => new
            {
                Type = t,
                Attribute = t.GetCustomAttribute<TAttribute>()
            })
                .Where(x => x.Attribute != null)
                .OrderByDescending(x => attributeMember(x.Attribute))
                .Select(x => x.Type);
        }

        public static IEnumerable<IGrouping<TKey, Type>> GroupBy<TAttribute, TKey>(this IEnumerable<Type> source,
            Func<TAttribute, TKey> attributeMember) where TAttribute : Attribute
        {
            return source.Select(t => new
            {
                Type = t,
                Attribute = t.GetCustomAttribute<TAttribute>()
            })
                .Where(x => x.Attribute != null)
                .GroupBy(x => attributeMember(x.Attribute), x => x.Type);
        }
    }
}
