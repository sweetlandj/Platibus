namespace Platibus.MongoDB
{
    /// <summary>
    /// A delegate that produces the name of the MongoDB collection to use
    /// to store queued messages for the queue with the specified 
    /// <paramref name="queueName"/>.
    /// </summary>
    /// <param name="queueName">The name of the queue</param>
    /// <returns>Returns the name of the MongoDB collection in which the
    /// queued messages should be stored</returns>
    public delegate string QueueCollectionNameFactory(QueueName queueName);
}
