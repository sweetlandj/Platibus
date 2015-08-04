// The MIT License (MIT)
// 
// Copyright (c) 2015 Jesse Sweetland
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

namespace Platibus.Config
{
    /// <summary>
    /// Extension methods for <see cref="PlatibusConfiguration"/> objects to
    /// simplify the structure and syntax for adding rules.
    /// </summary>
    public static class PlatibusConfigurationExtensions
    {
        /// <summary>
        /// Adds a handling rule based on message name pattern matching
        /// </summary>
        /// <param name="configuration">The configuration object to which the
        /// handling rule is being added</param>
        /// <param name="namePattern">A regular expression used to match
        /// message names</param>
        /// <param name="messageHandler">The message handler to which matching
        /// messages will be routed</param>
        /// <param name="queueName">(Optional) The name of the queue to which
        /// the handler will be attached</param>
        public static void AddHandlingRule(this PlatibusConfiguration configuration, string namePattern,
            IMessageHandler messageHandler, QueueName queueName = null)
        {
            var specification = new MessageNamePatternSpecification(namePattern);
            var handlingRule = new HandlingRule(specification, messageHandler, queueName);
            configuration.AddHandlingRule(handlingRule);
        }

        /// <summary>
        /// Adds a handling rule based on message name pattern matching
        /// </summary>
        /// <typeparam name="TContent">The type of deserialized message 
        /// content expected</typeparam>
        /// <param name="configuration">The configuration object to which the
        /// handling rule is being added</param>
        /// <param name="namePattern">A regular expression used to match
        /// message names</param>
        /// <param name="messageHandler">The message handler to which matching
        /// messages will be routed</param>
        /// <param name="queueName">(Optional) The name of the queue to which
        /// the handler will be attached</param>
        public static void AddHandlingRule<TContent>(this PlatibusConfiguration configuration, string namePattern,
            IMessageHandler<TContent> messageHandler, QueueName queueName = null)
        {
            var specification = new MessageNamePatternSpecification(namePattern);
            var genericMessageHandler = GenericMessageHandlerAdapter.For(messageHandler);
            var handlingRule = new HandlingRule(specification, genericMessageHandler, queueName);
            configuration.AddHandlingRule(handlingRule);
        }

        /// <summary>
        /// Adds a handling rule based on the supplied message specification and
        /// function delegate
        /// </summary>
        /// <typeparam name="TContent">The type of deserialized message 
        /// content expected</typeparam>
        /// <param name="configuration">The configuration object to which the
        /// handling rule is being added</param>
        /// <param name="specification">The message specification used to match
        /// messages for the handler</param>
        /// <param name="handleContent">A function delegate that will be used to
        /// handle messages</param>
        /// <param name="queueName">(Optional) The name of the queue to which
        /// the handler will be attached</param>
        public static void AddHandlingRule<TContent>(this PlatibusConfiguration configuration,
            IMessageSpecification specification,
            Func<TContent, IMessageContext, Task> handleContent, QueueName queueName = null)
        {
            var messageHandler = DelegateMessageHandler.For(handleContent);
            var handlingRule = new HandlingRule(specification, messageHandler, queueName);
            configuration.AddHandlingRule(handlingRule);
        }

        /// <summary>
        /// Adds a handling rule based on the supplied message specification and
        /// function delegate
        /// </summary>
        /// <typeparam name="TContent">The type of deserialized message 
        /// content expected</typeparam>
        /// <param name="configuration">The configuration object to which the
        /// handling rule is being added</param>
        /// <param name="specification">The message specification used to match
        /// messages for the handler</param>
        /// <param name="handleContent">A function delegate that will be used to
        /// handle messages</param>
        /// <param name="queueName">(Optional) The name of the queue to which
        /// the handler will be attached</param>
        public static void AddHandlingRule<TContent>(this PlatibusConfiguration configuration,
            IMessageSpecification specification,
            Func<TContent, IMessageContext, CancellationToken, Task> handleContent,
            QueueName queueName = null)
        {
            var messageHandler = DelegateMessageHandler.For(handleContent);
            var handlingRule = new HandlingRule(specification, messageHandler, queueName);
            configuration.AddHandlingRule(handlingRule);
        }

        /// <summary>
        /// Adds a handling rule based on the supplied message specification and
        /// action delegate
        /// </summary>
        /// <typeparam name="TContent">The type of deserialized message 
        /// content expected</typeparam>
        /// <param name="configuration">The configuration object to which the
        /// handling rule is being added</param>
        /// <param name="specification">The message specification used to match
        /// messages for the handler</param>
        /// <param name="handleContent">An action delegate that will be used to
        /// handle messages</param>
        /// <param name="queueName">(Optional) The name of the queue to which
        /// the handler will be attached</param>
        public static void AddHandlingRule<TContent>(this PlatibusConfiguration configuration,
            IMessageSpecification specification, Action<TContent, IMessageContext> handleContent,
            QueueName queueName = null)
        {
            var messageHandler = DelegateMessageHandler.For(handleContent);
            var handlingRule = new HandlingRule(specification, messageHandler, queueName);
            configuration.AddHandlingRule(handlingRule);
        }

        /// <summary>
        /// Adds a handling rule based on the a name pattern and function delegate
        /// </summary>
        /// <typeparam name="TContent">The type of deserialized message 
        /// content expected</typeparam>
        /// <param name="configuration">The configuration object to which the
        /// handling rule is being added</param>
        /// <param name="namePattern">A regular expression used to match message
        /// names for the handler</param>
        /// <param name="handleContent">A function delegate that will be used to
        /// handle messages</param>
        /// <param name="queueName">(Optional) The name of the queue to which
        /// the handler will be attached</param>
        public static void AddHandlingRule<TContent>(this PlatibusConfiguration configuration, string namePattern,
            Func<TContent, IMessageContext, Task> handleContent, QueueName queueName = null)
        {
            var specification = new MessageNamePatternSpecification(namePattern);
            var messageHandler = DelegateMessageHandler.For(handleContent);
            var handlingRule = new HandlingRule(specification, messageHandler, queueName);
            configuration.AddHandlingRule(handlingRule);
        }

        /// <summary>
        /// Adds a handling rule based on the a name pattern and function delegate
        /// </summary>
        /// <typeparam name="TContent">The type of deserialized message 
        /// content expected</typeparam>
        /// <param name="configuration">The configuration object to which the
        /// handling rule is being added</param>
        /// <param name="namePattern">A regular expression used to match message
        /// names for the handler</param>
        /// <param name="handleContent">A function delegate that will be used to
        /// handle messages</param>
        /// <param name="queueName">(Optional) The name of the queue to which
        /// the handler will be attached</param>
        public static void AddHandlingRule<TContent>(this PlatibusConfiguration configuration, string namePattern,
            Func<TContent, IMessageContext, CancellationToken, Task> handleContent,
            QueueName queueName = null)
        {
            var specification = new MessageNamePatternSpecification(namePattern);
            var messageHandler = DelegateMessageHandler.For(handleContent);
            var handlingRule = new HandlingRule(specification, messageHandler, queueName);
            configuration.AddHandlingRule(handlingRule);
        }

        /// <summary>
        /// Adds a handling rule based on the a name pattern and action delegate
        /// </summary>
        /// <typeparam name="TContent">The type of deserialized message 
        /// content expected</typeparam>
        /// <param name="configuration">The configuration object to which the
        /// handling rule is being added</param>
        /// <param name="namePattern">A regular expression used to match message
        /// names for the handler</param>
        /// <param name="handleContent">An action delegate that will be used to
        /// handle messages</param>
        /// <param name="queueName">(Optional) The name of the queue to which
        /// the handler will be attached</param>
        public static void AddHandlingRule<TContent>(this PlatibusConfiguration configuration, string namePattern,
            Action<TContent, IMessageContext> handleContent, QueueName queueName = null)
        {
            var specification = new MessageNamePatternSpecification(namePattern);
            var messageHandler = DelegateMessageHandler.For(handleContent);
            var handlingRule = new HandlingRule(specification, messageHandler, queueName);
            configuration.AddHandlingRule(handlingRule);
        }
    }
}