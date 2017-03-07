using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Common.Logging;

namespace Platibus.Multicast
{
    /// <summary>
    /// A <see cref="ISubscriptionTrackingService"/> implementation that broadcasts and consumes
    /// subscription changes on a multicast group in addition to maintaining local subscription
    /// state.
    /// </summary>
    public class MulticastSubscriptionTrackingService : ISubscriptionTrackingService, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.UDP);

        private readonly ISubscriptionTrackingService _inner;
        private readonly IPAddress _groupAddress;
        private readonly int _port;
        private readonly UdpClient _broadcastClient;
        private readonly UdpClient _listeningClient;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Task _listeningTask;
        private readonly ActionBlock<UdpReceiveResult> _receiveResultQueue;

        /// <summary>
        /// Initializes a new <see cref="MulticastSubscriptionTrackingService"/> decorating the
        /// specified <paramref name="inner"/> <see cref="ISubscriptionTrackingService"/> 
        /// configured to broadcast and consume changes over the multicast group at the specified
        /// <paramref name="groupAddress"/>.
        /// </summary>
        /// <param name="inner">The underlying subscription tracking service used to persist or
        /// maintain subscription state</param>
        /// <param name="groupAddress">The address of the multicast group to join</param>
        /// <param name="port">The local port to bind to</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="inner"/> or
        /// <paramref name="groupAddress"/> is <c>null</c></exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="groupAddress"/> does not
        /// specify a compatible <see cref="AddressFamily"/></exception>
        /// <exception cref="SocketException">Thrown if there is an error joining the multicast
        /// group, i.e. due to an invalid multicast address.</exception>
        /// <remarks>
        /// The multicast group <paramref name="groupAddress"/> must be on the 224.0.0/4 subnet 
        /// </remarks>
        public MulticastSubscriptionTrackingService(ISubscriptionTrackingService inner, IPAddress groupAddress, int port)
        {
            if (inner == null) throw new ArgumentNullException("inner");
            if (groupAddress == null) throw new ArgumentNullException("groupAddress");
            _inner = inner;
            _groupAddress = groupAddress;
            _port = port;

            var localIpAddress = GetLocalIPAddress(_groupAddress.AddressFamily);
            Log.InfoFormat("Starting multicast subscription tracking service on group address {0}...", _groupAddress);

            Log.InfoFormat("Binding multicast broadcast socket to local address {0}...", localIpAddress);
            _broadcastClient = new UdpClient
            {
                ExclusiveAddressUse = false
            };
            _broadcastClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _broadcastClient.JoinMulticastGroup(groupAddress, IPAddress.Any);
            _broadcastClient.Ttl = 2;
            _broadcastClient.Client.Bind(new IPEndPoint(localIpAddress, 0));
            Log.InfoFormat("Multicast broadcast socket bound to {0}, port {1}", localIpAddress, localIpAddress, ((IPEndPoint)_broadcastClient.Client.LocalEndPoint).Port);

            Log.InfoFormat("Binding multicast listening socket to local address {0}...", localIpAddress);
            _listeningClient = new UdpClient
            {
                ExclusiveAddressUse = false
            };
            _listeningClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _listeningClient.JoinMulticastGroup(groupAddress, IPAddress.Any);
            _listeningClient.Client.Bind(new IPEndPoint(localIpAddress, _port));
            Log.InfoFormat("Multicast listening socket bound to {0}, port {1}", localIpAddress, ((IPEndPoint)_listeningClient.Client.LocalEndPoint).Port);

            _cancellationTokenSource = new CancellationTokenSource();
            _listeningTask = Task.Run(async () => await Listen(_cancellationTokenSource.Token));

            _receiveResultQueue = new ActionBlock<UdpReceiveResult>(result => HandleReceiveResult(result),
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = 2
                });
        }

        private static IPAddress GetLocalIPAddress(AddressFamily addressFamily)
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .SelectMany(i => i.GetIPProperties().UnicastAddresses)
                .Where(a => a.Address.AddressFamily == addressFamily && a.IsDnsEligible)
                .Select(a => a.Address)
                .FirstOrDefault();
        }

        private async Task Listen(CancellationToken cancellationToken)
        {
            var cancelation = new TaskCompletionSource<bool>();
            cancellationToken.Register(() => cancelation.TrySetCanceled());
            var canceled = cancelation.Task;
            try
            {
                Log.DebugFormat("Multicast listener started");
                while (!cancellationToken.IsCancellationRequested)
                {
                    var received = _listeningClient.ReceiveAsync();
                    await Task.WhenAny(received, canceled);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    var receiveResult = await received;
                    Log.DebugFormat("Received multicast datagram from {0}", receiveResult.RemoteEndPoint);
                    await _receiveResultQueue.SendAsync(receiveResult, cancellationToken);
                }
                Log.DebugFormat("Multicast listener stopped");
            }
            catch (OperationCanceledException)
            {
                Log.DebugFormat("Multicast listener canceled");
            }
        }

        private async Task HandleReceiveResult(UdpReceiveResult result)
        {
            try
            {
                var datagram = SubscriptionTrackingDatagram.Decode(result.Buffer);
                switch (datagram.Action)
                {
                    case SubscriptionTrackingDatagram.ActionType.Add:
                        await _inner.AddSubscription(datagram.Topic, datagram.SubscriberUri, datagram.TTL);
                        return;
                    case SubscriptionTrackingDatagram.ActionType.Remove:
                        await _inner.RemoveSubscription(datagram.Topic, datagram.SubscriberUri);
                        return;
                    default:
                        Log.WarnFormat("Unknown or unsupported action type in UDP datagram received from remote endpoint {0}: {1}", result.RemoteEndPoint, datagram.Action);
                        break;
                }
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Error processing UDP datagram received from remote endpoint {0}", e, result.RemoteEndPoint);
            }
        }

        private async Task Broadcast(SubscriptionTrackingDatagram datagram)
        {
            try
            {
                var bytes = datagram.Encode();
                var endpoint = new IPEndPoint(_groupAddress, _port);
                Log.DebugFormat("Broadcasting subscription tracking datagram to {0}...", endpoint);
                await _broadcastClient.SendAsync(bytes, bytes.Length, endpoint);
            }
            catch (Exception e)
            {
                Log.Error("Error broadcasting UDP datagram", e);
            }
        }

        /// <inheritdoc />
        public async Task AddSubscription(TopicName topic, Uri subscriber, TimeSpan ttl = new TimeSpan(),
            CancellationToken cancellationToken = new CancellationToken())
        {
            await _inner.AddSubscription(topic, subscriber, ttl, cancellationToken);
            var datagram = new SubscriptionTrackingDatagram(SubscriptionTrackingDatagram.ActionType.Add, topic, subscriber, ttl);
            await Broadcast(datagram);
        }

        /// <inheritdoc />
        public async Task RemoveSubscription(TopicName topic, Uri subscriber, CancellationToken cancellationToken = new CancellationToken())
        {
            await _inner.RemoveSubscription(topic, subscriber, cancellationToken);
            var datagram = new SubscriptionTrackingDatagram(SubscriptionTrackingDatagram.ActionType.Remove, topic, subscriber);
            await Broadcast(datagram);
        }

        /// <inheritdoc />
        public Task<IEnumerable<Uri>> GetSubscribers(TopicName topic, CancellationToken cancellationToken = new CancellationToken())
        {
            return _inner.GetSubscribers(topic, cancellationToken);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }

            if (_listeningTask != null)
            {
                _listeningTask.Wait(TimeSpan.FromSeconds(5));
            }

            if (_receiveResultQueue != null)
            {
                _receiveResultQueue.Complete();
                _receiveResultQueue.Completion.Wait(TimeSpan.FromSeconds(5));
            }

            if (_broadcastClient != null)
            {
                _broadcastClient.DropMulticastGroup(_groupAddress);
                _broadcastClient.Close();
            }

            if (_listeningClient != null)
            {
                _listeningClient.DropMulticastGroup(_groupAddress);
                _listeningClient.Close();
            }
        }
    }
}
