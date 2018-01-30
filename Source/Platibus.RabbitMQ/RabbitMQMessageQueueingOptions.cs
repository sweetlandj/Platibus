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

using Platibus.Diagnostics;
using Platibus.Security;
using System;
using System.Text;

namespace Platibus.RabbitMQ
{
    /// <summary>
    /// Options governing the behavior of the <see cref="RabbitMQMessageQueueingService"/>
    /// </summary>
    public class RabbitMQMessageQueueingOptions
    {
        /// <summary>
        /// The diagnostic service through which events related to RabbitMQ queueing will
        /// be raised and processed
        /// </summary>
        public IDiagnosticService DiagnosticService { get; set; }
        
        /// <summary>
        /// The URI of the RabbitMQ server
        /// </summary>
        public Uri Uri { get; }

        /// <summary>
        /// The connection manager to use for connection pooling and recovery
        /// </summary>
        public IConnectionManager ConnectionManager { get; set; }

        /// <summary>
        /// The character encoding to use when reading and writing message headers and content
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Default options to apply when creating queues where no explicit options are specified
        /// </summary>
        public QueueOptions DefaultQueueOptions { get; set; }

        /// <summary>
        /// The message security token  service to use to issue and validate 
        /// security tokens for persisted messages.
        /// </summary>
        public ISecurityTokenService SecurityTokenService { get; set; }

        /// <summary>
        /// The encryption service used to encrypted persisted message files at rest
        /// </summary>
        public IMessageEncryptionService MessageEncryptionService { get; set; }

        /// <summary>
        /// Initializes new <see cref="RabbitMQMessageQueueingOptions"/>
        /// </summary>
        /// <param name="uri">The URI of the RabbitMQ server</param>
        public RabbitMQMessageQueueingOptions(Uri uri)
        {
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
        }
    }
}
