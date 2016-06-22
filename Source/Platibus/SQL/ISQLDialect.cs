namespace Platibus.SQL
{
    /// <summary>
    /// An interface describing the dialect-specific commands and parameters 
    /// that are required for SQL-based services such as 
    /// <see cref="SQLMessageQueueingService"/> and 
    /// <see cref="SQLSubscriptionTrackingService"/>
    /// </summary>
    /// <remarks>
    /// A dialect is selected via the <see cref="ISQLDialectProvider"/>.  A
    /// typical implementation will select a dialect based on the name of the
    /// ADO.NET provider specified in the connection string settings.
    /// </remarks>
    /// <see cref="CommonSQLDialect"/>
    /// <see cref="MSSQLDialect"/>
    /// <see cref="ISQLDialect"/>
    /// <see cref="MSSQLDialectProvider"/>
    public interface ISQLDialect
    {
        /// <summary>
        /// The dialect-specific command used to create the objects (tables, indexes,
        /// stored procedures, views, etc.) needed to store queued messages in the 
        /// SQL database
        /// </summary>
        string CreateMessageQueueingServiceObjectsCommand { get; }

        /// <summary>
        /// The dialect-specific command used to create the objects (tables, indexes,
        /// stored procedures, views, etc.) needed to store subscription tracking data 
        /// in the SQL database
        /// </summary>
        string CreateSubscriptionTrackingServiceObjectsCommand { get; }

        /// <summary>
        /// The dialect-specific command used to insert a queued message
        /// </summary>
        string InsertQueuedMessageCommand { get; }

        /// <summary>
        /// The dialect-specific command used to select the list of queued messages
        /// in a particular queue
        /// </summary>
        string SelectQueuedMessagesCommand { get; }

        /// <summary>
        /// The dialect-specific command used to select the list of abandoned messages
        /// in a particular queue
        /// </summary>
        string SelectAbandonedMessagesCommand { get; }

        /// <summary>
        /// The dialect-specific command used to update the state of a queued message
        /// </summary>
        string UpdateQueuedMessageCommand { get; }

        /// <summary>
        /// The dialect-specific command used to insert new subscriptions
        /// </summary>
        string InsertSubscriptionCommand { get; }

        /// <summary>
        /// The dialect-specific command used to update existing subscriptions
        /// </summary>
        string UpdateSubscriptionCommand { get; }

        /// <summary>
        /// The dialect-specific command used to select subscriptions
        /// </summary>
        string SelectSubscriptionsCommand { get; }

        /// <summary>
        /// The dialect-specific command used to delete a subscription
        /// </summary>
        string DeleteSubscriptionCommand { get; }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the name of
        /// a queue when inserting messages
        /// </summary>
        string QueueNameParameterName { get; }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the message ID value
        /// when inserting, updating, or deleting queued messages
        /// </summary>
        string MessageIdParameterName { get; }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the message name value
        /// when inserting queued messages
        /// </summary>
        string MessageNameParameterName { get; }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the originating URI value
        /// when inserting queued messages
        /// </summary>
        string OriginationParameterName { get; }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the destination URI value
        /// when inserting queued messages
        /// </summary>
        string DestinationParameterName { get; }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the reply-to URI value
        /// when inserting queued messages; inserting subscriptions; or updating subscriptions
        /// </summary>
        string ReplyToParameterName { get; }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the message expiration
        /// date when inserting queued messages
        /// </summary>
        string ExpiresParameterName { get; }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the content type value
        /// when inserting queued messages
        /// </summary>
        string ContentTypeParameterName { get; }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the entire serialized headers
        /// collection when inserting queued messages
        /// </summary>
        string HeadersParameterName { get; }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the sender principal
        /// when inserting queued messages
        /// </summary>
        string SenderPrincipalParameterName { get; }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the message content when
        /// inserting queued messages
        /// </summary>
        string MessageContentParameterName { get; }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the number of attempts when
        /// updating queued messages
        /// </summary>
        string AttemptsParameterName { get; }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the date and time the message
        /// was acknowledged when updating queued messages
        /// </summary>
        string AcknowledgedParameterName { get; }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the date and time the message
        /// was abandoned when updating queued messages
        /// </summary>
        string AbandonedParameterName { get; }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the name of the topic being
        /// subscribed to when inserting subscriptions
        /// </summary>
        string TopicNameParameterName { get; }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the URI of the subscriber
        /// when inserting subscriptions
        /// </summary>
        string SubscriberParameterName { get; }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the current date and time
        /// on the server when selecting active subscriptions
        /// </summary>
        string CurrentDateParameterName { get; }

        /// <summary>
        /// The name of the parameter used to specify the start date in queries based on date ranges
        /// </summary>
        string StartDateParameterName { get; }

        /// <summary>
        /// The name of the parameter used to specify the end date in queries based on date ranges
        /// </summary>
        string EndDateParameterName { get; }
    }


}