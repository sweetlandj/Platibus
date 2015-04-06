using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platibus.SQL
{
    public interface ISQLDialect
    {
        string CreateObjectsCommand { get; }

        string InsertQueueCommand { get; }
        string UpdateQueueCommand { get; }

        string InsertQueuedMessageCommand { get; }
        string SelectQueuedMessagesCommand { get; }
        string UpdateQueuedMessageCommand { get; }

        string QueueNameParameterName { get; }
        string MaxConcurrencyParameterName { get; }
        string MaxAttemptsParameterName { get; }
        string RetryDelayParameterName { get; }

        string CurrentDateParameterName { get; }
        string MessageIdParameterName { get; }
        string MessageNameParameterName { get; }
        string OriginationParameterName { get; }
        string DestinationParameterName { get; }
        string ReplyToParameterName { get; }
        string ExpiresParameterName { get; }
        string ContentTypeParameterName { get; }
        string MessageHeadersParameterName { get; }
        string SenderPrincipalParameterName { get; }
        string MessageContentParameterName { get; }
        string AttemptsParameterName { get; }
        string AcknowledgedParameterName { get; }
        string AbandonedParameterName { get; }
    }
}
