using System.Threading.Tasks;

namespace Platibus.Http
{
    /// <summary>
    /// Describes the signature for methods that handle transport events
    /// </summary>
    /// <param name="sender">The object that emitted the event (e.g. the 
    /// <see cref="ITransportService"/>)</param>
    /// <param name="args">The arguments to the handler, including the 
    /// <see cref="Message"/> to which the event pertains</param>
    /// <returns>Returns a <see cref="Task"/> that completes once the
    /// event has been handled</returns>
    public delegate Task TransportMessageEventHandler(object sender, TransportMessageEventArgs args);
}
