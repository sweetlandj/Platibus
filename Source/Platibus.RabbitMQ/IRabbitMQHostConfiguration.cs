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
using Platibus.Security;

namespace Platibus.RabbitMQ
{
    /// <summary>
    /// Extends <see cref="IPlatibusConfiguration"/> with configuration specific to RabbitMQ hosting
    /// </summary>
    public interface IRabbitMQHostConfiguration : IPlatibusConfiguration
    {
        /// <summary>
        /// The base URI for the RabbitMQ hosted bus instance
        /// </summary>
        /// <remarks>
        /// This is the server URI.  The use of virtual hosts is recommended.
        /// </remarks>
        Uri BaseUri { get; }

        /// <summary>
        /// The encoding used to convert strings to byte streams when publishing 
        /// and consuming messages
        /// </summary>
        Encoding Encoding { get; }

        /// <summary>
        /// The maxiumum number of concurrent consumers that will be started
        /// for each queue
        /// </summary>
        int ConcurrencyLimit { get; }

        /// <summary>
        /// Whether queues should be configured to automatically acknowledge
        /// messages when read by a consumer
        /// </summary>
        bool AutoAcknowledge { get; }

        /// <summary>
        /// The maximum number of attempts to process a message before moving it to
        /// a dead letter queue
        /// </summary>
        int MaxAttempts { get; }

        /// <summary>
        /// The amount of time to wait between redelivery attempts
        /// </summary>
        TimeSpan RetryDelay { get; }

        /// <summary>
        /// Whether queues should be transient by default
        /// </summary>
        bool IsDurable { get; }

        /// <summary>
        /// A service that issues and validates security tokens to capture principal claims
        /// with queued messages
        /// </summary>
        ISecurityTokenService SecurityTokenService { get; }
    }
}