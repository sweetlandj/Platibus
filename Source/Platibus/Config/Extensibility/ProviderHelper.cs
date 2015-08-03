using System;
using System.Collections.Generic;
using System.Linq;
using Common.Logging;

namespace Platibus.Config.Extensibility
{
    /// <summary>
    /// Helper class used to load providers by name,
    /// </summary>
    public static class ProviderHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.Config);

        /// <summary>
        /// Finds the most appropriate type or subtype of 
        /// <typeparamref name="TProvider"/> whose type name is <see cref="providerName"/>
        /// or which has been decorated with a <see cref="ProviderAttribute"/> with the
        /// specified <paramref name="providerName"/>.
        /// </summary>
        /// <typeparam name="TProvider">The provider type or at type from which the provider 
        /// must inherit</typeparam>
        /// <param name="providerName">The provider name associated with the type</param>
        /// <returns>Returns a new provider instance with the specified 
        /// <paramref name="providerName"/> and type.</returns>
        /// <exception cref="NullReferenceException">Thrown if <paramref name="providerName"/>
        /// is <c>null</c> or whitespace.</exception>
        /// <exception cref="ProviderNotFoundException">Thrown if no suitable providers are
        /// found in the application domain.</exception>
        /// <exception cref="MultipleProvidersFoundException">Thrown if there are multiple
        /// providers found with the same priority.</exception>
        /// <seealso cref="ProviderAttribute"/>
        public static TProvider GetProvider<TProvider>(string providerName)
        {
            var providerType = Type.GetType(providerName);
            if (providerType == null)
            {
                Log.DebugFormat("Looking for provider \"{0}\"...", providerName);
                var prioritizedProviders = ReflectionHelper
                    .FindConcreteSubtypes<TProvider>()
                    .WithProviderName(providerName)
                    .GroupByPriorityDescending()
                    .ToList();

                var highestPriority = prioritizedProviders.FirstOrDefault();
                if (highestPriority == null) throw new ProviderNotFoundException(providerName);

                var providers = highestPriority.ToList();
                if (providers.Count > 1) throw new MultipleProvidersFoundException(providerName, providers);
                
                providerType = providers.First();
            }

            Log.DebugFormat("Found provider type \"{0}\"", providerType.FullName);
            return (TProvider)Activator.CreateInstance(providerType);
        }

        private static IEnumerable<Type> WithProviderName(this IEnumerable<Type> types, string providerName)
        {
            return string.IsNullOrWhiteSpace(providerName) 
                ? Enumerable.Empty<Type>() 
                : types.With<ProviderAttribute>(a => providerName.Equals(a.Name, StringComparison.OrdinalIgnoreCase));
        }

        private static IEnumerable<IGrouping<int, Type>> GroupByPriorityDescending(this IEnumerable<Type> types)
        {
            return types
                .GroupBy<ProviderAttribute, int>(a => a.Priority)
                .OrderByDescending(g => g.Key);
        }
    }
}