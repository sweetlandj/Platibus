using System.Threading;
using System.Threading.Tasks;
using Pluribus.Serialization;

namespace Pluribus.SampleWebApp.Controllers
{
    public class TestHandler : DeserializedContentHandler<TestMessage>
    {
        protected override Task HandleMessageContent(TestMessage content, IMessageContext context, CancellationToken cancellationToken)
        {
            // TODO: Do something with the message
            context.Acknowledge();
            return Task.FromResult(true);
        }
    }
}