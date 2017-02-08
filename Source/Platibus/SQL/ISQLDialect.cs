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

        /// <summary>
        /// The dialect-specific command used to create the objects (tables, indexes,
        /// stored procedures, views, etc.) needed to store journaled messages in the 
        /// SQL database
        /// </summary>
        string CreateMessageJournalingServiceObjectsCommand { get; }

        /// <summary>
        /// The dialect-specific command used to insert a queued message
        /// </summary>
        string InsertJournaledMessageCommand { get; }

        /// <summary>
        /// The name of the parameter used to specify a category
        /// </summary>
        string CategoryParameterName { get; }

        /// <summary>
        /// The dialect-specific command used to select the list of journaled messages
        /// in a particular queue
        /// </summary>
        string SelectJournaledMessagesCommand { get; }

        /// <summary>
        /// The name of the parameter used to specify a timestamp
        /// </summary>
        string TimestampParameterName { get; }
    }


}