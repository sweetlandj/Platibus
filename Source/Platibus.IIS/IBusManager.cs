using System.Threading.Tasks;

namespace Platibus.IIS
{
    public interface IBusManager
    {
        Task<IBus> GetBus();
    }
}