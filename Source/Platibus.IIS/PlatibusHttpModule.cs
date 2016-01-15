
using System;
using System.Threading.Tasks;
using System.Web;
using Common.Logging;
using Platibus.Http;

namespace Platibus.IIS
{
    /// <summary>
    /// HTTP module for routing Platibus resource requests
    /// </summary>
    public class PlatibusHttpModule : IHttpModule
    {
        private static readonly ILog Log = LogManager.GetLogger(IISLoggingCategories.IIS);

        private static readonly object SyncRoot = new object();
	    private static volatile Task _initialization;

		private static IIISConfiguration _configuration;
		private static Uri _baseUri;
		private static ISubscriptionTrackingService _subscriptionTrackingService;
		private static IMessageQueueingService _messageQueueingService;
		private static IMessageJournalingService _messageJournalingService;
		private static HttpTransportService _transportService;
		private static Bus _bus;
		private static IHttpResourceRouter _resourceRouter;
		private static bool _disposed;

		/// <summary>
		/// Initializes a new <see cref="PlatibusHttpModule"/> with the default configuration
		/// using any configuration hooks present in the app domain assemblies
		/// </summary>
		public PlatibusHttpModule() : this(null)
		{
		}

		/// <summary>
		/// Initializes a new <see cref="PlatibusHttpModule"/> with the specified configuration
		/// and any configuration hooks present in the app domain assemblies
		/// </summary>
		public PlatibusHttpModule(IIISConfiguration configuration)
		{
			_configuration = configuration;
        }

	    /// <summary>
	    /// Initializes a module and prepares it to handle requests.
	    /// </summary>
	    /// <param name="context">An <see cref="T:System.Web.HttpApplication"/> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application </param>
	    public void Init(HttpApplication context)
		{
            Log.DebugFormat("Initializing Platibus HTTP module...");
			var eventHandler = new EventHandlerTaskAsyncHelper(OnPostMapRequestHandlerAsync);
			context.AddOnPostMapRequestHandlerAsync(eventHandler.BeginEventHandler, eventHandler.EndEventHandler);

            if (_initialization == null)
            {
                lock (SyncRoot)
                {
                    if (_initialization == null)
                    {
                        _initialization = InitAsync();
                    }
                }
            }

            Log.DebugFormat("Platibus HTTP module initialized successfully");
        }

		private static async Task InitAsync()
		{
            Log.DebugFormat("Initializing Platibus components...");

            if (_configuration == null)
			{
                Log.DebugFormat("Loading default IIS configuration...");
                _configuration = await IISConfigurationManager.LoadConfiguration();
			}

			_baseUri = _configuration.BaseUri;
			_subscriptionTrackingService = _configuration.SubscriptionTrackingService;
			_messageQueueingService = _configuration.MessageQueueingService;
			_messageJournalingService = _configuration.MessageJournalingService;
			var endpoints = _configuration.Endpoints;
			_transportService = new HttpTransportService(_baseUri, endpoints, _messageQueueingService, _messageJournalingService, _subscriptionTrackingService);
			_bus = new Bus(_configuration, _baseUri, _transportService, _messageQueueingService);

            Log.DebugFormat("Initializing HTTP transport service...");
            await _transportService.Init();

            Log.DebugFormat("Initializing bus...");
            await _bus.Init();

            Log.DebugFormat("Initializing HTTP resource router...");
            var authorizationService = _configuration.AuthorizationService;
			_resourceRouter = new ResourceTypeDictionaryRouter
            {
                {"message", new MessageController(_bus.HandleMessage, authorizationService)},
                {"topic", new TopicController(_subscriptionTrackingService, _configuration.Topics, authorizationService)}
            };

            Log.DebugFormat("Platibus components initialized successfully");
        }

		private static async Task OnPostMapRequestHandlerAsync(object source, EventArgs args)
		{
            var application = (HttpApplication)source;
            var context = application.Context;
            var request = context.Request;
            if (!IsPlatibusUri(request.Url)) return;

            Log.DebugFormat("Detected {0} request for Platibus resource {1}...",
                context.Request.HttpMethod, context.Request.Url);

            await _initialization;
            context.Handler = new PlatibusHttpHandler(_resourceRouter);
		}

		private static bool IsPlatibusUri(Uri uri)
		{
			var baseUriPath = _baseUri.AbsolutePath.Trim().ToLower();
			var uriPath = uri.AbsolutePath.Trim().TrimEnd('/').ToLower();
			return uriPath.StartsWith(baseUriPath);
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
