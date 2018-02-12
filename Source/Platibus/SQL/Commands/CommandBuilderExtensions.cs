// The MIT License (MIT)
// 
// Copyright (c) 2017 Jesse Sweetland
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
#if NET452 || NET461
using System.Configuration;
#elif NETSTANDARD2_0
using Platibus.Config;
#endif
using Platibus.Config.Extensibility;
using Platibus.Diagnostics;

namespace Platibus.SQL.Commands
{
    /// <summary>
    /// Convenience methods for determining appropriate command builders based on connection
    /// string settings and ADO.NET providers.
    /// </summary>
    [Obsolete("Use CommandBuildersFactory")]
    public static class CommandBuilderExtensions
    {
        /// <summary>
        /// Returns the message journal commands that are most appropriate for the specified 
        /// <paramref name="connectionStringSettings"/>
        /// </summary>
        /// <param name="connectionStringSettings">The connection string settings</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="connectionStringSettings"/>
        /// is <c>null</c></exception>
        /// <returns>Returns the message journal commands that are most appropriate for the 
        /// specified connection string settings</returns>
        /// <remarks>
        /// If no provider name is specified in the connection string settings, the 
        /// <see cref="MSSQLMessageQueueingCommandBuilders"/> will be returned by default.
        /// </remarks>
        /// <seealso cref="ReflectionBasedProviderService"/>
        [Obsolete("Use instance method CommandBuildersFactory.InitMessageJournalingCommandBuilders")]
        public static IMessageJournalingCommandBuilders GetMessageJournalingCommandBuilders(this ConnectionStringSettings connectionStringSettings)
        {
            if (connectionStringSettings == null) throw new ArgumentNullException(nameof(connectionStringSettings));
            return new CommandBuildersFactory(connectionStringSettings, new DiagnosticService()).InitMessageJournalingCommandBuilders();
        }

        /// <summary>
        /// Returns the message queueing service  commands that are most appropriate for the 
        /// specified <paramref name="connectionStringSettings"/>
        /// </summary>
        /// <param name="connectionStringSettings">The connection string settings</param>
        /// <exception cref="ArgumentNullException">Thrown if
        /// <paramref name="connectionStringSettings"/> is <c>null</c></exception>
        /// <returns>Returns the message queueing service  commands that are most appropriate for 
        /// the specified connection string settings</returns>
        /// <remarks>
        /// If no provider name is specified in the connection string settings, the 
        /// <see cref="MSSQLMessageQueueingCommandBuilders"/> will be returned by default.
        /// </remarks>
        /// <seealso cref="ReflectionBasedProviderService"/>
        [Obsolete("Use instance method CommandBuildersFactory.InitMessageQueueingCommandBuilders")]
        public static IMessageQueueingCommandBuilders GetMessageQueueingCommandBuilders(this ConnectionStringSettings connectionStringSettings)
        {
            if (connectionStringSettings == null) throw new ArgumentNullException(nameof(connectionStringSettings));
            return new CommandBuildersFactory(connectionStringSettings, new DiagnosticService()).InitMessageQueueingCommandBuilders();
        }

        /// <summary>
        /// Returns the message queueing service  commands that are most appropriate for the 
        /// specified <paramref name="connectionStringSettings"/>
        /// </summary>
        /// <param name="connectionStringSettings">The connection string settings</param>
        /// <exception cref="ArgumentNullException">Thrown if
        /// <paramref name="connectionStringSettings"/> is <c>null</c></exception>
        /// <returns>Returns the message queueing service  commands that are most appropriate for 
        /// the specified connection string settings</returns>
        /// <remarks>
        /// If no provider name is specified in the connection string settings, the 
        /// <see cref="MSSQLSubscriptionTrackingCommandBuilders"/> will be returned by default.
        /// </remarks>
        /// <seealso cref="ReflectionBasedProviderService"/>
        [Obsolete("Use instance method CommandBuildersFactory.InitSubscriptionTrackingCommandBuilders")]
        public static ISubscriptionTrackingCommandBuilders GetSubscriptionTrackingCommandBuilders(this ConnectionStringSettings connectionStringSettings)
        {
            if (connectionStringSettings == null) throw new ArgumentNullException(nameof(connectionStringSettings));
            return new CommandBuildersFactory(connectionStringSettings, new DiagnosticService()).InitSubscriptionTrackingCommandBuilders();
        }
    }
}
