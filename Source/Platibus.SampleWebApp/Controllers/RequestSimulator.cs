using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Platibus.SampleMessages.Diagnostics;

namespace Platibus.SampleWebApp.Controllers
{
    public class RequestSimulator
    {
        private readonly Random _rng = new Random();
        private readonly IBus _bus;
        
        private readonly int _requests;
        private readonly int _minTime;
        private readonly int _maxTime;
        private readonly double _acknowledgementRate;
        private readonly double _replyRate;
        private readonly double _errorRate;

        private readonly CancellationTokenSource _cancellationTokenSource;

        public RequestSimulator(IBus bus,
            int requests, int minTime, int maxTime, 
            double acknowledgementRate, double replyRate, double errorRate)
        {
            _bus = bus;
            _cancellationTokenSource = new CancellationTokenSource();
            _requests = requests;
            _minTime = minTime;
            _maxTime = maxTime;
            _acknowledgementRate = acknowledgementRate;
            _replyRate = replyRate;
            _errorRate = errorRate;
        }

        public void Start()
        {
            Task.Run(() => SendSimulatedRequests(_cancellationTokenSource.Token));
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        private Task SendSimulatedRequests(CancellationToken cancellationToken)
        {
            var tasks = Enumerable.Range(0, _requests)
                .Select(_ => SendSimulatedRequest(cancellationToken));

            return Task.WhenAll(tasks);
        }

        private async Task SendSimulatedRequest(CancellationToken cancellationToken)
        {
            var request = new SimulatedRequest
            {
                CorrelationId = Guid.NewGuid().ToString(),
                Error = _rng.NextDouble() < _errorRate,
                Time = _rng.Next(_minTime, _maxTime)
            };

            if (!request.Error)
            {
                request.Reply = _rng.NextDouble() < _replyRate;
                request.Acknowledge = _rng.NextDouble() < _acknowledgementRate;
            }

            // Wait a random amount of time (0 - 10 seconds) before sending to even the calls out
            await Task.Delay(_rng.Next(10000), cancellationToken);

            await _bus.Send(request, null, cancellationToken);
        }
    }
}