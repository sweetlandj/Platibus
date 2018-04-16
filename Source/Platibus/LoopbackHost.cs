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

using Platibus.Config;
using Platibus.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus
{
    /// <inheritdoc />
    /// <summary>
    /// Simple Platibus host for passing messages within a single process
    /// </summary>
    /// <remarks>
    /// All messages are delivered to the local Platibus instance.  There is no need for
    /// send or subscription rules, although handling rules must still be specified.
    /// </remarks>
    public class LoopbackHost : IDisposable
    {
        /// <summary>
        /// Creates and starts a new <see cref="LoopbackHost"/>
        /// </summary>
        /// <returns>Returns the fully initialized and listening loopback host</returns>
#if NET452 || NET461
/// <seealso cref="LoopbackConfigurationSection"/> 
#endif
        public static LoopbackHost Start() => Start(null, _ => { });

        /// <summary>
        /// Creates and starts a new <see cref="LoopbackHost"/>
        /// </summary>
        /// <param name="configSectionName">The name of the configuration section containing
        /// the loopback host configuration</param>
        /// <returns>Returns the fully initialized and listening loopback host</returns>
#if NET452 || NET461
/// <seealso cref="LoopbackConfigurationSection"/> 
#endif
        public static LoopbackHost Start(string configSectionName) => Start(configSectionName, _ => { });

        /// <summary>
        /// Creates and starts a new <see cref="LoopbackHost"/>
        /// </summary>
        /// <param name="configure">Delegate used to modify the configuration loaded
        /// from the default configuration section</param>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be used by the
        /// caller to cancel loopback host initialization</param>
        /// <returns>Returns the fully initialized and listening loopback host</returns>
#if NET452 || NET461
/// <seealso cref="LoopbackConfigurationSection"/> 
#endif
        public static LoopbackHost Start(
            Action<LoopbackConfiguration> configure, 
            CancellationToken cancellationToken = default(CancellationToken)) => Start(null, configure, cancellationToken);

        /// <summary>
        /// Creates and starts a new <see cref="LoopbackHost"/>
        /// </summary>
        /// <param name="configure">Delegate used to modify the configuration loaded
        /// from the default configuration section</param>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be used by the
        /// caller to cancel loopback host initialization</param>
        /// <returns>Returns the fully initialized and listening loopback host</returns>
#if NET452 || NET461
/// <seealso cref="LoopbackConfigurationSection"/> 
#endif
        public static LoopbackHost Start(
            Func<LoopbackConfiguration, Task> configure, 
            CancellationToken cancellationToken = default(CancellationToken)) => Start(null, configure, cancellationToken);

        /// <summary>
        /// Creates and starts a new <see cref="LoopbackHost"/>
        /// </summary>
        /// <param name="configSectionName">The name of the configuration section from which
        /// declarative configuration should be loaded before invoking the <paramref name="configure"/></param>
        /// <param name="configure">Delegate used to modify the configuration loaded
        /// from the default configuration section</param>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be used by the
        /// caller to cancel loopback host initialization</param>
        /// <returns>Returns the fully initialized and listening loopback host</returns>
#if NET452 || NET461
/// <seealso cref="LoopbackConfigurationSection"/> 
#endif
        public static LoopbackHost Start(
            string configSectionName, 
            Action<LoopbackConfiguration> configure, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return Start(configSectionName, configuration =>
            {
                configure?.Invoke(configuration);
                return Task.FromResult(0);
            }, cancellationToken);
        }

        /// <summary>
        /// Creates and starts a new <see cref="LoopbackHost"/>
        /// </summary>
        /// <param name="configSectionName">The name of the configuration section from which
        /// declarative configuration should be loaded before invoking the 
        /// <paramref name="configure"/></param>
        /// <param name="configure">Delegate used to modify the configuration loaded
        /// from the default configuration section</param>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be used by the
        /// caller to cancel loopback host initialization</param>
        /// <returns>Returns the fully initialized and listening loopback host</returns>
#if NET452 || NET461
/// <seealso cref="LoopbackConfigurationSection"/> 
#endif
        public static LoopbackHost Start(
            string configSectionName,
            Func<LoopbackConfiguration, Task> configure, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return StartAsync(configSectionName, configure, cancellationToken)
                .GetResultFromCompletionSource(cancellationToken);
        }

        /// <summary>
        /// Creates and starts a new <see cref="LoopbackHost"/>
        /// </summary>
        /// <param name="configuration">The HTTP sever configuration</param>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be used by the
        /// caller to cancel loopback host initialization</param>
        /// <returns>Returns the fully initialized and listening loopback host</returns>
        public static LoopbackHost Start(ILoopbackConfiguration configuration, CancellationToken cancellationToken = default(CancellationToken))
        {
            return StartAsync(configuration, cancellationToken)
                .GetResultFromCompletionSource(cancellationToken);
        }

        private static async Task<LoopbackHost> StartAsync(
            string configSectionName,
            Func<LoopbackConfiguration, Task> configure, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var configManager = new LoopbackConfigurationManager();
            var configuration = new LoopbackConfiguration();
            await configManager.Initialize(configuration, configSectionName);
            await configManager.FindAndProcessConfigurationHooks(configuration);
            if (configure != null)
            {
                await configure(configuration);
            }

            var host = await StartAsync(configuration, cancellationToken);
            return host;
        }

        private static async Task<LoopbackHost> StartAsync(
            ILoopbackConfiguration configuration, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var host = new LoopbackHost(configuration);
            await host.Init(cancellationToken);
            return host;
        }

        private readonly Bus _bus;
        private bool _disposed;

        /// <summary>
        /// The hosted Platibus instance
        /// </summary>
        /// <returns>Returns the hosted Platibus</returns>
        public IBus Bus => _bus;

        private LoopbackHost(ILoopbackConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            // Placeholder value; required by the bus
            var baseUri = configuration.BaseUri;
            var transportService = new LoopbackTransportService(configuration.MessageJournal);
            _bus = new Bus(configuration, baseUri, transportService, configuration.MessageQueueingService);
        }

        private async Task Init(CancellationToken cancellationToken = default(CancellationToken))
        {
            await _bus.Init(cancellationToken);
        }
        
        /// <summary>
        /// Finalizer to ensure that resources are released
        /// </summary>
        ~LoopbackHost()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Called by the <see cref="Dispose()"/> method or the finalizer to ensure that
        /// resources are released
        /// </summary>
        /// <param name="disposing">Indicates whether this method is called from the 
        /// <see cref="Dispose()"/> method (<c>true</c>) or the finalizer (<c>false</c>)</param>
        /// <remarks>
        /// This method will not be called more than once
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _bus.Dispose();
            }
        }
    }
}