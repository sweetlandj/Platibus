// The MIT License (MIT)
// 
// Copyright (c) 2014 Jesse Sweetland
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;

namespace Platibus.Http
{
    /// <summary>
    /// A standalone HTTP server bus host
    /// </summary>
    public class HttpServer : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.Http);

        /// <summary>
        /// Creates and starts a new <see cref="HttpServer"/> based on the configuration
        /// in the named configuration section.
        /// </summary>
        /// <param name="configSectionName">The configuration section containing the
        /// settings for this HTTP server instance.</param>
        /// <param name="cancellationToken">(Optional) A cancelation token that may be
        /// used by the caller to interrupt the HTTP server initialization process</param>
        /// <returns>Returns the fully initialized and listening HTTP server</returns>
        /// <seealso cref="HttpServerConfigurationSection"/>
        public static async Task<HttpServer> Start(string configSectionName = "platibus.httpserver",
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var configuration = await HttpServerConfigurationManager.LoadConfiguration(configSectionName);
            return await Start(configuration, cancellationToken);
        }

        /// <summary>
        /// Creates and starts a new <see cref="HttpServer"/> based on the specified
        /// <paramref name="configuration"/>.
        /// </summary>
        /// <param name="configuration">The configuration for this HTTP server instance.</param>
        /// <param name="cancellationToken">(Optional) A cancelation token that may be
        /// used by the caller to interrupt the HTTP server initialization process</param>
        /// <returns>Returns the fully initialized and listening HTTP server</returns>
        /// <seealso cref="HttpServerConfigurationSection"/>
        public static async Task<HttpServer> Start(IHttpServerConfiguration configuration,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var server = new HttpServer(configuration);
            await server.Init(cancellationToken);
            return server;
        }

        private bool _disposed;
        private readonly Uri _baseUri;
        private readonly ISubscriptionTrackingService _subscriptionTrackingService;
        private readonly IMessageQueueingService _messageQueueingService;
        private readonly IMessageJournalingService _messageJournalingService;
        private readonly HttpTransportService _transportService;
        private readonly Bus _bus;
        private readonly IHttpResourceRouter _resourceRouter;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly HttpListener _httpListener;

        private Task _listenTask;

        public Task<Uri> GetBaseUri()
        {
            return Task.FromResult(_baseUri);
        }

        public Task<ITransportService> GetTransportService()
        {
            return Task.FromResult<ITransportService>(_transportService);
        }

        public IBus Bus
        {
            get { return _bus; }
        }

        private HttpServer(IHttpServerConfiguration configuration)
        {
            _baseUri = configuration.BaseUri;
            _subscriptionTrackingService = configuration.SubscriptionTrackingService;
            _messageQueueingService = configuration.MessageQueueingService;
            _messageJournalingService = configuration.MessageJournalingService;
            var endpoints = configuration.Endpoints;
            _transportService = new HttpTransportService(_baseUri, endpoints, _messageQueueingService, _messageJournalingService, _subscriptionTrackingService);
            _bus = new Bus(configuration, _baseUri, _transportService, _messageQueueingService);

            _resourceRouter = new ResourceTypeDictionaryRouter
            {
                {"message", new MessageController(_bus.HandleMessage)},
                {"topic", new TopicController(_subscriptionTrackingService, configuration.Topics)}
            };
            _httpListener = InitHttpListener(_baseUri, configuration.AuthenticationSchemes);
        }

        private static HttpListener InitHttpListener(Uri baseUri, AuthenticationSchemes authenticationSchemes)
        {
            var httpListener = new HttpListener
            {
                AuthenticationSchemes = authenticationSchemes
            };

            var prefix = baseUri.ToString();
            if (!prefix.EndsWith("/"))
            {
                prefix += "/";
            }
            httpListener.Prefixes.Add(prefix);
            return httpListener;
        }

        private async Task Init(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_httpListener.IsListening) return;

            await _transportService.Init(cancellationToken);
            await _bus.Init(cancellationToken);
            
            Log.InfoFormat("Starting HTTP server listening on {0}...", _baseUri);
            _httpListener.Start();
            Log.InfoFormat("HTTP started successfully");

            // Create a new async task but do not wait for it to complete.
            _listenTask = Listen(_cancellationTokenSource.Token);
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private async Task Listen(CancellationToken cancellationToken = default(CancellationToken))
        {
            while (_httpListener.IsListening)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var context = await _httpListener.GetContextAsync();

                Log.DebugFormat("Accepting {0} request for resource {1} from {2}...",
                    context.Request.HttpMethod, context.Request.Url, context.Request.RemoteEndPoint);

                // Create a new async task but do not wait for it to complete.

                // ReSharper disable once UnusedVariable
                var acceptTask = Accept(context);
            }
        }

        protected async Task Accept(HttpListenerContext context)
        {
            var resourceRequest = new HttpListenerRequestAdapter(context.Request, context.User);
            var resourceResponse = new HttpListenerResponseAdapter(context.Response);
            try
            {
                Log.DebugFormat("Routing {0} request for resource {1} from {2}...",
                    context.Request.HttpMethod, context.Request.Url, context.Request.RemoteEndPoint);

                await _resourceRouter.Route(resourceRequest, resourceResponse);

                Log.DebugFormat("{0} request for resource {1} handled successfully",
                    context.Request.HttpMethod, context.Request.Url);
            }
            catch (Exception ex)
            {
                var exceptionHandler = new HttpExceptionHandler(resourceRequest, resourceResponse, Log);
                exceptionHandler.HandleException(ex);
            }
            finally
            {
                context.Response.Close();
            }
        }

        ~HttpServer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_cancellationTokenSource")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_bus")]
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            Log.Info("Stopping HTTP server...");
            _httpListener.Stop();

            _cancellationTokenSource.Cancel();

            _bus.TryDispose();
            _transportService.TryDispose();
            _messageQueueingService.TryDispose();
            _messageJournalingService.TryDispose();
            _subscriptionTrackingService.TryDispose();

            try
            {
                _httpListener.Close();
            }
            catch (Exception ex)
            {
                Log.Warn("Error closing HTTP listener; aborting...", ex);
                _httpListener.Abort();
            }

            _listenTask.TryWait(TimeSpan.FromSeconds(30));

            Log.InfoFormat("HTTP server stopped");

            _cancellationTokenSource.TryDispose();
        }
    }
}