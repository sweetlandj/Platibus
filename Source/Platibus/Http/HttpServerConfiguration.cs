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
using System.Net;
using Platibus.Config;
using Platibus.Diagnostics;
using Platibus.InMemory;
using Platibus.Security;

namespace Platibus.Http
{
    /// <inheritdoc cref="PlatibusConfiguration"/>
    /// <inheritdoc cref="IHttpServerConfiguration"/>
    /// <summary>
    /// Configuration for hosting Platibus in a standalone HTTP server
    /// </summary>
    public class HttpServerConfiguration : PlatibusConfiguration, IHttpServerConfiguration
    {
        private Uri _baseUri;

        /// <inheritdoc />
        /// <summary>
        /// The URI on which the HTTP server should listen
        /// </summary>
        public Uri BaseUri
        {
            get => _baseUri ?? (_baseUri = new Uri("http://localhost/platibus"));
            set => _baseUri = value;
        }

        /// <inheritdoc />
        public AuthenticationSchemes AuthenticationSchemes { get; set; } = AuthenticationSchemes.Anonymous;

        /// <inheritdoc />
        public ISubscriptionTrackingService SubscriptionTrackingService { get; set; }

        /// <inheritdoc />
        public IMessageQueueingService MessageQueueingService { get; set; }

        /// <inheritdoc />
        public IAuthorizationService AuthorizationService { get; set; }

        /// <inheritdoc />
        public int ConcurrencyLimit { get; set; }

        /// <inheritdoc />
        public bool BypassTransportLocalDestination { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.Http.HttpServerConfiguration" /> with a preconfigured
        /// <paramref name="diagnosticService" />
        /// </summary>
        /// <param name="diagnosticService">The service through which diagnostic events are
        /// reported and processed</param>
        public HttpServerConfiguration(IDiagnosticService diagnosticService = null) : base(diagnosticService)
        {
            SubscriptionTrackingService = new InMemorySubscriptionTrackingService();
            MessageQueueingService = new InMemoryMessageQueueingService();
        }
    }
}