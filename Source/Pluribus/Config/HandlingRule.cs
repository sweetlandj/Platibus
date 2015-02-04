// The MIT License (MIT)
// 
// Copyright (c) 2014 Jesse Sweetland
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

namespace Pluribus.Config
{
    public class HandlingRule : IHandlingRule
    {
        private readonly IMessageHandler _messageHandler;
        private readonly QueueName _queueName;
        private readonly IMessageSpecification _messageSpecification;

        public IMessageSpecification MessageSpecification
        {
            get { return _messageSpecification; }
        }

        public IMessageHandler MessageHandler
        {
            get { return _messageHandler; }
        }

        public QueueName QueueName
        {
            get { return _queueName; }
        }

        public HandlingRule(IMessageSpecification messageSpecification, IMessageHandler messageHandler,
            QueueName queueName = null)
        {
            if (messageSpecification == null) throw new ArgumentNullException("messageSpecification");
            if (messageHandler == null) throw new ArgumentNullException("messageHandler");
            _messageSpecification = messageSpecification;
            _messageHandler = messageHandler;
            _queueName = queueName ?? GenerateQueueName(messageHandler);
        }

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