namespace Platibus.SQL
{
    public abstract class CommonSQLDialect : ISQLDialect
    {
        public abstract string CreateMessageQueueingServiceObjectsCommand { get; }
        public abstract string CreateSubscriptionTrackingServiceObjectsCommand { get; }

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

        public string UpdateSubscriptionCommand
        {
            get { return @"
UPDATE [PB_Subscriptions] SET [Expires]=@Expires
WHERE [TopicName]=@TopicName
AND [Subscriber]=@Subscriber"; }
        }

        public string SelectSubscriptionsCommand
        {
            get { return @"
SELECT [TopicName], [Subscriber], [Expires]
FROM [PB_Subscriptions]
WHERE [Expires] IS NULL
OR [Expires] > @CurrentDate"; }
        }

        public string DeleteSubscriptionCommand
        {
            get { return @"
DELETE FROM [PB_Subscriptions]
WHERE [TopicName]=@TopicName
AND [Subscriber]=@Subscriber"; }
        }

        public virtual string QueueNameParameterName
        {
            get { return "@QueueName"; }
        }

        public virtual string MessageIdParameterName
        {
            get { return "@MessageId"; }
        }

        public virtual string MessageNameParameterName
        {
            get { return "@MessageName"; }
        }

        public virtual string OriginationParameterName
        {
            get { return "@Origination"; }
        }

        public virtual string DestinationParameterName
        {
            get { return "@Destination"; }
        }

        public virtual string ReplyToParameterName
        {
            get { return "@ReplyTo"; }
        }

        public virtual string ExpiresParameterName
        {
            get { return "@Expires"; }
        }

        public virtual string ContentTypeParameterName
        {
            get { return "@ContentType"; }
        }

        public virtual string HeadersParameterName
        {
            get { return "@Headers"; }
        }

        public virtual string SenderPrincipalParameterName
        {
            get { return "@SenderPrincipal"; }
        }

        public virtual string MessageContentParameterName
        {
            get { return "@MessageContent"; }
        }

        public virtual string AttemptsParameterName
        {
            get { return "@Attempts"; }
        }

        public virtual string AcknowledgedParameterName
        {
            get { return "@Acknowledged"; }
        }

        public virtual string AbandonedParameterName
        {
            get { return "@Abandoned"; }
        }

        public string TopicNameParameterName
        {
            get { return "@TopicName"; }
        }

        public string SubscriberParameterName
        {
            get { return "@Subscriber"; }
        }

        public string CurrentDateParameterName
        {
            get { return "@CurrentDate"; }
        }
    }
}