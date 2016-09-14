using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Microsoft.Owin;
using Platibus.Http;
using Platibus.Security;

namespace Platibus.Owin
{
    public class PlatibusMiddleware : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.Owin);

        private readonly Task<IOwinConfiguration> _configuration;
        private readonly Task<Bus> _bus;
        private readonly Task<IHttpResourceRouter> _resourceRouter;

        private HttpTransportService _transportService;
        private ISubscriptionTrackingService _subscriptionTrackingService;
        private IMessageQueueingService _messageQueueingService;
        private IMessageJournalingService _messageJournalingService;

        private bool _disposed;

        public Task<IOwinConfiguration> Configuration { get { return _configuration; } }
        public Task<Bus> Bus { get { return _bus; } }

        public PlatibusMiddleware(string sectionName = null) : this(LoadDefaultConfiguration(sectionName))
        {
        }

        public PlatibusMiddleware(IOwinConfiguration configuration) : this(Task.FromResult(configuration))
        {
        }

        public PlatibusMiddleware(Task<IOwinConfiguration> configuration)
        {
            _configuration = configuration;
            _bus = InitBus();
            _resourceRouter = InitResourceRouter();
        }

        public async Task Invoke(IOwinContext context, Func<Task> next)
        {
            var configuration = await _configuration;
            var baseUri = configuration.BaseUri;
            if (IsPlatibusUri(context.Request.Uri, baseUri))
            {
                await HandlePlatibusRequest(context);
            }
            else if (next != null)
            {
                await next();
            }
        }

        private async Task HandlePlatibusRequest(IOwinContext context)
        {
            var resourceRequest = new OwinRequestAdapter(context.Request);
            var resourceResponse = new OwinResponseAdapter(context.Response);
            try
            {
                var bus = await _bus;
                context.SetBus(bus);
                var router = await _resourceRouter;
                await router.Route(resourceRequest, resourceResponse);
            }
            catch (Exception ex)
            {
                var exceptionHandler = new HttpExceptionHandler(resourceRequest, resourceResponse, Log);
                exceptionHandler.HandleException(ex);
            }
        }

        private static async Task<IOwinConfiguration> LoadDefaultConfiguration(string sectionName)
        {
            return await OwinConfigurationManager.LoadConfiguration(sectionName);
        }

        private async Task<Bus> InitBus()
        {
            return await InitBus(await _configuration);
        }

        private async Task<Bus> InitBus(IOwinConfiguration configuration)
        {
            var baseUri = configuration.BaseUri;
            _subscriptionTrackingService = configuration.SubscriptionTrackingService;
            _messageQueueingService = configuration.MessageQueueingService;
            _messageJournalingService = configuration.MessageJournalingService;

            var endpoints = configuration.Endpoints;
            _transportService = new HttpTransportService(baseUri, endpoints, _messageQueueingService,
                _messageJournalingService, _subscriptionTrackingService,
                configuration.BypassTransportLocalDestination, HandleMessage);

            var bus = new Bus(configuration, baseUri, _transportService, _messageQueueingService);

            await _transportService.Init();
            await bus.Init();

            return bus;
        }

        private async Task<IHttpResourceRouter> InitResourceRouter()
        {
            var configuration = await _configuration;
            var bus = await _bus;
            var authorizationService = configuration.AuthorizationService;
            var subscriptionTrackingService = configuration.SubscriptionTrackingService;
            return new ResourceTypeDictionaryRouter
            {
                {"message", new MessageController(bus.HandleMessage, authorizationService)},
                {"topic", new TopicController(subscriptionTrackingService, configuration.Topics, authorizationService)}
            };
        }
        
        private async Task HandleMessage(Message message, CancellationToken cancellationToken)
        {
            var bus = await _bus;
            var senderPrincipal = SenderPrincipal.From(Thread.CurrentPrincipal);
            await bus.HandleMessage(message, senderPrincipal);
        }

        private static bool IsPlatibusUri(Uri uri, Uri baseUri)
        {
            var baseUriPath = baseUri.AbsolutePath.ToLower();
            var uriPath = uri.AbsolutePath.ToLower();
            return uriPath.StartsWith(baseUriPath);
        }

        /// <summary>
        /// Finalizer that ensures resources are released
        /// </summary>
        ~PlatibusMiddleware()
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
            if (disposing)
            {
                _bus.TryDispose();
                _transportService.TryDispose();
                _messageQueueingService.TryDispose();
                _messageJournalingService.TryDispose();
                _subscriptionTrackingService.TryDispose();
            }
        }
    }
}
