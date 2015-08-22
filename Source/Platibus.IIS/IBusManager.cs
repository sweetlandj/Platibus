using System.Threading.Tasks;

namespace Platibus.IIS
{
    /// <summary>
    /// Manages access to the IIS-hosted bus
    /// </summary>
    public interface IBusManager
    {
        /// <summary>
        /// Provides access to the IIS-hosted bus
        /// </summary>
        /// <returns>Returns a task whose result is the bus instance</returns>
        Task<IBus> GetBus();
    }
}