using System;
using System.Threading;
using System.Threading.Tasks;
using Platibus.SampleMessages.Diagnostics;

namespace Platibus.SampleApi.Diagnostics
{
    public class SimulatedRequestHandler : IMessageHandler<SimulatedRequest>
    {
        public async Task HandleMessage(SimulatedRequest content, IMessageContext messageContext, CancellationToken cancellationToken)
        {
            if (content.Time > 0)
            {
                await Task.Delay(content.Time, cancellationToken);
            }

            if (content.Error)
            {
                throw new Exception("Simulating ynhandled exception thrown from handler");
            }

            if (content.Reply)
            {
                var reply = new SimulatedResponse
                {
                    CorrelationId = content.CorrelationId
                };
                await messageContext.SendReply(reply, null, cancellationToken);
            }

            if (content.Acknowledge)
            {
                messageContext.Acknowledge();
            }
        }
    }
}