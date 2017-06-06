using System;
using System.Data;
using System.Data.Common;

namespace Platibus.SQL.Commands
{
    /// <summary>
    /// Default command builder for creating commands to delete existing queued messages from the
    /// database (i.e. in response to acknowledgement).
    /// </summary>
    public class DeleteQueuedMessageCommandBuilder
    {
        /// <summary>
        /// The message ID
        /// </summary>
        public Guid MessageId { get; set; }

        /// <summary>
        /// The name of the message queue
        /// </summary>
        public string QueueName { get; set; }
        
        /// <summary>
        /// Initializes and returns a new non-query <see cref="DbCommand"/> with the configured
        /// parameters
        /// </summary>
        /// <param name="connection">An open database connection</param>
        /// <returns>Returns a new non-query <see cref="DbCommand"/> with the configured
        /// parameters</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public virtual DbCommand BuildDbCommand(DbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = CommandText;

            command.SetParameter("@MessageId", MessageId);
            command.SetParameter("@QueueName", QueueName);

            return command;
        }

        /// <summary>
        /// The default command text (Transact-SQL syntax)
        /// </summary>
        public virtual string CommandText
        {
            get
            {
                return @"
DELETE FROM [PB_QueuedMessages]
WHERE [MessageId]=@MessageId 
AND [QueueName]=@QueueName";
            }
        }
    }
}