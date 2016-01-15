
using System;
using System.Threading.Tasks;
using System.Web;
using Common.Logging;

namespace Platibus.IIS
{
    /// <summary>
    /// HTTP module for routing Platibus resource requests
    /// </summary>
    public class PlatibusHttpModule : IHttpModule
    {
        private static readonly ILog Log = LogManager.GetLogger(IISLoggingCategories.IIS);

        private readonly Task<IIISConfiguration> _configuration;
        private readonly Task<Bus> _bus;
        private bool _disposed;
        
		/// <summary>
		/// Initializes a new <see cref="PlatibusHttpModule"/> with the default configuration
		/// using any configuration hooks present in the app domain assemblies
		/// </summary>
		public PlatibusHttpModule()
		{
		    _configuration = LoadDefaultConfiguration();
            _bus = InitBus(_configuration);
		}

		/// <summary>
		/// Initializes a new <see cref="PlatibusHttpModule"/> with the specified configuration
		/// and any configuration hooks present in the app domain assemblies
		/// </summary>
		public PlatibusHttpModule(IIISConfiguration configuration)
		{
		    if (configuration == null) throw new ArgumentNullException("configuration");
		    _configuration = Task.FromResult(configuration);
		    _bus = InitBus(configuration);
		}

        private static async Task<IIISConfiguration> LoadDefaultConfiguration()
        {
            return await IISConfigurationManager.LoadConfiguration();
        }

        private static async Task<Bus> InitBus(Task<IIISConfiguration> configuration)
        {
            return await InitBus(await configuration);
        }

        private static async Task<Bus> InitBus(IIISConfiguration configuration)
        {
            var managedBus = await BusManager.SingletonInstance.GetManagedBus(configuration);
            return await managedBus.GetBus();
        }

        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication"/> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application </param>
        public void Init(HttpApplication context)
		{
            Log.DebugFormat("Initializing Platibus HTTP module...");

            Log.DebugFormat("Registering event handler for begin-request event...");
            var beginRequest = new EventHandlerTaskAsyncHelper(OnBeginRequest);
            context.AddOnBeginRequestAsync(beginRequest.BeginEventHandler, beginRequest.EndEventHandler);
            
            Log.DebugFormat("Registering event handler for post-map-request-handler event...");
            var postMapRequestHandler = new EventHandlerTaskAsyncHelper(OnPostMapRequestHandler);
            context.AddOnPostMapRequestHandlerAsync(postMapRequestHandler.BeginEventHandler, postMapRequestHandler.EndEventHandler);

            Log.DebugFormat("Platibus HTTP module initialized successfully");
        }

        private async Task OnBeginRequest(object source, EventArgs args)
        {
            var application = (HttpApplication)source;
            var context = application.Context;
            var bus = await _bus;
            context.SetBus(bus);
        }
        
        private async Task OnPostMapRequestHandler(object source, EventArgs args)
		{
            var application = (HttpApplication)source;
            var context = application.Context;
            var request = context.Request;

            var configuration = await _configuration;
            var baseUri = configuration.BaseUri;
            if (IsPlatibusUri(request.Url, baseUri))
            {
                var bus = context.GetBus() as Bus ?? await _bus;
                context.Items["Platibus.Bus"] = bus;
                context.Handler = new PlatibusHttpHandler(bus, configuration);
            }
		}

        private static bool IsPlatibusUri(Uri uri, Uri baseUri)
        {
            var baseUriPath = baseUri.AbsolutePath.ToLower();
            var uriPath = uri.AbsolutePath.ToLower();
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
			    
			}
		}
	}
}
