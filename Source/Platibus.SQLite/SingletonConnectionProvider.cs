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
using System.Configuration;
using System.Data;
using System.Data.Common;
using Common.Logging;
using Platibus.SQL;

namespace Platibus.SQLite
{
    internal class SingletonConnectionProvider : IDbConnectionProvider
    {
        private static readonly ILog Log = LogManager.GetLogger(SQLiteLoggingCategories.SQLite);
        private readonly object _syncRoot = new object();
        private readonly DbProviderFactory _dbProviderFactory;
        private readonly string _connectionString;

        private volatile DbConnection _connection;
        private bool _disposed;

        public SingletonConnectionProvider(ConnectionStringSettings connectionStringSettings)
        {
            if (connectionStringSettings == null) throw new ArgumentNullException("connectionStringSettings");
            _dbProviderFactory = DbProviderFactories.GetFactory(connectionStringSettings.ProviderName);
            _connectionString = connectionStringSettings.ConnectionString;
        }


        public DbConnection GetConnection()
        {
            CheckDisposed();
            var myConnection = _connection;
            if (myConnection != null && myConnection.State == ConnectionState.Open)
            {
                return myConnection;
            }

            lock (_syncRoot)
            {
                if (myConnection != null && myConnection.State == ConnectionState.Broken)
                {
                    try
                    {
                        myConnection.Close();
                    }
                    catch (Exception ex)
                    {
                        Log.Warn("Error closing connection", ex);
                    }
                    myConnection = null;
                }

                if (myConnection == null)
                {
                    myConnection = _dbProviderFactory.CreateConnection();
                    myConnection.ConnectionString = _connectionString;
                }

                if (myConnection.State == ConnectionState.Closed)
                {
                    myConnection.Open();
                }
                _connection = myConnection;
            }
            return myConnection;
        }

        public void ReleaseConnection(DbConnection connection)
        {
        }

        protected void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }

        ~SingletonConnectionProvider()
        {
            if (_disposed) return;
            Dispose(false);
        }

        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    _connection.Close();
                }
                catch (Exception ex)
                {
                    Log.Warn("Error closing singleton connection", ex);
                }
            }
        }
    }
}