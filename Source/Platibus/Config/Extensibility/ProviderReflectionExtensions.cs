using System;
using System.Collections.Generic;
using System.Linq;
using Platibus.Utils;

namespace Platibus.Config.Extensibility
{
    internal static class ProviderReflectionExtensions
    {
        public static IEnumerable<Type> WithProviderName(this IEnumerable<Type> types, string providerName)
        {
            return string.IsNullOrWhiteSpace(providerName)
                ? Enumerable.Empty<Type>()
                : types.With<ProviderAttribute>(a => providerName.Equals(a.Name, StringComparison.OrdinalIgnoreCase));
        }

        public static IEnumerable<IGrouping<int, Type>> GroupByPriorityDescending(this IEnumerable<Type> types)
        {
            return types
                .GroupBy<ProviderAttribute, int>(a => a.Priority)
                .OrderByDescending(g => g.Key);
        }
    }
}
