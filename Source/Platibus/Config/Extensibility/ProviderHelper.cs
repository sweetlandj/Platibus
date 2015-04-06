using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    .With<ProviderAttribute>(a => string.Equals(providerName, a.Name, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (!providers.Any()) throw new ProviderNotFoundException(providerName);
                if (providers.Count > 1) throw new MultipleProvidersFoundException(providerName, providers);

                providerType = providers.First();
            }

            Log.DebugFormat("Found provider type \"{0}\"", providerType.FullName);
            return (TProvider)Activator.CreateInstance(providerType);
        }
    }
}
