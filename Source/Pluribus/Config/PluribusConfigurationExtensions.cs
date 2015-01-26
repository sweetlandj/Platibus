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
using System.Threading;
using System.Threading.Tasks;
using Pluribus.Serialization;

namespace Pluribus.Config
{
    public static class PluribusConfigurationExtensions
    {
        public static void AddHandlingRule(this PluribusConfiguration configuration, string namePattern,
            IMessageHandler messageHandler, QueueName queueName = null)
        {
            var specification = new MessageNamePatternSpecification(namePattern);
            configuration.AddHandlingRule(specification, messageHandler, queueName);
        }

        public static void AddHandlingRule(this PluribusConfiguration configuration, IMessageSpecification specification,
            Func<Message, IMessageContext, Task> handleMessage, QueueName queueName = null)
        {
            var messageHandler = new DelegateMessageHandler(handleMessage);
            configuration.AddHandlingRule(specification, messageHandler, queueName);
        }

        public static void AddHandlingRule(this PluribusConfiguration configuration, IMessageSpecification specification,
            Func<Message, IMessageContext, CancellationToken, Task> handleMessage, QueueName queueName = null)
        {
            var messageHandler = new DelegateMessageHandler(handleMessage);
            configuration.AddHandlingRule(specification, messageHandler, queueName);
        }

        public static void AddHandlingRule(this PluribusConfiguration configuration, IMessageSpecification specification,
            Action<Message, IMessageContext> handleMessage, QueueName queueName = null)
        {
            var messageHandler = new DelegateMessageHandler(handleMessage);
            configuration.AddHandlingRule(specification, messageHandler, queueName);
        }

        public static void AddHandlingRule(this PluribusConfiguration configuration, string namePattern,
            Func<Message, IMessageContext, Task> handleMessage, QueueName queueName = null)
        {
            var specification = new MessageNamePatternSpecification(namePattern);
            var messageHandler = new DelegateMessageHandler(handleMessage);
            configuration.AddHandlingRule(specification, messageHandler, queueName);
        }

        public static void AddHandlingRule(this PluribusConfiguration configuration, string namePattern,
            Func<Message, IMessageContext, CancellationToken, Task> handleMessage, QueueName queueName = null)
        {
            var specification = new MessageNamePatternSpecification(namePattern);
            var messageHandler = new DelegateMessageHandler(handleMessage);
            configuration.AddHandlingRule(specification, messageHandler, queueName);
        }

        public static void AddHandlingRule(this PluribusConfiguration configuration, string namePattern,
            Action<Message, IMessageContext> handleMessage, QueueName queueName = null)
        {
            var specification = new MessageNamePatternSpecification(namePattern);
            var messageHandler = new DelegateMessageHandler(handleMessage);
            configuration.AddHandlingRule(specification, messageHandler, queueName);
        }

        public static void AddHandlingRule<TContent>(this PluribusConfiguration configuration,
            IMessageSpecification specification,
            Func<TContent, IMessageContext, Task> handleContent, QueueName queueName = null)
        {
            var messageHandler = new DelegateDeserializedContentHandler<TContent>(handleContent);
            configuration.AddHandlingRule(specification, messageHandler, queueName);
        }

        public static void AddHandlingRule<TContent>(this PluribusConfiguration configuration,
            IMessageSpecification specification,
            Func<TContent, IMessageContext, CancellationToken, Task> handleContent,
            QueueName queueName = null)
        {
            var messageHandler = new DelegateDeserializedContentHandler<TContent>(handleContent);
            configuration.AddHandlingRule(specification, messageHandler, queueName);
        }

        public static void AddHandlingRule<TContent>(this PluribusConfiguration configuration,
            IMessageSpecification specification, Action<TContent, IMessageContext> handleContent,
            QueueName queueName = null)
        {
            var messageHandler = new DelegateDeserializedContentHandler<TContent>(handleContent);
            configuration.AddHandlingRule(specification, messageHandler, queueName);
        }

        public static void AddHandlingRule<TContent>(this PluribusConfiguration configuration, string namePattern,
            Func<TContent, IMessageContext, Task> handleContent, QueueName queueName = null)
        {
            var specification = new MessageNamePatternSpecification(namePattern);
            var messageHandler = new DelegateDeserializedContentHandler<TContent>(handleContent);
            configuration.AddHandlingRule(specification, messageHandler, queueName);
        }

        public static void AddHandlingRule<TContent>(this PluribusConfiguration configuration, string namePattern,
            Func<TContent, IMessageContext, CancellationToken, Task> handleContent,
            QueueName queueName = null)
        {
            var specification = new MessageNamePatternSpecification(namePattern);
            var messageHandler = new DelegateDeserializedContentHandler<TContent>(handleContent);
            configuration.AddHandlingRule(specification, messageHandler, queueName);
        }

        public static void AddHandlingRule<TContent>(this PluribusConfiguration configuration, string namePattern,
            Action<TContent, IMessageContext> handleContent, QueueName queueName = null)
        {
            var specification = new MessageNamePatternSpecification(namePattern);
            var messageHandler = new DelegateDeserializedContentHandler<TContent>(handleContent);
            configuration.AddHandlingRule(specification, messageHandler, queueName);
        }
    }
}