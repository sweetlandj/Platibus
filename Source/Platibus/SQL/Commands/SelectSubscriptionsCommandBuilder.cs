using System;
using System.Data;
using System.Data.Common;

namespace Platibus.SQL.Commands
{
    /// <summary>
    /// Default command builder for creating commands to select topic subscription info from the
    /// database.
    /// </summary>
    public class SelectSubscriptionsCommandBuilder
    {
        /// <summary>
        /// The cutoff date for subscriptions (usually current date/time)
        /// </summary>
        /// <remarks>
        /// This is compared to subscription expiration dates
        /// </remarks>
        public DateTime CutoffDate { get; set; }

        /// <summary>
        /// Initializes a new <see cref="CreateSubscriptionTrackingObjectsCommandBuilder"/> with
        /// default parameter values.
        /// </summary>
        public SelectSubscriptionsCommandBuilder()
        {
            CutoffDate = DateTime.UtcNow;
        }

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
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = CommandText;

            command.SetParameter("@CutoffDate", CutoffDate);

            return command;
        }

        /// <summary>
        /// The default command text (Transact-SQL syntax)
        /// </summary>
        public virtual string CommandText => @"
SELECT [TopicName], [Subscriber], [Expires]
FROM [PB_Subscriptions]
WHERE [Expires] IS NULL
OR [Expires] > @CutoffDate";
    }
}