﻿// The MIT License (MIT)
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

using System;
using Platibus.Config;
using Platibus.Http;
using Platibus.Security;

namespace Platibus.AspNetCore
{
    public interface IAspNetCoreConfiguration : IPlatibusConfiguration
    {
        /// <summary>
        /// The URI on which the HTTP server should listen
        /// </summary>
        Uri BaseUri { get; }

        /// <summary>
        /// The subscription tracking service implementation
        /// </summary>
        ISubscriptionTrackingService SubscriptionTrackingService { get; }

        /// <summary>
        /// The message queueing service implementation
        /// </summary>
        IMessageQueueingService MessageQueueingService { get; }

        /// <summary>
        /// An optional component used to restrict access for callers to send
        /// messages or subscribe to topics
        /// </summary>
        IAuthorizationService AuthorizationService { get; }

        /// <summary>
        /// The factory class used to get or create <see cref="System.Net.Http.HttpClient"/>
        /// instances used to send messages to other applications.
        /// </summary>
        IHttpClientFactory HttpClientFactory { get; }

        /// <summary>
        /// Whether the transport service can be bypassed when delivering messages
        /// whose destination and origination is the same.
        /// </summary>
        bool BypassTransportLocalDestination { get; }
    }
}