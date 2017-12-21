// The MIT License (MIT)
// 
// Copyright (c) 2017 Jesse Sweetland
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

using Platibus.Diagnostics;
using Platibus.Journaling;
using System;

namespace Platibus.Http
{
    /// <summary>
    /// Options for configuring an <see cref="HttpTransportService"/>
    /// </summary>
    public class HttpTransportServiceOptions
    {
        /// <summary>
        /// The URI on which the HTTP server should listen
        /// </summary>
        public Uri BaseUri { get; }

        /// <summary>
        /// The message queueing service used to queue outbound messages
        /// </summary>
        public IMessageQueueingService MessageQueueingService { get; }

        /// <summary>
        /// The subscription tracking service
        /// </summary>
        public ISubscriptionTrackingService SubscriptionTrackingService { get; }
        
        /// <summary>
        /// Returns the diagnostic service provided by the host
        /// </summary>
        /// <remarks>
        /// This can be used by implementors to raise new diagnostic events during the 
        /// configuration process or register custom <see cref="IDiagnosticEventSink"/>s to
        /// handle diagnostic events.
        /// </remarks>
        public IDiagnosticService DiagnosticService { get; set; }
        
        /// <summary>
        /// The collection of named endpoints to which messages will be sent
        /// </summary>
        public IEndpointCollection Endpoints { get; set; } 
        
        /// <summary>
        /// A message log used to record and play back the sending, receipt, and publication of 
        /// messages.
        /// </summary>
        public IMessageJournal MessageJournal { get; set; }

        /// <summary>
        /// The factory class used to get or create <see cref="System.Net.Http.HttpClient"/>
        /// instances used to send messages to other applications.
        /// </summary>
        public IHttpClientFactory HttpClientFactory { get; set; }

        /// <summary>
        /// Whether the transport service can be bypassed when delivering messages
        /// whose destination and origination is the same.
        /// </summary>
        public bool BypassTransportLocalDestination { get; set; }

        /// <summary>
        /// Initializes a new set of <see cref="HttpTransportServiceOptions"/>
        /// </summary>
        /// <param name="baseUri">The base URI of the application</param>
        /// <param name="messageQueueingService">The message queueing service used
        /// to queue outbound messages</param>
        /// <param name="subscriptionTrackingService">The subscription tracking service</param>
        public HttpTransportServiceOptions(Uri baseUri, IMessageQueueingService messageQueueingService, ISubscriptionTrackingService subscriptionTrackingService)
        {
            BaseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));
            MessageQueueingService = messageQueueingService ?? throw new ArgumentNullException(nameof(messageQueueingService));
            SubscriptionTrackingService = subscriptionTrackingService ?? throw new ArgumentNullException(nameof(subscriptionTrackingService));
        }
    }
}
