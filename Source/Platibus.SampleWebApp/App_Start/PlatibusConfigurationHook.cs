using Platibus.Config;
using Platibus.SampleWebApp.Controllers;

namespace Platibus.SampleWebApp
{
    public class PlatibusConfigurationHook : IConfigurationHook
    {
        public void Configure(PlatibusConfiguration configuration)
        {
            var receivedMessageRespository = new ReceivedMessageRepository();
            receivedMessageRespository.Init();

            configuration.AddHandlingRule(".*TestMessage", new ReceivedMessageHandler(receivedMessageRespository));
        }
    }
}