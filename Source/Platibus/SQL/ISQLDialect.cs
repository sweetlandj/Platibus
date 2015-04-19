
namespace Platibus.SQL
{
    public interface ISQLDialect
    {
        string CreateMessageQueueingServiceObjectsCommand { get; }
        string CreateSubscriptionTrackingServiceObjectsCommand { get; }

        string InsertQueuedMessageCommand { get; }
        string SelectQueuedMessagesCommand { get; }
        string UpdateQueuedMessageCommand { get; }

        string InsertSubscriptionCommand { get; }
        string UpdateSubscriptionCommand { get; }
        string SelectSubscriptionsCommand { get; }
        string DeleteSubscriptionCommand { get; }

        string QueueNameParameterName { get; }
        string MessageIdParameterName { get; }
        string MessageNameParameterName { get; }
        string OriginationParameterName { get; }
        string DestinationParameterName { get; }
        string ReplyToParameterName { get; }
        string ExpiresParameterName { get; }
        string ContentTypeParameterName { get; }
        string HeadersParameterName { get; }
        string SenderPrincipalParameterName { get; }
        string MessageContentParameterName { get; }
        string AttemptsParameterName { get; }
        string AcknowledgedParameterName { get; }
        string AbandonedParameterName { get; }

        string TopicNameParameterName { get; }
        string SubscriberParameterName { get; }
        string CurrentDateParameterName { get; }
    }
}
