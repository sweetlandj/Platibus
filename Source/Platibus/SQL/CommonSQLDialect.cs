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
        /// <inheritdoc />
        public abstract string CreateMessageQueueingServiceObjectsCommand { get; }

        /// <inheritdoc />
        public abstract string CreateMessageJournalingServiceObjectsCommand { get; }

        /// <inheritdoc />
        public abstract string CreateSubscriptionTrackingServiceObjectsCommand { get; }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public virtual string InsertJournaledMessageCommand
        {
            get { return @"
INSERT INTO [PB_MessageJournal] (
    [MessageId],
    [Timestamp],
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
    @Timestamp,
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public string UpdateSubscriptionCommand
        {
            get { return @"
UPDATE [PB_Subscriptions] SET [Expires]=@Expires
WHERE [TopicName]=@TopicName
AND [Subscriber]=@Subscriber"; }
        }

        /// <inheritdoc />
        public string SelectSubscriptionsCommand
        {
            get { return @"
SELECT [TopicName], [Subscriber], [Expires]
FROM [PB_Subscriptions]
WHERE [Expires] IS NULL
OR [Expires] > @CurrentDate"; }
        }

        /// <inheritdoc />
        public string DeleteSubscriptionCommand
        {
            get { return @"
DELETE FROM [PB_Subscriptions]
WHERE [TopicName]=@TopicName
AND [Subscriber]=@Subscriber"; }
        }

        /// <inheritdoc />
        public virtual string QueueNameParameterName
        {
            get { return "@QueueName"; }
        }

        /// <inheritdoc />
        public virtual string MessageIdParameterName
        {
            get { return "@MessageId"; }
        }

        /// <inheritdoc />
        public virtual string MessageNameParameterName
        {
            get { return "@MessageName"; }
        }

        /// <inheritdoc />
        public virtual string OriginationParameterName
        {
            get { return "@Origination"; }
        }

        /// <inheritdoc />
        public virtual string DestinationParameterName
        {
            get { return "@Destination"; }
        }

        /// <inheritdoc />
        public virtual string ReplyToParameterName
        {
            get { return "@ReplyTo"; }
        }

        /// <inheritdoc />
        public virtual string ExpiresParameterName
        {
            get { return "@Expires"; }
        }

        /// <inheritdoc />
        public virtual string ContentTypeParameterName
        {
            get { return "@ContentType"; }
        }

        /// <inheritdoc />
        public virtual string HeadersParameterName
        {
            get { return "@Headers"; }
        }

        /// <inheritdoc />
        public virtual string SenderPrincipalParameterName
        {
            get { return "@SenderPrincipal"; }
        }

        /// <inheritdoc />
        public virtual string MessageContentParameterName
        {
            get { return "@MessageContent"; }
        }

        /// <inheritdoc />
        public virtual string AttemptsParameterName
        {
            get { return "@Attempts"; }
        }

        /// <inheritdoc />
        public virtual string AcknowledgedParameterName
        {
            get { return "@Acknowledged"; }
        }

        /// <inheritdoc />
        public virtual string AbandonedParameterName
        {
            get { return "@Abandoned"; }
        }

        /// <inheritdoc />
        public string TopicNameParameterName
        {
            get { return "@TopicName"; }
        }

        /// <inheritdoc />
        public string SubscriberParameterName
        {
            get { return "@Subscriber"; }
        }

        /// <inheritdoc />
        public string CurrentDateParameterName
        {
            get { return "@CurrentDate"; }
        }

        /// <inheritdoc />
        public string StartDateParameterName
        {
            get { return "@StartDate"; }
        }

        /// <inheritdoc />
        public string EndDateParameterName
        {
            get { return "@EndDate"; }
        }

        /// <inheritdoc />
        public string CategoryParameterName
        {
            get { return "@Category"; }
        }

        /// <inheritdoc />
        public string TimestampParameterName
        {
            get { return "@Timestamp"; }
        }
    }
}