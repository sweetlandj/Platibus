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
#if NET452 || NET461
using System.Configuration;
#endif
using System.Data;
using System.Data.Common;
#if NETSTANDARD2_0
using Platibus.Config;
#endif
using Platibus.Diagnostics;

namespace Platibus.SQL
{
    /// <summary>
    /// A connection provider that creates a new connection via the ADO.NET provider factory
    /// and closes the connection when released
    /// </summary>
    public class DefaultConnectionProvider : IDbConnectionProvider
    {
        private readonly IDiagnosticService _diagnosticService;

        /// <summary>
        /// The connection string settings for this connection provider
        /// </summary>
        protected ConnectionStringSettings ConnectionStringSettings { get; }

        /// <summary>
        /// The ADO.NET provider factory
        /// </summary>
        protected DbProviderFactory ProviderFactory { get; }

        /// <summary>
        /// Initializes a new <see cref="DefaultConnectionProvider"/> with the specified
        /// <paramref name="connectionStringSettings"/>
        /// </summary>
        /// <param name="connectionStringSettings">The connection string settings for which
        /// the connection provider will provide connections</param>
        /// <param name="diagnosticService">(Optional) The service through which diagnostic events
        /// are reported and processed</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="connectionStringSettings"/>
        /// is <c>null</c></exception>
        public DefaultConnectionProvider(ConnectionStringSettings connectionStringSettings, IDiagnosticService diagnosticService = null)
        {
            ConnectionStringSettings = connectionStringSettings ?? throw new ArgumentNullException(nameof(connectionStringSettings));
            _diagnosticService = diagnosticService ?? DiagnosticService.DefaultInstance;
            ProviderFactory = DbProviderFactories.GetFactory(connectionStringSettings.ProviderName);
        }

        /// <inheritdoc />
        /// <summary>
        /// Produces a database connection
        /// </summary>
        /// <returns>A database connection</returns>
        /// <remarks>
        /// The return value of this method may be <c>null</c> depending on whether the ADO.NET
        /// provider has overridden the virtual <c>DbProviderFactory.CreateConnection</c>
        /// method.
        /// </remarks>
        public virtual DbConnection GetConnection()
        {
            var connection = ProviderFactory.CreateConnection();

            // The abstract DbProviderFactory class specifies a virtual CreateConnection method
            // whose default implementation returns null.  There is a possibility that this method
            // will not be overridden by the concrete provider factory.  If that is the case, then 
            // there is nothing we can do other than avoid a NullReferenceException by not attempting 
            // to set the connection string or open the connection.
            if (connection != null)
            {
                connection.ConnectionString = ConnectionStringSettings.ConnectionString;
                connection.Open(); 

                _diagnosticService.Emit(new SQLEventBuilder(this, SQLEventType.ConnectionOpened)
                {
                    ConnectionName = ConnectionStringSettings.Name
                }.Build());
            }
            return connection;
        }

        /// <summary>
        /// Releases a database connection
        /// </summary>
        /// <param name="connection">The connection to release</param>
        public virtual void ReleaseConnection(DbConnection connection)
        {
            if (connection != null && connection.State == ConnectionState.Open)
            {
                try
                {
                    connection.Close();
                    _diagnosticService.Emit(new SQLEventBuilder(this, SQLEventType.ConnectionClosed)
                    {
                        ConnectionName = ConnectionStringSettings.Name
                    }.Build());
                }
                catch (Exception ex)
                {
                    _diagnosticService.Emit(new SQLEventBuilder(this, SQLEventType.CommandError)
                    {
                        Detail = "Error closing connection",
                        ConnectionName = ConnectionStringSettings.Name,
                        Exception = ex
                    }.Build());
                }
            }
        }
    }
}