﻿// The MIT License (MIT)
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
using Platibus.Config;
using Platibus.Diagnostics;
using Platibus.InMemory;
using Platibus.Security;

namespace Platibus.Owin
{
    /// <inheritdoc cref="PlatibusConfiguration"/>
    /// <inheritdoc cref="IOwinConfiguration"/>
    /// <summary>
    /// Extends the base <see cref="T:Platibus.Config.PlatibusConfiguration" /> with Owin-specific configuration
    /// </summary>
    public class OwinConfiguration : PlatibusConfiguration, IOwinConfiguration
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
        /// <summary>
        /// The subscription tracking service implementation
        /// </summary>
        public ISubscriptionTrackingService SubscriptionTrackingService { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// The message queueing service implementation
        /// </summary>
        public IMessageQueueingService MessageQueueingService { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// An optional component used to restrict access for callers to send
        /// messages or subscribe to topics
        /// </summary>
        public IAuthorizationService AuthorizationService { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Whether the transport service can be bypassed when delivering messages
        /// whose destination and origination is the same.
        /// </summary>
        public bool BypassTransportLocalDestination { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.Owin.OwinConfiguration" /> with a preconfigured
        /// <paramref name="diagnosticService" />
        /// </summary>
        public OwinConfiguration(IDiagnosticService diagnosticService = null) : base(diagnosticService)
        {
            SubscriptionTrackingService = new InMemorySubscriptionTrackingService();
            MessageQueueingService = new InMemoryMessageQueueingService();
        }
    }
}