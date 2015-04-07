using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platibus.SQL
{
    public abstract class CommonSQLDialect : ISQLDialect
    {
        public abstract string CreateObjectsCommand { get; }

        public virtual string InsertQueuedMessageCommand
        {
            get { return CommonSQLCommands.InsertQueuedMessageCommand; }
        }

        public virtual string SelectQueuedMessagesCommand
        {
            get { return CommonSQLCommands.SelectQueuedMessagesCommand; }
        }

        public virtual string UpdateQueuedMessageCommand
        {
            get { return CommonSQLCommands.UpdateQueuedMessageCommand; }
        }

        public virtual string QueueNameParameterName
        {
            get { return "@QueueName"; }
        }

        public virtual string CurrentDateParameterName
        {
            get { return "@CurrentDate"; }
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
    }
}
