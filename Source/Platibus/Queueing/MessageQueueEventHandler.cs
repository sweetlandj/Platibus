using System.Threading.Tasks;

namespace Platibus.Queueing
{
    /// <summary>
    /// A signature describing a method that handles message queue events
    /// </summary>
    /// <param name="sender">The object that raised the event</param>
    /// <param name="args">The event arguments</param>
    public delegate Task MessageQueueEventHandler(object sender, MessageQueueEventArgs args);
}
