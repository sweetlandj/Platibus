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
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Platibus.Config
{
    /// <summary>
    /// The default <see cref="IHandlingRule"/> implementation
    /// </summary>
    public class HandlingRule : IHandlingRule
    {
        private readonly IMessageHandler _messageHandler;
        private readonly QueueName _queueName;
        private readonly IMessageSpecification _specification;
        private readonly QueueOptions _queueOptions;

        /// <inheritdoc />
        public IMessageSpecification Specification
        {
            get { return _specification; }
        }

        /// <inheritdoc />
        public IMessageHandler MessageHandler
        {
            get { return _messageHandler; }
        }

        /// <inheritdoc />
        public QueueName QueueName
        {
            get { return _queueName; }
        }

        /// <inheritdoc />
        public QueueOptions QueueOptions
        {
            get { return _queueOptions; }
        }

        /// <summary>
        /// Initializes a new <see cref="HandlingRule"/> with the supplied message
        /// <paramref name="specification"/>, <see cref="MessageHandler"/>, and
        /// <paramref name="queueName"/>.
        /// </summary>
        /// <param name="specification">The message specification that selects messages 
        /// to which the handling rule applies</param>
        /// <param name="messageHandler">The handler to which messages matching the
        /// specification will be routed</param>
        /// <param name="queueName">(Optional) The name of the queue in which matching
        /// messages will be placed while they await handling</param>
        /// <param name="queueOptions">(Optional) Options for how queued messages for the handler 
        /// should be processed</param>
        /// <remarks>
        /// If the <paramref name="queueName"/> is ommitted, a default queue name will
        /// be generated based on the MD5 hash of the full type name of the supplied
        /// <paramref name="messageHandler"/>
        /// </remarks>
        /// <seealso cref="GenerateQueueName"/>
        public HandlingRule(IMessageSpecification specification, IMessageHandler messageHandler, QueueName queueName = null, QueueOptions queueOptions = default(QueueOptions))
        {
            if (specification == null) throw new ArgumentNullException("specification");
            if (messageHandler == null) throw new ArgumentNullException("messageHandler");
            _specification = specification;
            _messageHandler = messageHandler;
            _queueName = queueName ?? GenerateQueueName(messageHandler);
            _queueOptions = queueOptions;
        }

        /// <summary>
        /// Deterministically generates a queue name for a 
        /// <paramref name="messageHandler"/> based on the MD5 hash of its full
        /// type name
        /// </summary>
        /// <param name="messageHandler">The message handler</param>
        /// <returns>A unique and deterministic queue name based on the message
        /// handler type</returns>
        public static QueueName GenerateQueueName(IMessageHandler messageHandler)
        {
            // Produce a short type name that is safe (ish?) from collisions
            var typeName = messageHandler.GetType().FullName;
            var typeNameBytes = Encoding.UTF8.GetBytes(typeName);
            var typeHash = MD5.Create().ComputeHash(typeNameBytes);
            return string.Join("", typeHash.Select(b => ((ushort) b).ToString("x2")));
        }
    }
}