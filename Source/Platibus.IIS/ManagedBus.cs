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

using Platibus.Http;
using Platibus.Journaling;
using Platibus.Utils;
using System;
using System.Threading.Tasks;

namespace Platibus.IIS
{
    /// <inheritdoc />
    /// <summary>
    /// Encapsulates the IIS hosted bus and its dependencies for coordinated
    /// intialization and disposal.
    /// </summary>
    public class ManagedBus : IDisposable
    {
        private readonly ISubscriptionTrackingService _subscriptionTrackingService;
        private readonly IMessageQueueingService _messageQueueingService;
        private readonly IMessageJournal _messageJournal;

        private bool _disposed;

        /// <summary>
        /// Returns the managed bus instance
        /// </summary>
        /// <returns>Returns the managed bus instance</returns>
        public Bus Bus { get; }

        /// <summary>
        /// The transport service for this managed bus instance
        /// </summary>
        public HttpTransportService TransportService { get; }

        /// <summary>
        /// Initializes a new <see cref="ManagedBus"/> with the specified <paramref name="configuration"/>
        /// </summary>
        /// <param name="configuration">The configuration used to initialize the bus instance
        /// and its related components</param>
        public ManagedBus(IIISConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            var baseUri = configuration.BaseUri;
            _subscriptionTrackingService = configuration.SubscriptionTrackingService;
            _messageQueueingService = configuration.MessageQueueingService;
            _messageJournal = configuration.MessageJournal;
            
            var transportServiceOptions = new HttpTransportServiceOptions(baseUri, _messageQueueingService, _subscriptionTrackingService)
            {
                DiagnosticService = configuration.DiagnosticService,
                Endpoints = configuration.Endpoints,
                MessageJournal = configuration.MessageJournal,
                BypassTransportLocalDestination = configuration.BypassTransportLocalDestination
            };

            TransportService = new HttpTransportService(transportServiceOptions);

            Bus = InitBus(configuration, TransportService, _messageQueueingService).GetResultFromCompletionSource();
        }

        private static async Task<Bus> InitBus(IIISConfiguration cfg, HttpTransportService ts, IMessageQueueingService mqs)
        {
            var bus = new Bus(cfg, cfg.BaseUri, ts, mqs);
            await ts.Init();
            await bus.Init();
            return bus;
        }

        /// <summary>
        /// Finalizer that ensures resources are released
        /// </summary>
        ~ManagedBus()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Called by the <see cref="Dispose()"/> method or finalizer to ensure that
        /// resources are released
        /// </summary>
        /// <param name="disposing">Indicates whether this method is called from the 
        /// <see cref="Dispose()"/> method (<c>true</c>) or the finalizer (<c>false</c>)</param>
        /// <remarks>
        /// This method will not be called more than once
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            Bus.Dispose();
            TransportService.Dispose();
            if (_messageQueueingService is IDisposable disposableMessageQueueingService)
            {
                disposableMessageQueueingService.Dispose();
            }

            if (_messageJournal is IDisposable disposableMessageJournal)
            {
                disposableMessageJournal.Dispose();
            }

            if (_subscriptionTrackingService is IDisposable disposableSubscriptionTrackingService)
            {
                disposableSubscriptionTrackingService.Dispose();
            }
        }
    }
}