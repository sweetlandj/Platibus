using System;

namespace Platibus.Queueing
{
    /// <summary>
    /// Event arguments describing a failed message handling attempt 
    /// </summary>
    public class MessageQueueEventArgs : EventArgs
    {
        /// <summary>
        /// The name of the queue from which the message was read
        /// </summary>
        public QueueName Queue { get; }

        /// <summary>
        /// The queued message for which the handling attempt was made
        /// </summary>
        public QueuedMessage QueuedMessage { get; }

        /// <summary>
        /// The exception that was caught during the handling attempt, if applicable
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Initializes a new <see cref="MessageQueueEventArgs"/>
        /// </summary>
        /// <param name="queue">The name of the queue from which the message was read</param>
        /// <param name="queuedMessage">The queued message for which the handling attempt was made</param>
        public MessageQueueEventArgs(QueueName queue, QueuedMessage queuedMessage)
        {
            Queue = queue;
            QueuedMessage = queuedMessage;
        }
        

        /// <summary>
        /// Initializes a new <see cref="MessageQueueEventArgs"/>
        /// </summary>
        /// <param name="queue">The name of the queue from which the message was read</param>
        /// <param name="queuedMessage">The queued message for which the handling attempt was made</param>
        /// <param name="exception">The exception that was caught</param>
        public MessageQueueEventArgs(QueueName queue, QueuedMessage queuedMessage, Exception exception)
        {
            Queue = queue;
            QueuedMessage = queuedMessage;
            Exception = exception;
        }
    }
}
