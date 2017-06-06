using System;

namespace Platibus.Queueing
{
    /// <summary>
    /// Event arguments describing a failed message handling attempt 
    /// </summary>
    public class MessageQueueEventArgs : EventArgs
    {
        private readonly QueueName _queue;
        private readonly QueuedMessage _queuedMessage;
        private readonly Exception _exception;

        /// <summary>
        /// The name of the queue from which the message was read
        /// </summary>
        public QueueName Queue
        {
            get { return _queue; }
        }

        /// <summary>
        /// The queued message for which the handling attempt was made
        /// </summary>
        public QueuedMessage QueuedMessage
        {
            get { return _queuedMessage; }
        }
        
        /// <summary>
        /// The exception that was caught during the handling attempt, if applicable
        /// </summary>
        public Exception Exception
        {
            get { return _exception; }
        }

        /// <summary>
        /// Initializes a new <see cref="MessageQueueEventArgs"/>
        /// </summary>
        /// <param name="queue">The name of the queue from which the message was read</param>
        /// <param name="queuedMessage">The queued message for which the handling attempt was made</param>
        public MessageQueueEventArgs(QueueName queue, QueuedMessage queuedMessage)
        {
            _queue = queue;
            _queuedMessage = queuedMessage;
        }
        

        /// <summary>
        /// Initializes a new <see cref="MessageQueueEventArgs"/>
        /// </summary>
        /// <param name="queue">The name of the queue from which the message was read</param>
        /// <param name="queuedMessage">The queued message for which the handling attempt was made</param>
        /// <param name="exception">The exception that was caught</param>
        public MessageQueueEventArgs(QueueName queue, QueuedMessage queuedMessage, Exception exception)
        {
            _queue = queue;
            _queuedMessage = queuedMessage;
            _exception = exception;
        }
    }
}
