
using System.Collections.Generic;
using System.Web;
using Platibus.Config;

namespace Platibus.IIS
{
    public class PlatibusHttpHandlerFactory : IHttpHandlerFactory, IBusHost
    {
        private static readonly IDictionary<string, Bus> Buses = new Dictionary<string, Bus>();

        static PlatibusHttpHandlerFactory()
        {
            
        }

        public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        {
            
        }

        public void ReleaseHandler(IHttpHandler handler)
        {
            
        }

        public event MessageReceivedHandler MessageReceived;
        public ITransportService TransportService { get; private set; }
    }
}
