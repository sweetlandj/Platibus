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
using System.Text;
using Platibus.Config;
using Platibus.Diagnostics;
using Platibus.Security;

namespace Platibus.RabbitMQ
{
    /// <inheritdoc cref="PlatibusConfiguration" />
    /// <inheritdoc cref="IRabbitMQHostConfiguration" />
    public class RabbitMQHostConfiguration : PlatibusConfiguration, IRabbitMQHostConfiguration
    {
        /// <inheritdoc />
        public Uri BaseUri { get; set; }

        /// <inheritdoc />
        public Encoding Encoding { get; set; }

        /// <inheritdoc />
        public int ConcurrencyLimit { get; set; }

        /// <inheritdoc />
        public bool AutoAcknowledge { get; set; }

        /// <inheritdoc />
        public int MaxAttempts { get; set; }

        /// <inheritdoc />
        public TimeSpan RetryDelay { get; set; }

        /// <inheritdoc />
        public bool IsDurable { get; set; }

        /// <inheritdoc />
        public ISecurityTokenService SecurityTokenService { get; set; }

        /// <summary>
        /// Initializes a new <see cref="IRabbitMQHostConfiguration"/> with a preconfigured
        /// <paramref name="diagnosticService"/>
        /// </summary>
        /// <param name="diagnosticService">(Optional) The service through which diagnostic events
        /// are reported and processed</param>
        public RabbitMQHostConfiguration(IDiagnosticService diagnosticService = null) : base(diagnosticService)
        {
        }
    }
}
