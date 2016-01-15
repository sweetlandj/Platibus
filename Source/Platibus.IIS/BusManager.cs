using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Platibus.IIS
{
    /// <summary>
    /// Initializes an IIS-hosted bus instance
    /// </summary>
    public class BusManager : IBusManager, IDisposable
    {
        internal static readonly BusManager SingletonInstance = new BusManager();

        /// <summary>
        /// Returns the singleton <see cref="IBusManager"/> instance
        /// </summary>
        /// <returns>Returns the singleton <see cref="IBusManager"/> instance</returns>
        public static IBusManager GetInstance()
        {
            return SingletonInstance;
        }

        private readonly object _syncRoot = new object();
        private readonly IDictionary<Uri, ManagedBus> _busInstances = new Dictionary<Uri, ManagedBus>(); 
        private bool _disposed;

        /// <summary>
        /// Provides access to the IIS-hosted bus with the default configuration
        /// </summary>
        /// <returns>Returns a task whose result is the bus instance</returns>
        public async Task<IBus> GetBus()
        {
            var configuration = await IISConfigurationManager.LoadConfiguration();
            return await GetBus(configuration);
        }

        /// <summary>
        /// Provides access to the IIS-hosted bus using the configuration loaded
        /// from the configuration section with the specified <paramref name="sectionName"/>
        /// </summary>
        /// <param name="sectionName">The name of the configuration section</param>
        /// <returns>Returns a task whose result is the bus instance</returns>
        public async Task<IBus> GetBus(string sectionName)
        {
            var configuration = await IISConfigurationManager.LoadConfiguration(sectionName);
            return await GetBus(configuration);
        }

        /// <summary>
        /// Provides access to the IIS-hosted bus with the specified 
        /// <paramref name="configuration"/>.
        /// </summary>
        /// <param name="configuration">The bus</param>
        /// <returns>Returns a task whose result is the bus instance</returns>
        public async Task<IBus> GetBus(IIISConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            var managedBus = await GetManagedBus(configuration);
            return await managedBus.GetBus();
        }

        internal async Task<ManagedBus> GetManagedBus(IIISConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            ManagedBus bus;
            lock (_syncRoot)
            {
                if (_busInstances.TryGetValue(configuration.BaseUri, out bus))
                {
                    return bus;
                }
                bus = new ManagedBus(configuration);
                _busInstances[configuration.BaseUri] = bus;
            }
            return await Task.FromResult(bus);
        }

        /// <summary>
        /// Allows an object to try to free resources and perform other cleanup operations before it is reclaimed by garbage collection.
        /// </summary>
        ~BusManager()
        {
            if (_disposed) return;
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Called by <see cref="Dispose()"/> or the finalizer to performs application-defined tasks associated with 
        /// freeing, releasing, or resetting unmanaged resources. 
        /// </summary>
        /// <param name="disposing">Whether this method was called from <see cref="Dispose()"/> as part
        /// of an explicit disposal</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                IList<ManagedBus> busInstances;
                lock (_syncRoot)
                {
                    busInstances = _busInstances.Values.ToList();
                }

                foreach (var busInstance in busInstances)
                {
                    busInstance.TryDispose();
                }
            }
        }
    }
}