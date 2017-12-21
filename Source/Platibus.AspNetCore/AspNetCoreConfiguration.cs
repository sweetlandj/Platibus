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

using System;
using Platibus.Config;
using Platibus.Diagnostics;
using Platibus.Http;
using Platibus.InMemory;
using Platibus.Security;

namespace Platibus.AspNetCore
{
    /// <inheritdoc cref="PlatibusConfiguration"/>
    /// <inheritdoc cref="IAspNetCoreConfiguration"/>
    /// <summary>
    /// Extends the base <see cref="T:Platibus.Config.PlatibusConfiguration" /> with configuration
    /// specific to ASP.NET Core hosted applications
    /// </summary>
    public class AspNetCoreConfiguration : PlatibusConfiguration, IAspNetCoreConfiguration
    {
        private Uri _baseUri;

        /// <inheritdoc/>
        public Uri BaseUri
        {
            get => _baseUri ?? (_baseUri = new Uri("http://localhost/platibus"));
            set => _baseUri = value;
        }

        /// <inheritdoc/>
        public ISubscriptionTrackingService SubscriptionTrackingService { get; set; }

        /// <inheritdoc/>
        public IMessageQueueingService MessageQueueingService { get; set; }

        /// <inheritdoc/>
        public IAuthorizationService AuthorizationService { get; set; }

        /// <inheritdoc/>
        public IHttpClientFactory HttpClientFactory { get; set; }

        /// <inheritdoc/>
        public bool BypassTransportLocalDestination { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.AspNetCore.AspNetCoreConfiguration" /> with a 
        /// preconfigured <paramref name="diagnosticService" />
        /// </summary>
        public AspNetCoreConfiguration(IDiagnosticService diagnosticService = null) : base(diagnosticService)
        {
            SubscriptionTrackingService = new InMemorySubscriptionTrackingService();
            MessageQueueingService = new InMemoryMessageQueueingService();
        }
    }
}
