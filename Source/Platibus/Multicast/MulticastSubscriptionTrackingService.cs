using System;
using System.Collections.Generic;
using System.Net;
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
        private readonly UdpClient _listenerClient;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Task _listeningTask;
        private readonly ActionBlock<UdpReceiveResult> _receiveResultQueue;
        private readonly NodeId _nodeId;

        private bool _disposed;

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

            _nodeId = NodeId.Generate();

            _inner = inner;
            _groupAddress = groupAddress;
            _port = port;

            Log.InfoFormat("[{0}] Starting multicast subscription tracking service for multicast group {1}:{2}...", _nodeId, _groupAddress, _port);

            Log.InfoFormat("[{0}] Binding multicast broadcast socket...", _nodeId);
            _broadcastClient = new UdpClient
            {
                ExclusiveAddressUse = false
            };
            _broadcastClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _broadcastClient.JoinMulticastGroup(groupAddress, IPAddress.Any);
            _broadcastClient.Ttl = 2;
            _broadcastClient.Client.Bind(new IPEndPoint(IPAddress.Any, 0));

            var broadcastBinding = (IPEndPoint) _broadcastClient.Client.LocalEndPoint;
            Log.InfoFormat("[{0}] Multicast broadcast socket bound to {1}:{2}", _nodeId, broadcastBinding.Address, broadcastBinding.Port);

            Log.InfoFormat("[{0}] Binding multicast listener socket...", _nodeId);
            _listenerClient = new UdpClient
            {
                ExclusiveAddressUse = false
            };
            _listenerClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _listenerClient.JoinMulticastGroup(groupAddress, IPAddress.Any);
            _listenerClient.Client.Bind(new IPEndPoint(IPAddress.Any, _port));

            var listenerBinding = (IPEndPoint)_broadcastClient.Client.LocalEndPoint;
            Log.InfoFormat("[{0}] Multicast listener socket bound to {1}:{2}", _nodeId, listenerBinding.Address, listenerBinding.Port);

            _cancellationTokenSource = new CancellationTokenSource();
            _listeningTask = Task.Run(async () => await Listen(_cancellationTokenSource.Token));

            _receiveResultQueue = new ActionBlock<UdpReceiveResult>(result => HandleReceiveResult(result),
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = 2
                });
        }
        
        private async Task Listen(CancellationToken cancellationToken)
        {
            var cancelation = new TaskCompletionSource<bool>();
            cancellationToken.Register(() => cancelation.TrySetCanceled());
            var canceled = cancelation.Task;
            try
            {
                Log.DebugFormat("[{0}] Multicast listener started", _nodeId);
                while (!cancellationToken.IsCancellationRequested)
                {
                    var received = _listenerClient.ReceiveAsync();
                    await Task.WhenAny(received, canceled);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    var receiveResult = await received;
                    Log.TraceFormat("[{0}] Received multicast datagram from {1}", _nodeId, receiveResult.RemoteEndPoint);
                    await _receiveResultQueue.SendAsync(receiveResult, cancellationToken);
                }
                Log.DebugFormat("[{0}] Multicast listener stopped", _nodeId);
            }
            catch (OperationCanceledException)
            {
                Log.DebugFormat("[{0}] Multicast listener canceled", _nodeId);
            }
        }

        private async Task HandleReceiveResult(UdpReceiveResult result)
        {
            try
            {
                var datagram = SubscriptionTrackingDatagram.Decode(result.Buffer);
                if (datagram.NodeId == _nodeId)
                {
                    Log.TraceFormat("[{0}] Ignoring local broadcast datagram (same node)", _nodeId);
                    return;
                }

                switch (datagram.Action)
                {
                    case SubscriptionTrackingDatagram.ActionType.Add:
                        await _inner.AddSubscription(datagram.Topic, datagram.SubscriberUri, datagram.TTL);
                        return;
                    case SubscriptionTrackingDatagram.ActionType.Remove:
                        await _inner.RemoveSubscription(datagram.Topic, datagram.SubscriberUri);
                        return;
                    default:
                        Log.WarnFormat("[{0}] Unknown or unsupported action type in datagram received from remote endpoint {1}: {2}", _nodeId, result.RemoteEndPoint, datagram.Action);
                        break;
                }
            }
            catch (Exception e)
            {
                Log.ErrorFormat("[{0}] Error processing datagram received from remote endpoint {1}", e, _nodeId, result.RemoteEndPoint);
            }
        }

        private async Task Broadcast(SubscriptionTrackingDatagram datagram)
        {
            try
            {
                var bytes = datagram.Encode();
                var endpoint = new IPEndPoint(_groupAddress, _port);
                Log.DebugFormat("[{0}] Broadcasting subscription tracking datagram to {1}:{2}...", _nodeId, endpoint.Address, endpoint.Port);
                await _broadcastClient.SendAsync(bytes, bytes.Length, endpoint);
            }
            catch (Exception e)
            {
                Log.ErrorFormat("[{0}] Error broadcasting datagram", e, _nodeId);
            }
        }

        /// <inheritdoc />
        public async Task AddSubscription(TopicName topic, Uri subscriber, TimeSpan ttl = new TimeSpan(),
            CancellationToken cancellationToken = new CancellationToken())
        {
            CheckDisposed();
            await _inner.AddSubscription(topic, subscriber, ttl, cancellationToken);
            var datagram = new SubscriptionTrackingDatagram(_nodeId,
                SubscriptionTrackingDatagram.ActionType.Add, topic, subscriber, ttl);
            await Broadcast(datagram);
        }

        /// <inheritdoc />
        public async Task RemoveSubscription(TopicName topic, Uri subscriber, CancellationToken cancellationToken = new CancellationToken())
        {
            CheckDisposed();
            await _inner.RemoveSubscription(topic, subscriber, cancellationToken);
            var datagram = new SubscriptionTrackingDatagram(_nodeId,
                SubscriptionTrackingDatagram.ActionType.Remove, topic, subscriber);
            await Broadcast(datagram);
        }

        /// <inheritdoc />
        public Task<IEnumerable<Uri>> GetSubscribers(TopicName topic, CancellationToken cancellationToken = new CancellationToken())
        {
            CheckDisposed();
            return _inner.GetSubscribers(topic, cancellationToken);
        }

        /// <summary>
        /// Throws an exception if this object has already been disposed
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the object has been disposed</exception>
        protected void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }
        
        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting 
        /// unmanaged resources.
        /// </summary>
        /// <param name="disposing">Indicates whether this method was called from the 
        /// <see cref="Dispose()"/> method</param>
        protected virtual void Dispose(bool disposing)
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

            if (_listenerClient != null)
            {
                _listenerClient.DropMulticastGroup(_groupAddress);
                _listenerClient.Close();
            }
        }
    }
}
