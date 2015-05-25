using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using System.Transactions;
using Common.Logging;
using Platibus.SampleWebApp.Models;

namespace Platibus.SampleWebApp.Controllers
{
    public class ReceivedMessageRepository
    {
        private static readonly ILog Log = LogManager.GetLogger(SampleWebAppLoggingCategories.SampleWebApp);

        public Task Init()
        {
            return ExecuteUpdate(connection =>
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        IF (OBJECT_ID('ReceivedMessages') IS NULL)
                        CREATE TABLE ReceivedMessages (
                            MessageId VARCHAR(100) NOT NULL PRIMARY KEY,                      
                            SenderPrincipal VARCHAR(100) NOT NULL,
                            MessageName VARCHAR(500) NOT NULL,
                            Origination VARCHAR(500) NOT NULL,
                            Destination VARCHAR(500) NOT NULL,
                            RelatedTo VARCHAR(500) NOT NULL,
                            Sent DATETIME NOT NULL,
                            Received DATETIME NOT NULL,
                            Expires DATETIME,
                            ContentType VARCHAR(100) NOT NULL,
                            Content TEXT
                        )";

                    return command.ExecuteNonQueryAsync();
                }
            });
        }

        public async Task<IEnumerable<ReceivedMessage>> GetMessages()
        {
            Log.Debug("Retrieving received messages...");
            return await ExecuteQuery(async connection =>
            {
                var receivedMessages = new List<ReceivedMessage>();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = SelectReceivedMessages;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            receivedMessages.Add(ReadReceivedMessage(reader));
                        }
                        return receivedMessages;
                    }
                }
            });
        }

        public async Task<ReceivedMessage> Get(string messageId)
        {
            Log.Debug("Retrieving received messages...");
            return await ExecuteQuery(async connection =>
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = SelectReceivedMessages + @" WHERE MessageId=@MessageId";
                    SetParameter(command, "@MessageId", messageId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return ReadReceivedMessage(reader);
                        }
                        return null;
                    }
                }
            });
        }

        private const string SelectReceivedMessages = @"
            SELECT
                SenderPrincipal,
                MessageId,
                MessageName,
                Origination,
                Destination,
                RelatedTo,
                Sent,
                Received,
                Expires,
                ContentType,
                Content 
            FROM ReceivedMessages";

        private static ReceivedMessage ReadReceivedMessage(IDataRecord reader)
        {
            return new ReceivedMessage
            {
                SenderPrincipal = reader.GetString(reader.GetOrdinal("SenderPrincipal")),
                MessageId = reader.GetString(reader.GetOrdinal("MessageId")),
                MessageName = reader.GetString(reader.GetOrdinal("MessageName")),
                Origination = reader.GetString(reader.GetOrdinal("Origination")),
                Destination = reader.GetString(reader.GetOrdinal("Destination")),
                RelatedTo = reader.GetString(reader.GetOrdinal("RelatedTo")),
                Sent = reader.GetDateTime(reader.GetOrdinal("Sent")),
                Received = reader.GetDateTime(reader.GetOrdinal("Received")),
                Expires = reader.IsDBNull(reader.GetOrdinal("Expires"))
                    ? null
                    : (DateTime?) reader.GetDateTime(reader.GetOrdinal("Expires")),
                ContentType = reader.GetString(reader.GetOrdinal("ContentType")),
                Content = reader.GetString(reader.GetOrdinal("Content"))
            };
        }

        public Task Add(ReceivedMessage message)
        {
            Log.DebugFormat("Adding message ID {0} to received message repository...", message.MessageId);
            return ExecuteUpdate(connection =>
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        INSERT INTO ReceivedMessages (
                            SenderPrincipal,
                            MessageId,
                            MessageName,
                            Origination,
                            Destination,
                            RelatedTo,
                            Sent,
                            Received,
                            Expires,
                            ContentType,
                            Content
                        ) VALUES (
                            @SenderPrincipal,
                            @MessageId,
                            @MessageName,
                            @Origination,
                            @Destination,
                            @RelatedTo,
                            @Sent,
                            @Received,
                            @Expires,
                            @ContentType,
                            @Content
                        )";

                    SetParameter(command, "@SenderPrincipal", message.SenderPrincipal);
                    SetParameter(command, "@MessageId", message.MessageId);
                    SetParameter(command, "@MessageName", message.MessageName);
                    SetParameter(command, "@Origination", message.Origination);
                    SetParameter(command, "@Destination", message.Destination);
                    SetParameter(command, "@RelatedTo", message.RelatedTo);
                    SetParameter(command, "@Sent", message.Sent);
                    SetParameter(command, "@Received", message.Received);
                    SetParameter(command, "@Expires", message.Expires);
                    SetParameter(command, "@ContentType", message.ContentType);
                    SetParameter(command, "@Content", message.Content);

                    return command.ExecuteNonQueryAsync();
                }
            });
        }

        public Task Remove(string messageId)
        {
            Log.DebugFormat("Removing message ID {0} from received message repository...", messageId);
            return ExecuteUpdate(connection =>
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"DELETE FROM ReceivedMessages WHERE MessageId=@MessageId";
                    SetParameter(command, "@MessageId", messageId);
                    return command.ExecuteNonQueryAsync();
                }
            });
        }

        public async Task RemoveAll()
        {
            Log.Debug("Removing all messages from received message repository...");
            await ExecuteUpdate(connection =>
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"DELETE FROM ReceivedMessages";
                    return command.ExecuteNonQueryAsync();
                }
            });
        }

        private static async Task<TResult> ExecuteQuery<TResult>(Func<DbConnection, Task<TResult>> query)
        {
            TResult result;
            using (
                var scope = new TransactionScope(TransactionScopeOption.Suppress,
                    TransactionScopeAsyncFlowOption.Enabled))
            {
                using (var connection = await CreateConnection())
                {
                    result = await query(connection);
                }
                scope.Complete();
            }
            return result;
        }

        private static async Task<TResult> ExecuteUpdate<TResult>(Func<DbConnection, Task<TResult>> update)
        {
            TResult result;
            using (
                var scope = new TransactionScope(TransactionScopeOption.Required,
                    TransactionScopeAsyncFlowOption.Enabled))
            {
                using (var connection = await CreateConnection())
                {
                    result = await update(connection);
                }
                scope.Complete();
            }
            return result;
        }

        private static async Task<DbConnection> CreateConnection()
        {
            var connetionStringSettings = ConfigurationManager.ConnectionStrings["Platibus"];
            var dbProviderFactory = DbProviderFactories.GetFactory(connetionStringSettings.ProviderName);
            var connection = dbProviderFactory.CreateConnection();
            if (connection == null) throw new Exception("Database provider returned null connection");
            connection.ConnectionString = connetionStringSettings.ConnectionString;
            await connection.OpenAsync();
            return connection;
        }

        private static void SetParameter(DbCommand command, string parameterName, object parameterValue)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.Value = parameterValue ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }
    }
}