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
#if NET452
using System.Configuration;
#endif
using Platibus.Diagnostics;
using Platibus.SQL;
using Platibus.SQL.Commands;

namespace Platibus.Config.Extensibility
{
    /// <summary>
    /// Factory class for initializing SQL command builders
    /// </summary>
    public class CommandBuildersFactory
    {
        private readonly ConnectionStringSettings _connectionStringSettings;
        private readonly IDiagnosticService _diagnosticService;

        /// <summary>
        /// Initializes a new <see cref="CommandBuildersFactory"/>
        /// </summary>
        /// <param name="connectionStringSettings">The connection string settings for the
        /// connections that will be used to create and execute the commands</param>
        /// <param name="diagnosticService">(Optional) The service through which diagnostic events
        /// are reported and processed</param>
        public CommandBuildersFactory(ConnectionStringSettings connectionStringSettings, IDiagnosticService diagnosticService = null)
        {
            _connectionStringSettings = connectionStringSettings ?? throw new ArgumentNullException(nameof(connectionStringSettings));
            _diagnosticService = diagnosticService ?? DiagnosticService.DefaultInstance;
        }

        /// <summary>
        /// Initializes <see cref="IMessageJournalingCommandBuilders"/>
        /// </summary>
        /// <returns>Returns initialized <see cref="IMessageJournalingCommandBuilders"/></returns>
        public IMessageJournalingCommandBuilders InitMessageJournalingCommandBuilders()
        {
            var providerName = _connectionStringSettings.ProviderName;
            IMessageJournalingCommandBuildersProvider provider;
            if (string.IsNullOrWhiteSpace(providerName))
            {
                _diagnosticService.Emit(new SQLEventBuilder(this, DiagnosticEventType.ConfigurationDefault)
                {
                    Detail = "No provider name specified; using default provider (MSSQL)",
                    ConnectionName = _connectionStringSettings.Name,
                    
                }.Build());
                provider = new MSSQLCommandBuildersProvider();
            }
            else
            {
                provider = ProviderHelper.GetProvider<IMessageJournalingCommandBuildersProvider>(providerName);
            }
            return provider.GetMessageJournalingCommandBuilders(_connectionStringSettings);
        }

        /// <summary>
        /// Initializes <see cref="IMessageQueueingCommandBuilders"/>
        /// </summary>
        /// <returns>Returns initialized <see cref="IMessageQueueingCommandBuilders"/></returns>
        public IMessageQueueingCommandBuilders InitMessageQueueingCommandBuilders()
        {
            var providerName = _connectionStringSettings.ProviderName;
            IMessageQueueingCommandBuildersProvider provider;
            if (string.IsNullOrWhiteSpace(providerName))
            {
                _diagnosticService.Emit(new SQLEventBuilder(this, DiagnosticEventType.ConfigurationDefault)
                {
                    Detail = "No provider name specified; using default provider (MSSQL)",
                    ConnectionName = _connectionStringSettings.Name,

                }.Build());
                provider = new MSSQLCommandBuildersProvider();
            }
            else
            {
                provider = ProviderHelper.GetProvider<IMessageQueueingCommandBuildersProvider>(providerName);
            }
            return provider.GetMessageQueueingCommandBuilders(_connectionStringSettings);
        }

        /// <summary>
        /// Initializes <see cref="ISubscriptionTrackingCommandBuilders"/>
        /// </summary>
        /// <returns>Returns initialized <see cref="ISubscriptionTrackingCommandBuilders"/></returns>
        public ISubscriptionTrackingCommandBuilders InitSubscriptionTrackingCommandBuilders()
        {
            var providerName = _connectionStringSettings.ProviderName;
            ISubscriptionTrackingCommandBuildersProvider provider;
            if (string.IsNullOrWhiteSpace(providerName))
            {
                _diagnosticService.Emit(new SQLEventBuilder(this, DiagnosticEventType.ConfigurationDefault)
                {
                    Detail = "No provider name specified; using default provider (MSSQL)",
                    ConnectionName = _connectionStringSettings.Name,

                }.Build());
                provider = new MSSQLCommandBuildersProvider();
            }
            else
            {
                provider = ProviderHelper.GetProvider<ISubscriptionTrackingCommandBuildersProvider>(providerName);
            }
            return provider.GetSubscriptionTrackingCommandBuilders(_connectionStringSettings);
        }
    }
}
