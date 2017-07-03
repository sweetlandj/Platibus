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
using Platibus.SQL;
using Platibus.SQL.Commands;

namespace Platibus.UnitTests.LocalDB
{
    public class LocalDBFixture : IDisposable
    {
        private readonly IDbConnectionProvider _connectionProvider;
        private readonly SQLMessageQueueingService _messageQueueingService;
        private readonly SQLSubscriptionTrackingService _subscriptionTrackingService;
        private readonly SQLMessageJournal _messageJournal;

        private bool _disposed;
        
        public IDbConnectionProvider ConnectionProvider
        {
            get { return _connectionProvider; }
        }
       
        public SQLMessageJournal MessageJournal
        {
            get { return _messageJournal; }
        }

        public SQLMessageQueueingService MessageQueueingService
        {
            get { return _messageQueueingService; }
        }

        public SQLSubscriptionTrackingService SubscriptionTrackingService
        {
            get { return _subscriptionTrackingService; }
        }

        public LocalDBFixture()
        {
            var connectionStringSettings = ConfigurationManager.ConnectionStrings["PlatibusUnitTests.LocalDB"];
            _connectionProvider = new DefaultConnectionProvider(connectionStringSettings);
            
            _messageJournal = new SQLMessageJournal(_connectionProvider, new MSSQLMessageJournalingCommandBuilders());
            _messageJournal.Init();

            _messageQueueingService = new SQLMessageQueueingService(_connectionProvider, new MSSQLMessageQueueingCommandBuilders());
            _messageQueueingService.Init();

            _subscriptionTrackingService = new SQLSubscriptionTrackingService(_connectionProvider, new MSSQLSubscriptionTrackingCommandBuilders());
            _subscriptionTrackingService.Init();

            DeleteJournaledMessages();
            DeleteQueuedMessages();
            DeleteSubscriptions();
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
            _messageQueueingService.Dispose();
            _subscriptionTrackingService.Dispose();
        }

        public void DeleteQueuedMessages()
        {
            using (var connection = _connectionProvider.GetConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    IF (OBJECT_ID('[PB_QueuedMessages]')) IS NOT NULL 
                    BEGIN
                        DELETE FROM [PB_QueuedMessages]
                    END";

                command.ExecuteNonQuery();
            }
        }

        public void DeleteJournaledMessages()
        {
            using (var connection = _connectionProvider.GetConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    IF (OBJECT_ID('[PB_MessageJournal]')) IS NOT NULL 
                    BEGIN
                        DELETE FROM [PB_MessageJournal]
                    END";

                command.ExecuteNonQuery();
            }
        }

        public void DeleteSubscriptions()
        {
            using (var connection = _connectionProvider.GetConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    IF (OBJECT_ID('[PB_Subscriptions]')) IS NOT NULL 
                    BEGIN
                        DELETE FROM [PB_Subscriptions]
                    END";

                command.ExecuteNonQuery();
            }
        }
    }
}
