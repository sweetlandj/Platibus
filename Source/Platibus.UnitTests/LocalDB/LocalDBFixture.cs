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

using Platibus.SQL;
using Platibus.SQL.Commands;
#if NET452
using System.Configuration;
#endif
#if NETCOREAPP2_0
using Platibus.Config;
#endif

namespace Platibus.UnitTests.LocalDB
{
    public class LocalDBFixture : IDisposable
    {
        private bool _disposed;
        
        public IDbConnectionProvider ConnectionProvider { get; }

        public SQLMessageJournal MessageJournal { get; }

        public SQLMessageQueueingService MessageQueueingService { get; }

        public SQLSubscriptionTrackingService SubscriptionTrackingService { get; }

        public LocalDBFixture()
        {
            var connectionStringSettings = ConfigurationManager.ConnectionStrings["PlatibusUnitTests.LocalDB"];
            ConnectionProvider = new DefaultConnectionProvider(connectionStringSettings);
            
            MessageJournal = new SQLMessageJournal(ConnectionProvider, new MSSQLMessageJournalingCommandBuilders());
            MessageJournal.Init();

            MessageQueueingService = new SQLMessageQueueingService(ConnectionProvider, new MSSQLMessageQueueingCommandBuilders());
            MessageQueueingService.Init();

            SubscriptionTrackingService = new SQLSubscriptionTrackingService(ConnectionProvider, new MSSQLSubscriptionTrackingCommandBuilders());
            SubscriptionTrackingService.Init();

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
            MessageQueueingService.Dispose();
            SubscriptionTrackingService.Dispose();
        }

        public void DeleteQueuedMessages()
        {
            using (var connection = ConnectionProvider.GetConnection())
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
            using (var connection = ConnectionProvider.GetConnection())
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
            using (var connection = ConnectionProvider.GetConnection())
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
