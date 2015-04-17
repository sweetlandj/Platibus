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
using Common.Logging;
using Platibus.SQL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platibus.SQLite
{
    class SingletonConnectionProvider : IDbConnectionProvider
    {
        private static readonly ILog Log = LogManager.GetLogger(SQLiteLoggingCategories.SQLite);
        private readonly object _syncRoot = new object();
        private readonly DbConnection _connection;

        private bool _disposed;

        public SingletonConnectionProvider(ConnectionStringSettings connectionStringSettings)
        {
            if (connectionStringSettings == null) throw new ArgumentNullException("connectionStringSettings");
            _connection = connectionStringSettings.OpenConnection();
        }

        private bool IsConnectionOk
        {
            get
            {
                return _connection.State != ConnectionState.Broken
                    && _connection.State != ConnectionState.Closed;
            }
        }

        private void Reconnect()
        {
            try
            {
                _connection.Close();
            }
            catch(Exception ex)
            {
                Log.Info("Error closing connection", ex);
            }
            _connection.Open();
        }

        public DbConnection GetConnection()
        {
            CheckDisposed();
            if (IsConnectionOk)
            {
                return _connection;
            }
         
            lock(_connection)
            {
                if (!IsConnectionOk)
                {
                    Log.Info("Connection in closed or broken state; reconnecting...");
                    Reconnect();
                }
            }
            return _connection;
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
            Dispose(false);
        }

        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            GC.SuppressFinalize(true);
            _disposed = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            try
            {
                _connection.Close();
            }
            catch(Exception ex)
            {
                Log.Warn("Error closing singleton connection", ex);
            }
        }
    }
}
