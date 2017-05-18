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
using System.Configuration;
using Common.Logging;
using Platibus.Config.Extensibility;

namespace Platibus.SQL.Commands
{
    /// <summary>
    /// Convenience methods for determining appropriate command builders based on connection
    /// string settings and ADO.NET providers.
    /// </summary>
    public static class CommandBuilderExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.SQL);

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
        /// <seealso cref="ProviderHelper"/>
        public static IMessageJournalingCommandBuilders GetMessageJournalingCommandBuilders(this ConnectionStringSettings connectionStringSettings)
        {
            if (connectionStringSettings == null) throw new ArgumentNullException("connectionStringSettings");
            var providerName = connectionStringSettings.ProviderName;
            IMessageJournalingCommandBuildersProvider provider;
            if (string.IsNullOrWhiteSpace(providerName))
            {
                Log.Debug("No connection string provider specified; using default provider...");
                provider = new MSSQLCommandBuildersProvider();
            }
            else
            {
                provider = ProviderHelper.GetProvider<IMessageJournalingCommandBuildersProvider>(providerName);
            }

            Log.Debug("Getting message journal commands...");
            return provider.GetMessageJournalingCommandBuilders(connectionStringSettings);
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
        /// <seealso cref="ProviderHelper"/>
        public static IMessageQueueingCommandBuilders GetMessageQueueingCommandBuilders(this ConnectionStringSettings connectionStringSettings)
        {
            if (connectionStringSettings == null) throw new ArgumentNullException("connectionStringSettings");
            var providerName = connectionStringSettings.ProviderName;
            IMessageQueueingCommandBuildersProvider provider;
            if (string.IsNullOrWhiteSpace(providerName))
            {
                Log.Debug("No connection string provider specified; using default provider...");
                provider = new MSSQLCommandBuildersProvider();
            }
            else
            {
                provider = ProviderHelper.GetProvider<IMessageQueueingCommandBuildersProvider>(providerName);
            }

            Log.Debug("Getting message queueing service  commands...");
            return provider.GetMessageQueueingCommandBuilders(connectionStringSettings);
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
        /// <seealso cref="ProviderHelper"/>
        public static ISubscriptionTrackingCommandBuilders GetSubscriptionTrackingCommandBuilders(this ConnectionStringSettings connectionStringSettings)
        {
            if (connectionStringSettings == null) throw new ArgumentNullException("connectionStringSettings");
            var providerName = connectionStringSettings.ProviderName;
            ISubscriptionTrackingCommandBuildersProvider provider;
            if (string.IsNullOrWhiteSpace(providerName))
            {
                Log.Debug("No connection string provider specified; using default provider...");
                provider = new MSSQLCommandBuildersProvider();
            }
            else
            {
                provider = ProviderHelper.GetProvider<ISubscriptionTrackingCommandBuildersProvider>(providerName);
            }

            Log.Debug("Getting message queueing service  commands...");
            return provider.GetSubscriptionTrackingCommandBuilders(connectionStringSettings);
        }
    }
}
