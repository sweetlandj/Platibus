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
using System.Threading;
using System.Threading.Tasks;
using Platibus.Http;

namespace Platibus.IIS
{
    /// <summary>
    /// Encapsulates the IIS hosted bus and its dependencies for coordinated
    /// intialization and disposal.
    /// </summary>
    public class ManagedBus : IDisposable
    {
        private readonly Task _initialization;

        private readonly IIISConfiguration _configuration;
        private Uri _baseUri;
        private ISubscriptionTrackingService _subscriptionTrackingService;
        private IMessageQueueingService _messageQueueingService;
        private IMessageJournalingService _messageJournalingService;
        private HttpTransportService _transportService;
        private Bus _bus;
        private bool _disposed;

        /// <summary>
        /// Returns the managed bus instance
        /// </summary>
        /// <returns>Returns the managed bus instance</returns>
        public async Task<Bus> GetBus()
        {
            await _initialization;
            return _bus;
        }
        
        /// <summary>
        /// Initializes a new <see cref="ManagedBus"/> with the specified <paramref name="configuration"/>
        /// </summary>
        /// <param name="configuration">The configuration used to initialize the bus instance
        /// and its related components</param>
        public ManagedBus(IIISConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            _configuration = configuration;
            _initialization = Init();
        }

        private async Task Init(CancellationToken cancellationToken = default(CancellationToken))
        {
            _baseUri = _configuration.BaseUri;
            _subscriptionTrackingService = _configuration.SubscriptionTrackingService;
            _messageQueueingService = _configuration.MessageQueueingService;
            _messageJournalingService = _configuration.MessageJournalingService;

            var endpoints = _configuration.Endpoints;
            _transportService = new HttpTransportService(_baseUri, endpoints, _messageQueueingService, 
                _messageJournalingService, _subscriptionTrackingService,
                _configuration.BypassTransportLocalDestination, HandleMessage);

            _bus = new Bus(_configuration, _baseUri, _transportService, _messageQueueingService);

            await _transportService.Init(cancellationToken);
            await _bus.Init(cancellationToken);
        }

        private async Task HandleMessage(Message message, CancellationToken cancellationToken)
        {
            await _initialization;
            await _bus.HandleMessage(message, Thread.CurrentPrincipal);
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
            if (disposing)
            {
                _bus.TryDispose();
                _transportService.TryDispose();
                _messageQueueingService.TryDispose();
                _messageJournalingService.TryDispose();
                _subscriptionTrackingService.TryDispose();
            }
        }
    }
}