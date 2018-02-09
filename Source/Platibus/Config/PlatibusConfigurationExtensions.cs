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
using System.Text.RegularExpressions;
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
        /// <param name="queueOptions">(Optional) Options for how queued messages for the handler 
        /// should be processed</param>
        public static void AddHandlingRule(this PlatibusConfiguration configuration, 
            string namePattern, IMessageHandler messageHandler, 
            QueueName queueName = null, QueueOptions queueOptions = null)
        {
            var specification = new MessageNamePatternSpecification(namePattern);
            var handlingRule = new HandlingRule(specification, messageHandler, queueName, queueOptions);
            configuration.AddHandlingRule(handlingRule);
        }

        /// <summary>
        /// Adds a handling rule based on message name pattern matching
        /// </summary>
        /// <param name="configuration">The configuration object to which the
        /// handling rule is being added</param>
        /// <param name="namePattern">A regular expression used to match
        /// message names</param>
        /// <param name="handlerFactory">A factory method for getting an instance of the handler 
        /// (may return a singleton or scoped handler instance)</param>
        /// <param name="queueName">(Optional) The name of the queue to which
        /// the handler will be attached</param>
        /// <param name="queueOptions">(Optional) Options for how queued messages for the handler 
        /// should be processed</param>
        public static void AddHandlingRule(this PlatibusConfiguration configuration,
            string namePattern, Func<IMessageHandler> handlerFactory,
            QueueName queueName = null, QueueOptions queueOptions = null)
        {
            var specification = new MessageNamePatternSpecification(namePattern);
            var messageHandler = MessageHandlerFactory.For(handlerFactory);
            var handlingRule = new HandlingRule(specification, messageHandler, queueName, queueOptions);
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
        /// <param name="queueOptions">(Optional) Options for how queued messages for the handler 
        /// should be processed</param>
        public static void AddHandlingRule<TContent>(this PlatibusConfiguration configuration, 
            string namePattern, IMessageHandler<TContent> messageHandler, 
            QueueName queueName = null, QueueOptions queueOptions = null)
        {
            var specification = new MessageNamePatternSpecification(namePattern);
            var genericMessageHandler = GenericMessageHandlerAdapter.For(messageHandler);
            var handlingRule = new HandlingRule(specification, genericMessageHandler, queueName, queueOptions);
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
        /// <param name="handlerFactory">A factory method for getting an instance of the handler 
        /// (may return a singleton or scoped handler instance)</param>
        /// <param name="queueName">(Optional) The name of the queue to which
        /// the handler will be attached</param>
        /// <param name="queueOptions">(Optional) Options for how queued messages for the handler 
        /// should be processed</param>
        public static void AddHandlingRule<TContent>(this PlatibusConfiguration configuration,
            string namePattern, Func<IMessageHandler<TContent>> handlerFactory,
            QueueName queueName = null, QueueOptions queueOptions = null)
        {
            var specification = new MessageNamePatternSpecification(namePattern);
            var messageHandler = new DelegateMessageHandler((content, context, ct) =>
            {
                var handler = GenericMessageHandlerAdapter.For(handlerFactory());
                return handler.HandleMessage(content, context, ct);
            });
            var handlingRule = new HandlingRule(specification, messageHandler, queueName, queueOptions);
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
        /// <param name="queueOptions">(Optional) Options for how queued messages for the handler 
        /// should be processed</param>
        public static void AddHandlingRule<TContent>(this PlatibusConfiguration configuration,
            IMessageSpecification specification, Func<TContent, IMessageContext, Task> handleContent, 
            QueueName queueName = null, QueueOptions queueOptions = null)
        {
            var messageHandler = DelegateMessageHandler.For(handleContent);
            var handlingRule = new HandlingRule(specification, messageHandler, queueName, queueOptions);
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
        /// <param name="queueOptions">(Optional) Options for how queued messages for the handler 
        /// should be processed</param>
        public static void AddHandlingRule<TContent>(this PlatibusConfiguration configuration,
            IMessageSpecification specification,
            Func<TContent, IMessageContext, CancellationToken, Task> handleContent,
            QueueName queueName = null, QueueOptions queueOptions = null)
        {
            var messageHandler = DelegateMessageHandler.For(handleContent);
            var handlingRule = new HandlingRule(specification, messageHandler, queueName, queueOptions);
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
        /// <param name="queueOptions">(Optional) Options for how queued messages for the handler 
        /// should be processed</param>
        public static void AddHandlingRule<TContent>(this PlatibusConfiguration configuration,
            IMessageSpecification specification, Action<TContent, IMessageContext> handleContent,
            QueueName queueName = null, QueueOptions queueOptions = null)
        {
            var messageHandler = DelegateMessageHandler.For(handleContent);
            var handlingRule = new HandlingRule(specification, messageHandler, queueName, queueOptions);
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
        /// <param name="queueOptions">(Optional) Options for how queued messages for the handler 
        /// should be processed</param>
        public static void AddHandlingRule<TContent>(this PlatibusConfiguration configuration, 
            string namePattern, Func<TContent, IMessageContext, Task> handleContent, 
            QueueName queueName = null, QueueOptions queueOptions = null)
        {
            var specification = new MessageNamePatternSpecification(namePattern);
            var messageHandler = DelegateMessageHandler.For(handleContent);
            var handlingRule = new HandlingRule(specification, messageHandler, queueName, queueOptions);
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
        /// <param name="queueOptions">(Optional) Options for how queued messages for the handler 
        /// should be processed</param>
        public static void AddHandlingRule<TContent>(this PlatibusConfiguration configuration, string namePattern,
            Func<TContent, IMessageContext, CancellationToken, Task> handleContent,
            QueueName queueName = null, QueueOptions queueOptions = null)
        {
            var specification = new MessageNamePatternSpecification(namePattern);
            var messageHandler = DelegateMessageHandler.For(handleContent);
            var handlingRule = new HandlingRule(specification, messageHandler, queueName, queueOptions);
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
        /// <param name="queueOptions">(Optional) Options for how queued messages for the handler 
        /// should be processed</param>
        public static void AddHandlingRule<TContent>(this PlatibusConfiguration configuration, 
            string namePattern, Action<TContent, IMessageContext> handleContent, 
            QueueName queueName = null, QueueOptions queueOptions = null)
        {
            var specification = new MessageNamePatternSpecification(namePattern);
            var messageHandler = DelegateMessageHandler.For(handleContent);
            var handlingRule = new HandlingRule(specification, messageHandler, queueName, queueOptions);
            configuration.AddHandlingRule(handlingRule);
        }

        /// <summary>
        /// Uses reflection to add handling rules for every implemented <see cref="IMessageHandler{TContent}"/>
        /// interface on the specified <paramref name="handler"/> instance.
        /// </summary>
        /// <remarks>
        /// <para>A handling rule will be added for implemented <see cref="IMessageHandler{TContent}"/> interface 
        /// of <typeparamref name="THandler"/> according to the following convention:</para>
        /// <list type="bullet">
        /// <item>The name pattern will be an exact match of the <see cref="MessageName"/> returned by the
        /// <see cref="IMessageNamingService"/> in the specified <paramref name="configuration"/></item>
        /// <item>The handling function will rinvoke the appropriate <see cref="IMessageHandler{TContent}.HandleMessage"/>
        /// method (via reflection) on the specified <paramref name="handler"/> instance</item>
        /// <item>The designated queue name will be a hash derived from the full names of the <typeparamref name="THandler"/>
        /// type and the type of message that is handled</item>
        /// </list>
        /// </remarks>
        /// <typeparam name="THandler">The type of message handler</typeparam>
        /// <param name="configuration">The Platibus configuration to which the handling rules will be added</param>
        /// <param name="handler">The singleton handler instance</param>
        /// <param name="queueNameFactory">(Optional) A factory method that will return an appropriate queue
        /// name for each combination of handler type and message type</param>
        /// <param name="queueOptions">(Optional) Options for how queued messages for the handler 
        /// should be processed</param>
        public static void AddHandlingRules<THandler>(this PlatibusConfiguration configuration, 
            THandler handler, QueueNameFactory queueNameFactory = null, 
            QueueOptions queueOptions = null)
        {
            configuration.AddHandlingRulesForType(typeof(THandler), () => handler, queueNameFactory, queueOptions);
        }

        /// <summary>
        /// Uses reflection to add handling rules for every implemented <see cref="IMessageHandler{TContent}"/>
        /// interface.
        /// </summary>
        /// <remarks>
        /// <para>A handling rule will be added for implemented <see cref="IMessageHandler{TContent}"/> interface 
        /// of <typeparamref name="THandler"/> according to the following convention:</para>
        /// <list type="bullet">
        /// <item>The name pattern will be an exact match of the <see cref="MessageName"/> returned by the
        /// <see cref="IMessageNamingService"/> in the specified <paramref name="configuration"/></item>
        /// <item>The handling function will retrieve an instance of the handler object using the supplied
        /// <paramref name="handlerFactory"/> and invoke the appropriate <see cref="IMessageHandler{TContent}.HandleMessage"/>
        /// method (via reflection)</item>
        /// <item>The designated queue name will be a hash derived from the full names of the <typeparamref name="THandler"/>
        /// type and the type of message that is handled</item>
        /// </list>
        /// </remarks>
        /// <typeparam name="THandler">The type of message handler</typeparam>
        /// <param name="configuration">The Platibus configuration to which the handling rules will be added</param>
        /// <param name="handlerFactory">(Optional) A factory method for getting an instance of the handler (may 
        /// return a singleton or scoped handler instance).  If no factory method is specified, then the
        /// default constructor will be used.</param>
        /// <param name="queueNameFactory">(Optional) A factory method that will return an appropriate queue
        /// name for each combination of handler type and message type</param>
        /// <param name="queueOptions">(Optional) Options for how queued messages for the handler 
        /// should be processed</param>
        public static void AddHandlingRules<THandler>(this PlatibusConfiguration configuration,
            Func<THandler> handlerFactory = null, QueueNameFactory queueNameFactory = null,
            QueueOptions queueOptions = null)
        {
            Func<object> factory = null;
            if (handlerFactory != null)
            {
                factory = () => handlerFactory();
            }
            configuration.AddHandlingRulesForType(typeof(THandler), factory, queueNameFactory, queueOptions);
        }

        /// <summary>
        /// Uses reflection to add handling rules for every implemented <see cref="IMessageHandler{TContent}"/>
        /// interface.
        /// </summary>
        /// <remarks>
        /// <para>A handling rule will be added for implemented <see cref="IMessageHandler{TContent}"/> interface 
        /// of <paramref name="handlerType"/> according to the following convention:</para>
        /// <list type="bullet">
        /// <item>The name pattern will be an exact match of the <see cref="MessageName"/> returned by the
        /// <see cref="IMessageNamingService"/> in the specified <paramref name="configuration"/></item>
        /// <item>The handling function will retrieve an instance of the handler object using the supplied
        /// <paramref name="handlerFactory"/> and invoke the appropriate <see cref="IMessageHandler{TContent}.HandleMessage"/>
        /// method (via reflection)</item>
        /// <item>The designated queue name will be a hash derived from the full names of the <paramref name="handlerType"/> 
        /// and the type of message that is handled</item>
        /// </list>
        /// </remarks>
        /// <param name="configuration">The Platibus configuration to which the handling rules will be added</param>
        /// <param name="handlerType">The type of handler returned by the factory method</param>
        /// <param name="handlerFactory">(Optional) A factory method for getting an instance of the handler (may 
        /// return a singleton or scoped handler instance).  If no factory method is specified, then the
        /// default constructor will be used.</param>
        /// <param name="queueNameFactory">(Optional) A factory method that will return an appropriate queue
        /// name for each combination of handler type and message type</param>
        /// <param name="queueOptions">(Optional) Options for how queued messages for the handler 
        /// should be processed</param>
        public static void AddHandlingRulesForType(this PlatibusConfiguration configuration, 
            Type handlerType, Func<object> handlerFactory = null, 
            QueueNameFactory queueNameFactory = null, QueueOptions queueOptions = null)
        {
            var diagnosticService = configuration.DiagnosticService;
            var autoBindInterfaces = handlerType
                .GetInterfaces()
                .Where(i => i.IsGenericType && typeof(IMessageHandler<>).IsAssignableFrom(i.GetGenericTypeDefinition()));

            if (handlerFactory == null)
            {
                handlerFactory = () => Activator.CreateInstance(handlerType);
            }

            if (queueNameFactory == null)
            {
                queueNameFactory = DeriveQueueName;
            }

            foreach (var autoBindInterface in autoBindInterfaces)
            {
                var messageType = autoBindInterface.GetGenericArguments().First();
                var messageName = configuration.MessageNamingService.GetNameForType(messageType);
                var specification = new MessageNamePatternSpecification("^" + Regex.Escape(messageName) + "$");
                var queueName = queueNameFactory(handlerType, messageType);

                var args = new[] {messageType, typeof(IMessageContext), typeof(CancellationToken)};
                var method = autoBindInterface.GetMethod("HandleMessage", args);

                if (method == null) continue;

                var handlerActivation = new HandlerActivation(diagnosticService, handlerType, handlerFactory, method);
                var rule = new HandlingRule(specification, handlerActivation, queueName, queueOptions);
                configuration.AddHandlingRule(rule);
            }
        }

        private static QueueName DeriveQueueName(Type handlerType, Type messageType)
        {
            var rawStr = handlerType.FullName + "_" + messageType.FullName;
            var rawBytes = Encoding.UTF8.GetBytes(rawStr);
            var typeHash = MD5.Create().ComputeHash(rawBytes);
            return string.Concat(typeHash.Select(b => ((ushort)b).ToString("x2")));
        }
    }
}