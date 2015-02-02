using Pluribus.Config;
using Pluribus.SampleWebApp.Controllers;

namespace Pluribus.SampleWebApp
{
    public class PluribusConfigurationHook : IConfigurationHook
    {
        public void Configure(PluribusConfiguration configuration)
        {
            var receivedMessageRespository = new ReceivedMessageRepository();
            receivedMessageRespository.Init();

            configuration.AddHandlingRule(".*TestMessage", new ReceivedMessageHandler(receivedMessageRespository));
        }
    }
}