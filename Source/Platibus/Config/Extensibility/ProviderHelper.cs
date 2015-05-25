using System;
using System.Linq;
using Common.Logging;

namespace Platibus.Config.Extensibility
{
    public static class ProviderHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.Config);

        public static TProvider GetProvider<TProvider>(string providerName)
        {
            var providerType = Type.GetType(providerName);
            if (providerType == null)
            {
                Log.DebugFormat("Looking for provider \"{0}\"...", providerName);
                var providers = ReflectionHelper
                    .FindConcreteSubtypes<TProvider>()
                    .With<ProviderAttribute>(
                        a => string.Equals(providerName, a.Name, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending<ProviderAttribute>(a => a.Priority)
                    .ToList();

                if (!providers.Any()) throw new ProviderNotFoundException(providerName);

                providerType = providers.First();
            }

            Log.DebugFormat("Found provider type \"{0}\"", providerType.FullName);
            return (TProvider) Activator.CreateInstance(providerType);
        }
    }
}