using Platibus.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Platibus.IIS
{
    /// <summary>
    /// Configuration cache
    /// </summary>
    /// <remarks>
    /// An HTTP handler is created for each request and possibly pooled depending on the value
    /// returned by <see cref="System.Web.IHttpHandler.IsReusable"/>.  Because of this it is 
    /// possible for several HTTP handlers to exist at the same time.  In order to avoid 
    /// re-initializing the HTTP handler for each new request, the configuration tasks will
    /// be cached by section name.  This also guards against redundant initialization that
    /// may occur as the result of multiple HTTP modules being initialized concurrently
    /// during application startup.
    /// </remarks>
    internal class IISConfigurationCache
    {
        public static readonly IISConfigurationCache SingletonInstance = new IISConfigurationCache();

        private readonly object _syncRoot = new object();
        private readonly IDictionary<string, IISConfiguration> _cachedConfigurations = new Dictionary<string, IISConfiguration>();

        public IISConfiguration GetConfiguration(string sectionName)
        {
            var cacheKey = (sectionName ?? "").Trim();
            return GetOrAdd(cacheKey, () => LoadConfiguration(sectionName, null));
        }

        public IISConfiguration GetConfiguration(string sectionName, Action<IISConfiguration> configure)
        {
            var cacheKey = (sectionName ?? "").Trim();
            return GetOrAdd(cacheKey, () => LoadConfiguration(sectionName, configure));
        }

        public IISConfiguration GetConfiguration(string sectionName, Func<IISConfiguration, Task> configure)
        {
            var cacheKey = (sectionName ?? "").Trim();
            return GetOrAdd(cacheKey, () => LoadConfiguration(sectionName, configure));
        }

        public IISConfiguration GetOrAdd(string cacheKey, Func<IISConfiguration> get)
        {
            IISConfiguration configuration;
            lock (_syncRoot)
            {

                if (_cachedConfigurations.TryGetValue(cacheKey, out configuration))
                {
                    return configuration;
                }

                configuration = get();
                _cachedConfigurations[cacheKey] = configuration;
            }

            return configuration;
        }

        private static IISConfiguration LoadConfiguration(string sectionName, Action<IISConfiguration> configure)
        {
            var configuration = LoadConfigurationAsync(sectionName, null).GetResultUsingContinuation();
            configure?.Invoke(configuration);
            return configuration;
        }

        private static IISConfiguration LoadConfiguration(string sectionName, Func<IISConfiguration, Task> configure)
        {
            return LoadConfigurationAsync(sectionName, configure).GetResultUsingContinuation();
        }

        private static async Task<IISConfiguration> LoadConfigurationAsync(string sectionName, Func<IISConfiguration, Task> configure)
        {
            var configuration = new IISConfiguration();
            var configManager = new IISConfigurationManager();
            await configManager.Initialize(configuration, sectionName);
            await configManager.FindAndProcessConfigurationHooks(configuration);
            if (configure != null)
            {
                await configure(configuration);
            }
            return configuration;
        }
    }
}
