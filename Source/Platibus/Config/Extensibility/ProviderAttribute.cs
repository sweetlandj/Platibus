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
using Platibus.SQL;

namespace Platibus.Config.Extensibility
{
    /// <summary>
    /// Associates the decorated type with a provider name.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Decorated types must have a default constructor and must implement at 
    /// least one of the following interfaces:
    /// </para>
    /// <list type="table">
    /// <listheader>
    /// <term>Interface</term>
    /// <description>Description</description>
    /// </listheader>
    /// <item>
    /// <term><see cref="IMessageJournalingServiceProvider"/></term>
    /// <description>Indicates that the decorated type provides a
    /// <see cref="IMessageJournalingService"/>.  The <see cref="Name"/> 
    /// specified in <see cref="ProviderAttribute"/> corresponds to the value of 
    /// the <see cref="JournalingElement.Provider"/> property in a
    /// <see cref="JournalingElement"/>.</description>
    /// </item>
    /// <item>
    /// <term><see cref="IMessageQueueingServiceProvider"/></term>
    /// <description>Indicates that the decorated type provides a
    /// <see cref="IMessageQueueingService"/>.  The <see cref="Name"/> 
    /// specified in <see cref="ProviderAttribute"/> corresponds to the value of 
    /// the <see cref="QueueingElement.Provider"/> property in a
    /// <see cref="QueueingElement"/>.</description>
    /// </item>
    /// <item>
    /// <term><see cref="ISubscriptionTrackingServiceProvider"/></term>
    /// <description>Indicates that the decorated type provides a
    /// <see cref="ISubscriptionTrackingService"/>.  The <see cref="Name"/> 
    /// specified in <see cref="ProviderAttribute"/> corresponds to the value of 
    /// the <see cref="QueueingElement.Provider"/> property in a
    /// <see cref="QueueingElement"/>.</description>
    /// </item>
    /// <item>
    /// <term><see cref="ISQLDialectProvider"/></term>
    /// <description>Indicates that the decorated type provides a
    /// <see cref="ISQLDialect"/>.  The <see cref="Name"/> 
    /// specified in <see cref="ProviderAttribute"/> corresponds to the name of
    /// the ADO.NET provider specified in the connection string settings.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ProviderAttribute : Attribute
    {
        private readonly string _name;

        /// <summary>
        /// The provider name
        /// </summary>
        /// <remarks>
        /// The provider name is the value specified in configuration files
        /// to indicate that the decorated type should be used to provide
        /// service implementations during initialization.
        /// </remarks>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// The relative priority of the provider
        /// </summary>
        /// <remarks>
        /// In the case in which there are multiple providers of the same
        /// name, the priority is used to indicate which provider should be
        /// preferred.  Higher values take precedence.  This is useful when
        /// overriding default provider implementations.
        /// </remarks>
        public int Priority { get; set; }

        /// <summary>
        /// Initializes a new <see cref="ProviderAttribute"/> with the
        /// specified <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The provider name to associate with the
        /// decorated type.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/>
        /// is <c>null</c> or whitespace.</exception>
        public ProviderAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");
            _name = name;
        }
    }
}