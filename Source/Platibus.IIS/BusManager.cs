// The MIT License (MIT)
// 
// Copyright (c) 2016 Jesse Sweetland
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace Platibus.IIS
{
    /// <inheritdoc cref="IBusManager" />
    /// <inheritdoc cref="IDisposable" />
    /// <inheritdoc cref="IRegisteredObject" />
    /// <summary>
    /// Initializes an IIS-hosted bus instance
    /// </summary>
    public class BusManager : IBusManager, IDisposable, IRegisteredObject
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

        private BusManager()
        {
            HostingEnvironment.RegisterObject(this);
        }

        /// <inheritdoc />
        public virtual void Stop(bool immediate)
        {
            Dispose(true);
        }

        /// <inheritdoc />
        /// <summary>
        /// Provides access to the IIS-hosted bus with the specified 
        /// <paramref name="configuration" />.
        /// </summary>
        /// <param name="configuration">The bus configuration</param>
        /// <returns>Returns a task whose result is the bus instance</returns>
        public async Task<IBus> GetBus(IIISConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            var managedBus = await GetManagedBus(configuration);
            return await managedBus.GetBus();
        }

        internal async Task<ManagedBus> GetManagedBus(IIISConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var uri = configuration.BaseUri;
            ManagedBus bus;
            lock (_syncRoot)
            {
                if (_busInstances.TryGetValue(uri, out bus))
                {
                    return bus;
                }
                bus = new ManagedBus(configuration);
                _busInstances[configuration.BaseUri] = bus;
            }
            return await Task.FromResult(bus);
        }

        internal void DisposeManagedBus(IIISConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var uri = configuration.BaseUri;
            ManagedBus bus;
            lock (_syncRoot)
            {
                if (_busInstances.TryGetValue(uri, out bus))
                {
                    _busInstances.Remove(configuration.BaseUri);
                }
            }

            bus?.Dispose();
        }

        /// <summary>
        /// Allows an object to try to free resources and perform other cleanup operations before it is reclaimed by garbage collection.
        /// </summary>
        ~BusManager()
        {
            if (_disposed) return;
            Dispose(false);
        }

        /// <inheritdoc />
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
                IDictionary<Uri, ManagedBus> busInstances;
                lock (_syncRoot)
                {
                    busInstances = new Dictionary<Uri, ManagedBus>(_busInstances);
                }

                foreach (var busInstance in busInstances)
                {
                    var managedBus = busInstance.Value;
                    managedBus.Dispose();
                }
            }
        }
    }
}