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

namespace Platibus.Config.Extensibility
{
    /// <inheritdoc />
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
    /// <term><see cref="T:Platibus.Config.Extensibility.IMessageJournalProvider" /></term>
    /// <description>Indicates that the decorated type provides a
    /// <see cref="T:Platibus.Journaling.IMessageJournal" />.  The <see cref="P:Platibus.Config.Extensibility.ProviderAttribute.Name" /> 
    /// specified in <see cref="T:Platibus.Config.Extensibility.ProviderAttribute" /> corresponds to the value of 
    /// the <c>provider</c> property in a journaling configuration section
    /// </description>
    /// </item>
    /// <item>
    /// <term><see cref="T:Platibus.Config.Extensibility.IMessageQueueingServiceProvider" /></term>
    /// <description>Indicates that the decorated type provides a
    /// <see cref="T:Platibus.IMessageQueueingService" />.  The <see cref="P:Platibus.Config.Extensibility.ProviderAttribute.Name" /> 
    /// specified in <see cref="T:Platibus.Config.Extensibility.ProviderAttribute" /> corresponds to the value of 
    /// the <c>provider</c> property in a queueing configuration section
    /// </description>
    /// </item>
    /// <item>
    /// <term><see cref="T:Platibus.Config.Extensibility.ISecurityTokenServiceProvider" /></term>
    /// <description>Indicates that the decorated type provides a
    /// <see cref="T:Platibus.Security.ISecurityTokenService" />.  The <see cref="P:Platibus.Config.Extensibility.ProviderAttribute.Name" /> 
    /// specified in <see cref="T:Platibus.Config.Extensibility.ProviderAttribute" /> corresponds to the value of 
    /// the <c>provider</c> property in a security tokens configuration section
    /// </description>
    /// </item>
    /// <item>
    /// <term><see cref="T:Platibus.Config.Extensibility.ISubscriptionTrackingServiceProvider" /></term>
    /// <description>Indicates that the decorated type provides a
    /// <see cref="T:Platibus.ISubscriptionTrackingService" />.  The <see cref="P:Platibus.Config.Extensibility.ProviderAttribute.Name" /> 
    /// specified in <see cref="T:Platibus.Config.Extensibility.ProviderAttribute" /> corresponds to the value of 
    /// the <c>provider</c> property in a subscription tracking configuration
    /// section</description>
    /// </item>
    /// <item>
    /// <term><see cref="T:Platibus.Config.Extensibility.IMessageQueueingCommandBuildersProvider" /></term>
    /// <description>Indicates that the decorated type provides a set of
    /// <see cref="T:Platibus.SQL.Commands.IMessageQueueingCommandBuilders" /> for use with the 
    /// <see cref="T:Platibus.SQL.SQLMessageQueueingService" /> and its derivatives.  The <see cref="P:Platibus.Config.Extensibility.ProviderAttribute.Name" /> 
    /// specified in <see cref="T:Platibus.Config.Extensibility.ProviderAttribute" /> corresponds to the name of
    /// the ADO.NET provider specified in the connection string settings.
    /// </description>
    /// </item>
    /// <item>
    /// <term><see cref="T:Platibus.Config.Extensibility.IMessageJournalingCommandBuildersProvider" /></term>
    /// <description>Indicates that the decorated type provides a set of
    /// <see cref="T:Platibus.SQL.Commands.IMessageJournalingCommandBuilders" /> for use with the 
    /// <see cref="T:Platibus.SQL.SQLMessageJournal" /> and its derivatives.  The <see cref="P:Platibus.Config.Extensibility.ProviderAttribute.Name" /> 
    /// specified in <see cref="T:Platibus.Config.Extensibility.ProviderAttribute" /> corresponds to the name of
    /// the ADO.NET provider specified in the connection string settings.
    /// </description>
    /// </item>
    /// <item>
    /// <term><see cref="T:Platibus.Config.Extensibility.ISubscriptionTrackingCommandBuildersProvider" /></term>
    /// <description>Indicates that the decorated type provides a set of
    /// <see cref="T:Platibus.SQL.Commands.ISubscriptionTrackingCommandBuilders" /> for use with the 
    /// <see cref="T:Platibus.SQL.SQLSubscriptionTrackingService" /> and its derivatives.  The <see cref="P:Platibus.Config.Extensibility.ProviderAttribute.Name" /> 
    /// specified in <see cref="T:Platibus.Config.Extensibility.ProviderAttribute" /> corresponds to the name of
    /// the ADO.NET provider specified in the connection string settings.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ProviderAttribute : Attribute
    {
        /// <summary>
        /// The provider name
        /// </summary>
        /// <remarks>
        /// The provider name is the value specified in configuration files
        /// to indicate that the decorated type should be used to provide
        /// service implementations during initialization.
        /// </remarks>
        public string Name { get; }

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

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.Config.Extensibility.ProviderAttribute" /> with the
        /// specified <paramref name="name" />.
        /// </summary>
        /// <param name="name">The provider name to associate with the
        /// decorated type.</param>
        /// <exception cref="T:System.ArgumentNullException">Thrown if <paramref name="name" />
        /// is <c>null</c> or whitespace.</exception>
        public ProviderAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            Name = name;
        }
    }
}