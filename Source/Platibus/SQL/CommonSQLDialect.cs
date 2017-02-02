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
    /// An abstract base class containing standard SQL commands and parameter names
    /// </summary>
    /// <remarks>
    /// The parameter names specified in this base class are formatting according to
    /// the MS SQL Server convention of @<i>parameter</i>, which works for both
    /// MS SQL server and SQLite.  This base class may not be suitable for ADO.NET 
    /// providers that use different conventions for parameter naming.
    /// </remarks>
    public abstract class CommonSQLDialect : ISQLDialect
    {
        /// <summary>
        /// The dialect-specific command used to create the objects (tables, indexes,
        /// stored procedures, views, etc.) needed to store queued messages in the 
        /// SQL database
        /// </summary>
        public abstract string CreateMessageQueueingServiceObjectsCommand { get; }

        /// <summary>
        /// The dialect-specific command used to create the objects (tables, indexes,
        /// stored procedures, views, etc.) needed to store journaled messages in the 
        /// SQL database
        /// </summary>
        public abstract string CreateMessageJournalingServiceObjectsCommand { get; }

        /// <summary>
        /// The dialect-specific command used to create the objects (tables, indexes,
        /// stored procedures, views, etc.) needed to store subscription tracking data 
        /// in the SQL database
        /// </summary>
        public abstract string CreateSubscriptionTrackingServiceObjectsCommand { get; }

        /// <summary>
        /// The dialect-specific command used to insert a queued message
        /// </summary>
        public virtual string InsertQueuedMessageCommand
        {
            get { return @"
INSERT INTO [PB_QueuedMessages] (
    [MessageId], 
    [QueueName], 
    [MessageName], 
    [Origination], 
    [Destination], 
    [ReplyTo], 
    [Expires], 
    [ContentType], 
    [SenderPrincipal], 
    [Headers], 
    [MessageContent])
SELECT 
    @MessageId, 
    @QueueName, 
    @MessageName, 
    @Origination, 
    @Destination, 
    @ReplyTo, 
    @Expires, 
    @ContentType, 
    @SenderPrincipal, 
    @Headers, 
    @MessageContent
WHERE NOT EXISTS (
    SELECT [MessageID] 
    FROM [PB_QueuedMessages]
    WHERE [MessageId]=@MessageId 
    AND [QueueName]=@QueueName)"; }
        }

        /// <summary>
        /// The dialect-specific command used to insert a queued message
        /// </summary>
        public virtual string InsertJournaledMessageCommand
        {
            get { return @"
INSERT INTO [PB_MessageJournal] (
    [MessageId], 
    [Category],
    [MessageName], 
    [Origination], 
    [Destination], 
    [ReplyTo], 
    [Expires], 
    [ContentType], 
    [Headers], 
    [MessageContent])
VALUES ( 
    @MessageId, 
    @Category,
    @MessageName, 
    @Origination, 
    @Destination, 
    @ReplyTo, 
    @Expires, 
    @ContentType, 
    @Headers, 
    @MessageContent)"; }
        }

        /// <summary>
        /// The dialect-specific command used to select the list of queued messages
        /// in a particular queue
        /// </summary>
        public virtual string SelectQueuedMessagesCommand
        {
            get { return @"
SELECT 
    [MessageId], 
    [QueueName], 
    [MessageName], 
    [Origination], 
    [Destination], 
    [ReplyTo], 
    [Expires], 
    [ContentType], 
    [SenderPrincipal], 
    [Headers], 
    [MessageContent], 
    [Attempts], 
    [Acknowledged], 
    [Abandoned]
FROM [PB_QueuedMessages]
WHERE [QueueName]=@QueueName 
AND [Acknowledged] IS NULL
AND [Abandoned] IS NULL"; }
        }

        /// <summary>
        /// The dialect-specific command used to select the list of journaled messages
        /// in a particular queue
        /// </summary>
        public virtual string SelectJournaledMessagesCommand
        {
            get { return @"
SELECT 
    [MessageId], 
    [Category], 
    [MessageName], 
    [Origination], 
    [Destination], 
    [ReplyTo], 
    [Expires], 
    [ContentType], 
    [Headers], 
    [MessageContent]
FROM [PB_MessageJournal]"; }
        }

        /// <summary>
        /// The dialect-specific command used to select the list of queued messages
        /// in a particular queue
        /// </summary>
        public virtual string SelectAbandonedMessagesCommand
        {
            get { return @"
SELECT 
    [MessageId], 
    [QueueName], 
    [MessageName], 
    [Origination], 
    [Destination], 
    [ReplyTo], 
    [Expires], 
    [ContentType], 
    [SenderPrincipal], 
    [Headers], 
    [MessageContent], 
    [Attempts], 
    [Acknowledged], 
    [Abandoned]
FROM [PB_QueuedMessages]
WHERE [QueueName]=@QueueName 
AND [Acknowledged] IS NULL
AND [Abandoned] >= @StartDate
AND [Abandoned] < @EndDate"; }
        }

        /// <summary>
        /// The dialect-specific command used to update the state of a queued message
        /// </summary>
        public virtual string UpdateQueuedMessageCommand
        {
            get { return @"
UPDATE [PB_QueuedMessages] SET 
    [Acknowledged]=@Acknowledged,
    [Abandoned]=@Abandoned,
    [Attempts]=@Attempts
WHERE [MessageId]=@MessageId 
AND [QueueName]=@QueueName"; }
        }

        /// <summary>
        /// The dialect-specific command used to insert new subscriptions
        /// </summary>
        public string InsertSubscriptionCommand
        {
            get { return @"
INSERT INTO [PB_Subscriptions] ([TopicName], [Subscriber], [Expires])
SELECT @TopicName, @Subscriber, @Expires
WHERE NOT EXISTS (
    SELECT [TopicName], [Subscriber]
    FROM [PB_Subscriptions]
    WHERE [TopicName]=@TopicName
    AND [Subscriber]=@Subscriber)"; }
        }

        /// <summary>
        /// The dialect-specific command used to update existing subscriptions
        /// </summary>
        public string UpdateSubscriptionCommand
        {
            get { return @"
UPDATE [PB_Subscriptions] SET [Expires]=@Expires
WHERE [TopicName]=@TopicName
AND [Subscriber]=@Subscriber"; }
        }

        /// <summary>
        /// The dialect-specific command used to select subscriptions
        /// </summary>
        public string SelectSubscriptionsCommand
        {
            get { return @"
SELECT [TopicName], [Subscriber], [Expires]
FROM [PB_Subscriptions]
WHERE [Expires] IS NULL
OR [Expires] > @CurrentDate"; }
        }

        /// <summary>
        /// The dialect-specific command used to delete a subscription
        /// </summary>
        public string DeleteSubscriptionCommand
        {
            get { return @"
DELETE FROM [PB_Subscriptions]
WHERE [TopicName]=@TopicName
AND [Subscriber]=@Subscriber"; }
        }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the name of
        /// a queue when inserting messages
        /// </summary>
        public virtual string QueueNameParameterName
        {
            get { return "@QueueName"; }
        }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the message ID value
        /// when inserting, updating, or deleting queued messages
        /// </summary>
        public virtual string MessageIdParameterName
        {
            get { return "@MessageId"; }
        }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the message name value
        /// when inserting queued messages
        /// </summary>
        public virtual string MessageNameParameterName
        {
            get { return "@MessageName"; }
        }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the originating URI value
        /// when inserting queued messages
        /// </summary>
        public virtual string OriginationParameterName
        {
            get { return "@Origination"; }
        }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the destination URI value
        /// when inserting queued messages
        /// </summary>
        public virtual string DestinationParameterName
        {
            get { return "@Destination"; }
        }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the reply-to URI value
        /// when inserting queued messages; inserting subscriptions; or updating subscriptions
        /// </summary>
        public virtual string ReplyToParameterName
        {
            get { return "@ReplyTo"; }
        }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the message expiration
        /// date when inserting queued messages
        /// </summary>
        public virtual string ExpiresParameterName
        {
            get { return "@Expires"; }
        }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the content type value
        /// when inserting queued messages
        /// </summary>
        public virtual string ContentTypeParameterName
        {
            get { return "@ContentType"; }
        }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the entire serialized headers
        /// collection when inserting queued messages
        /// </summary>
        public virtual string HeadersParameterName
        {
            get { return "@Headers"; }
        }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the sender principal
        /// when inserting queued messages
        /// </summary>
        public virtual string SenderPrincipalParameterName
        {
            get { return "@SenderPrincipal"; }
        }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the message content when
        /// inserting queued messages
        /// </summary>
        public virtual string MessageContentParameterName
        {
            get { return "@MessageContent"; }
        }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the number of attempts when
        /// updating queued messages
        /// </summary>
        public virtual string AttemptsParameterName
        {
            get { return "@Attempts"; }
        }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the date and time the message
        /// was acknowledged when updating queued messages
        /// </summary>
        public virtual string AcknowledgedParameterName
        {
            get { return "@Acknowledged"; }
        }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the date and time the message
        /// was abandoned when updating queued messages
        /// </summary>
        public virtual string AbandonedParameterName
        {
            get { return "@Abandoned"; }
        }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the name of the topic being
        /// subscribed to when inserting subscriptions
        /// </summary>
        public string TopicNameParameterName
        {
            get { return "@TopicName"; }
        }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the URI of the subscriber
        /// when inserting subscriptions
        /// </summary>
        public string SubscriberParameterName
        {
            get { return "@Subscriber"; }
        }

        /// <summary>
        /// The dialect-specific name for the parameter used to specify the current date and time
        /// on the server when selecting active subscriptions
        /// </summary>
        public string CurrentDateParameterName
        {
            get { return "@CurrentDate"; }
        }

        /// <summary>
        /// The name of the parameter used to specify the start date in queries based on date ranges
        /// </summary>
        public string StartDateParameterName { get { return "@StartDate"; } }

        /// <summary>
        /// The name of the parameter used to specify the end date in queries based on date ranges
        /// </summary>
        public string EndDateParameterName { get { return "@EndDate"; } }

        /// <summary>
        /// The name of the parameter used to specify a category
        /// </summary>
        public string CategoryParameterName { get { return "@Category"; } }
    }
}