
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Platibus.Http;

namespace Platibus.IIS
{
	/// <summary>
	/// HTTP module for routing Platibus resource requests
	/// </summary>
	public class PlatibusHttpModule : IHttpModule
	{
		private static int _initCount;

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
		public PlatibusHttpModule()
		{
		}

		/// <summary>
		/// Initializes a new <see cref="PlatibusHttpModule"/> with the specified configuration
		/// and any configuration hooks present in the app domain assemblies
		/// </summary>
		public PlatibusHttpModule(IISConfiguration configuration)
		{
			_configuration = configuration;
		}

		public void Init(HttpApplication context)
		{
			var eventHandler = new EventHandlerTaskAsyncHelper(OnMapRequestHandlerAsync);
			context.AddOnMapRequestHandlerAsync(eventHandler.BeginEventHandler, eventHandler.EndEventHandler);

			if (Interlocked.Increment(ref _initCount) > 1) return;
			
			Task.Run(async () => await InitAsync()).Wait();
		}

		private static async Task InitAsync()
		{
			if (_configuration == null)
			{
				_configuration = await IISConfigurationManager.LoadConfiguration();
			}

			_baseUri = _configuration.BaseUri;
			_subscriptionTrackingService = _configuration.SubscriptionTrackingService;
			_messageQueueingService = _configuration.MessageQueueingService;
			_messageJournalingService = _configuration.MessageJournalingService;
			var endpoints = _configuration.Endpoints;
			_transportService = new HttpTransportService(_baseUri, endpoints, _messageQueueingService, _messageJournalingService, _subscriptionTrackingService);
			_bus = new Bus(_configuration, _baseUri, _transportService, _messageQueueingService);
			await _transportService.Init();
			await _bus.Init();

			var authorizationService = _configuration.AuthorizationService;
			_resourceRouter = new ResourceTypeDictionaryRouter
            {
                {"message", new MessageController(_bus.HandleMessage, authorizationService)},
                {"topic", new TopicController(_subscriptionTrackingService, _configuration.Topics, authorizationService)}
            };
		}

		private async Task OnMapRequestHandlerAsync(object source, EventArgs args)
		{
			var application = (HttpApplication)source;
			var context = application.Context;
			var request = context.Request;
			if (IsPlatibusUri(request.Url))
			{
				var response = context.Response;
				var resourceRequest = new HttpRequestAdapter(new HttpRequestWrapper(request), context.User);
				var resourceResponse = new HttpResponseAdapter(new HttpResponseWrapper(response));
				await _resourceRouter.Route(resourceRequest, resourceResponse);
				response.End();
			}
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
			if (Interlocked.Decrement(ref _initCount) > 0) return;

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
