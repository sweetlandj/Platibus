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

using Common.Logging;
using Platibus.Config;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.Http
{
    public class HttpServer : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.Http);
        private bool _disposed;
        private readonly Uri _baseUri;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly HttpListener _httpListener;
        private readonly IHttpResourceRouter _router;

        public HttpServer(string sectionName = null)
        {
            var bus = Bootstrapper.InitBus(sectionName).ConfigureAwait(false).GetAwaiter().GetResult();
            Shutdown += (source, e) => bus.Dispose();

            _router = InitDefaultRouter(bus);
            _baseUri = bus.BaseUri;
            _httpListener = InitHttpListener();
            _httpListener.Prefixes.Add(_baseUri.ToString());
        }

        public HttpServer(Bus bus)
        {
            if (bus == null) throw new ArgumentNullException("bus");

            _router = InitDefaultRouter(bus);
            _baseUri = bus.BaseUri;
            _httpListener = InitHttpListener();
            _httpListener.Prefixes.Add(_baseUri.ToString());
        }

        public HttpServer(Uri baseUri, IHttpResourceRouter router)
        {
            if (router == null) throw new ArgumentNullException("router");
            if (baseUri == null)
            {
                baseUri = new Uri("http://localhost:52180/platibus/");
            }

            _router = router;
            _baseUri = baseUri;
            _httpListener = InitHttpListener();
            _httpListener.Prefixes.Add(baseUri.ToString());
        }

        public Uri BaseUri
        {
            get { return _baseUri; }
        }

        public event HttpServerShutdownHandler Shutdown;

        private static IHttpResourceRouter InitDefaultRouter(Bus bus)
        {
            return new ResourceTypeDictionaryRouter
            {
                {"message", new MessageController(bus.TransportService)},
                {"topic", new TopicController(bus.TransportService, bus.Topics)}
            };
        }

        private static HttpListener InitHttpListener()
        {
            var httpListener = new HttpListener();
            return httpListener;
        }

        public void Start()
        {
            if (_httpListener.IsListening) return;

            Log.InfoFormat("Starting HTTP server listening on {0}...", _baseUri);
            _httpListener.Start();
            Log.InfoFormat("HTTP started successfully");

            // Create a new async task but do not wait for it to complete.

            // ReSharper disable once UnusedVariable
            var listenTask = Listen(_cancellationTokenSource.Token);
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private async Task Listen(CancellationToken cancellationToken = default(CancellationToken))
        {
            while (_httpListener.IsListening)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var context = await _httpListener.GetContextAsync().ConfigureAwait(false);

                Log.DebugFormat("Accepting {0} request for resource {1} from {2}...",
                    context.Request.HttpMethod, context.Request.Url, context.Request.RemoteEndPoint);

                // Create a new async task but do not wait for it to complete.

                // ReSharper disable once UnusedVariable
                var acceptTask = Accept(context);
            }
        }

        protected async Task Accept(HttpListenerContext context)
        {
            try
            {
                var resourceRequest = new HttpListenerRequestAdapter(context.Request, context.User);
                var resourceResponse = new HttpListenerResponseAdapter(context.Response);

                Log.DebugFormat("Routing {0} request for resource {1} from {2}...",
                    context.Request.HttpMethod, context.Request.Url, context.Request.RemoteEndPoint);

                await _router.Route(resourceRequest, resourceResponse).ConfigureAwait(false);

                Log.DebugFormat("{0} request for resource {1} handled successfully",
                    context.Request.HttpMethod, context.Request.Url);
            }
            catch (Exception e)
            {
                context.Response.StatusCode = 500;
                Func<Exception, bool> handler = ex =>
                {
                    Log.ErrorFormat("Error processing {0} request for resource {1}", ex, context.Request.HttpMethod,
                        context.Request.Url);
                    return true;
                };

                var aex = e as AggregateException;
                if (aex != null)
                {
                    aex.Handle(handler);
                }
                else
                {
                    handler(e);
                }
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

        protected virtual void Dispose(bool disposing)
        {
            _cancellationTokenSource.Cancel();

            Log.Info("Stopping HTTP server...");
            _httpListener.Stop();
            try
            {
                _httpListener.Close();
            }
            catch (Exception ex)
            {
                Log.Warn("Error closing HTTP listener; aborting...", ex);
                _httpListener.Abort();
            }
            Log.InfoFormat("HTTP server stopped");

            var shutdownHandlers = Shutdown;
            if (shutdownHandlers != null)
            {
                try
                {
                    shutdownHandlers(this, new HttpServerShutdownEventArgs());
                }
                catch (Exception e)
                {
                    Log.Warn("Unhandled exception invoking HTTP server shutdown handlers during dispose", e);
                }
            }
            _cancellationTokenSource.Dispose();
        }
    }
}