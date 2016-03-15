using System;

namespace Platibus.Config
{
    /// <summary>
    /// A delegate that receives as input the type of handler and the type of message
    /// and returns a queue name
    /// </summary>
    /// <param name="handlerType">The message handler type</param>
    /// <param name="messageType">The message type</param>
    /// <returns>The name of the queue to assign to the handling rule</returns>
    public delegate QueueName QueueNameFactory(Type handlerType, Type messageType);
}