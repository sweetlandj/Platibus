// The MIT License (MIT)
// 
// Copyright (c) 2016 Jesse Sweetland
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
using System.Threading.Tasks.Dataflow;
using Platibus.Diagnostics;
using Platibus.Http.Controllers;
using Platibus.Journaling;

namespace Platibus.Http
{
    /// <inheritdoc />
    /// <summary>
    /// A standalone HTTP server bus host
    /// </summary>
    public class HttpServer : IDisposable
    {
        /// <summary>
        /// Creates and starts a new <see cref="HttpServer"/> based on the configuration
        /// in the named configuration section.
        /// </summary>
        /// <param name="configSectionName">The configuration section containing the
        /// settings for this HTTP server instance.</param>
        /// <param name="cancellationToken">(Optional) A cancelation token that may be
        /// used by the caller to interrupt the HTTP server initialization process</param>
        /// <returns>Returns the fully initialized and listening HTTP server</returns>
#if NET452 || NET461
        /// <seealso cref="HttpServerConfigurationSection"/> 
#endif
        public static async Task<HttpServer> Start(string configSectionName = "platibus.httpserver",
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var configManager = new HttpServerConfigurationManager();
            var configuration = new HttpServerConfiguration();
            await configManager.Initialize(configuration, configSectionName);
            await configManager.FindAndProcessConfigurationHooks(configuration);
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
#if NET452 || NET461
        /// <seealso cref="HttpServerConfigurationSection"/> 
#endif
        public static async Task<HttpServer> Start(IHttpServerConfiguration configuration,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var server = new HttpServer(configuration);
            await server.Init(cancellationToken);
            return server;
        }

        private bool _disposed;
        private readonly Uri _baseUri;
        private readonly IDiagnosticService _diagnosticService;
        private readonly HttpMetricsCollector _metricsCollector;
        private readonly ISubscriptionTrackingService _subscriptionTrackingService;
        private readonly IMessageQueueingService _messageQueueingService;
        private readonly IMessageJournal _messageJournal;
        private readonly Bus _bus;
        private readonly IHttpResourceRouter _resourceRouter;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly HttpListener _httpListener;
        private readonly ActionBlock<HttpListenerContext> _acceptBlock; 

        private Task _listenTask;

        /// <summary>
        /// Returns the transport service used by the bus instance
        /// </summary>
        public HttpTransportService TransportService { get; }

        /// <summary>
        /// The hosted bus instance
        /// </summary>
        public IBus Bus => _bus;

        private HttpServer(IHttpServerConfiguration configuration)
        {
            _baseUri = configuration.BaseUri;

            _metricsCollector = new HttpMetricsCollector();
            _diagnosticService = configuration.DiagnosticService;
            _diagnosticService.AddSink(_metricsCollector);

            _subscriptionTrackingService = configuration.SubscriptionTrackingService;
            _messageQueueingService = configuration.MessageQueueingService;
            _messageJournal = configuration.MessageJournal;
            
            var transportServiceOptions = new HttpTransportServiceOptions(_baseUri, _messageQueueingService, _subscriptionTrackingService)
            {
                DiagnosticService = configuration.DiagnosticService,
                Endpoints = configuration.Endpoints,
                MessageJournal = configuration.MessageJournal,
                BypassTransportLocalDestination = configuration.BypassTransportLocalDestination
            };
            TransportService = new HttpTransportService(transportServiceOptions);

            _bus = new Bus(configuration, _baseUri, TransportService, _messageQueueingService);

            TransportService.LocalDelivery += (sender, args) => _bus.HandleMessage(args.Message, args.Principal);

            var authorizationService = configuration.AuthorizationService;
            _resourceRouter = new ResourceTypeDictionaryRouter(configuration.BaseUri)
            {
                {"message", new MessageController(_bus.HandleMessage, authorizationService)},
                {"topic", new TopicController(_subscriptionTrackingService, configuration.Topics, authorizationService)},
                {"journal", new JournalController(configuration.MessageJournal, configuration.AuthorizationService)},
                {"metrics", new MetricsController(_metricsCollector)}
            };
            _httpListener = InitHttpListener(_baseUri, configuration.AuthenticationSchemes);

            var acceptBlockOptions = new ExecutionDataflowBlockOptions
            {
                CancellationToken = _cancellationTokenSource.Token
            };

            if (configuration.ConcurrencyLimit > 0)
            {
                acceptBlockOptions.MaxDegreeOfParallelism = configuration.ConcurrencyLimit;
            }

            _acceptBlock = new ActionBlock<HttpListenerContext>(async ctx => await Accept(ctx), acceptBlockOptions);
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

            await TransportService.Init(cancellationToken);
            await _bus.Init(cancellationToken);
            
            _httpListener.Start();

            await _diagnosticService.EmitAsync(
                new HttpEventBuilder(this, HttpEventType.HttpServerStarted)
                {
                    Detail = "HTTP listener started",
                    Uri = _baseUri
                }.Build(), cancellationToken);

            // Create a new async task but do not wait for it to complete.
            _listenTask = Listen(_cancellationTokenSource.Token);
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private async Task Listen(CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var cancelationSource = new TaskCompletionSource<bool>();
                cancellationToken.Register(() => cancelationSource.TrySetResult(true));
                var canceled = cancelationSource.Task;

                while (_httpListener.IsListening && !cancellationToken.IsCancellationRequested)
                {
                    var contextReceived = _httpListener.GetContextAsync();
                    await Task.WhenAny(contextReceived, canceled);

                    if (cancellationToken.IsCancellationRequested) break;

                    var context = await contextReceived;
                    var request = context.Request;
                    var remote = request.RemoteEndPoint?.ToString();
                    await _diagnosticService.EmitAsync(
                        new HttpEventBuilder(this, HttpEventType.HttpRequestReceived)
                        {
                            Remote = remote,
                            Uri = request.Url,
                            Method = request.HttpMethod
                        }.Build(), cancellationToken);
                    
                    // Create a new async task but do not wait for it to complete.
                    await _acceptBlock.SendAsync(context, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (HttpListenerException)
            {
                // Thrown with GetContextAsync is interrupted due to thie
                // listener stopping
            }
        }

        /// <summary>
        /// Accepts an incoming HTTP request
        /// </summary>
        /// <param name="context">The HTTP listener context</param>
        /// <returns>Returns a task that will complete when the request has been handled</returns>
        protected async Task Accept(HttpListenerContext context)
        {
            var request = context.Request;
            var remote = request.RemoteEndPoint?.ToString();
            var resourceRequest = new HttpListenerRequestAdapter(context.Request, context.User);
            var resourceResponse = new HttpListenerResponseAdapter(context.Response);
            try
            {
                await _resourceRouter.Route(resourceRequest, resourceResponse);
            }
            catch (Exception ex)
            {
                var exceptionHandler = new HttpExceptionHandler(resourceRequest, resourceResponse, _diagnosticService);
                exceptionHandler.HandleException(ex);
            }
            finally
            {
                context.Response.Close();
            }

            await _diagnosticService.EmitAsync(
                new HttpEventBuilder(this, HttpEventType.HttpResponseSent)
                {
                    Remote = remote,
                    Uri = request.Url,
                    Method = request.HttpMethod
                }.Build());
        }

        /// <summary>
        /// Finalizer that ensures all resources are released
        /// </summary>
        ~HttpServer()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Called by the <see cref="Dispose()"/> method or finalizer to ensure that
        /// resources are released
        /// </summary>
        /// <param name="disposing">Indicates whether this method is called from the 
        /// <see cref="Dispose()"/> method (<c>true</c>) or the finalizer (<c>false</c>)</param>
        /// <remarks>
        /// This method will not be called more than once
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            _httpListener.Stop();
            _cancellationTokenSource.Cancel();
            _listenTask.Wait(TimeSpan.FromSeconds(30));
            if (!disposing) return;

            _cancellationTokenSource.Dispose();
            try
            {
                _httpListener.Close();

                _diagnosticService.Emit(
                    new HttpEventBuilder(this, HttpEventType.HttpServerStopped)
                    {
                        Uri = _baseUri
                    }.Build());
            }
            catch (Exception ex)
            {
                _diagnosticService.Emit(
                    new HttpEventBuilder(this, HttpEventType.HttpServerError)
                    {
                        Detail = "Unexpected error closing HTTP listener",
                        Uri = _baseUri,
                        Exception = ex
                    }.Build());
                _httpListener.Abort();
            }
            
            _bus.Dispose();
            TransportService.Dispose();

            if (_messageQueueingService is IDisposable disposableMessageQueueingService)
            {
                disposableMessageQueueingService.Dispose();
            }

            if (_messageJournal is IDisposable disposableMessageJournal)
            {
                disposableMessageJournal.Dispose();
            }

            if (_subscriptionTrackingService is IDisposable disposableSubscriptionTrackingService)
            {
                disposableSubscriptionTrackingService.Dispose();
            }

            _metricsCollector.Dispose();
        }
    }
}