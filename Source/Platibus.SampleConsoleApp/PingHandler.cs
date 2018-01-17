using System;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.SampleConsoleApp
{
    public class PingHandler : IMessageHandler<Ping>
    {
        private readonly TaskCompletionSource<bool> _taskCompletionSource;
        private readonly string _id;

        public Task Completed => _taskCompletionSource.Task;

        public PingHandler(string id)
        {
            _id = id;
            _taskCompletionSource = new TaskCompletionSource<bool>();
        }

        public async Task HandleMessage(Ping content, IMessageContext messageContext, CancellationToken cancellationToken)
        {
            cancellationToken.Register(() => _taskCompletionSource.TrySetCanceled(cancellationToken));

            Console.WriteLine($"[{_id}] Received ping from {content.Sender}: {content.Counter}");

            var counter = content.Counter;
            if (counter > 0)
            {
                counter--;
                var ping = new Ping(_id, counter);
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                await messageContext.Bus.Send(ping, cancellationToken: cancellationToken);
                Console.WriteLine($"[{_id}] Sent response ping: {counter}");
            }

            messageContext.Acknowledge();

            if (content.Counter <= 0)
            {
                _taskCompletionSource.TrySetResult(true);
            }
        }
    }
}
