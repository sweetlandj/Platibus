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
using System.Threading.Tasks;
using System.Web;
using Platibus.Diagnostics;

namespace Platibus.IIS
{
    /// <summary>
    /// HTTP module for routing Platibus resource requests
    /// </summary>
    public class PlatibusHttpModule : IHttpModule
    {
        private readonly Task<IIISConfiguration> _configuration;
        private readonly Task<Bus> _bus;

        private bool _disposed;
        
		/// <summary>
		/// Initializes a new <see cref="PlatibusHttpModule"/> with the default configuration
		/// using any configuration hooks present in the app domain assemblies
		/// </summary>
		public PlatibusHttpModule()
		{
		    _configuration = LoadConfiguration();
            _bus = InitBus(_configuration);
        }

        /// <summary>
        /// Initializes a new <see cref="PlatibusHttpModule"/> with the default configuration
        /// using any configuration hooks present in the app domain assemblies
        /// </summary>
        /// <param name="configurationSectionName">(Optional) The name of the configuration
        /// section from which the bus configuration should be loaded</param>
        /// <param name="diagnosticEventSink">(Optional) A data sink provided by the implementer
        /// to handle diagnostic events</param>
        public PlatibusHttpModule(string configurationSectionName = null, IDiagnosticEventSink diagnosticEventSink = null)
        {
            _configuration = LoadConfiguration(configurationSectionName, diagnosticEventSink);
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

        private static async Task<IIISConfiguration> LoadConfiguration(string sectionName = null, IDiagnosticEventSink diagnosticEventSink = null)
        {
            var configuration = new IISConfiguration
            {
                DiagnosticEventSink = diagnosticEventSink
            };
            var configManager = new IISConfigurationManager(diagnosticEventSink);
            await configManager.Initialize(configuration, sectionName);
            await configManager.FindAndProcessConfigurationHooks(configuration);
            return configuration;
        }
        
        private async Task<Bus> InitBus(Task<IIISConfiguration> configuration)
        {
            return await InitBus(await configuration);
        }

        private async Task<Bus> InitBus(IIISConfiguration configuration)
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
            var beginRequest = new EventHandlerTaskAsyncHelper(OnBeginRequest);
            context.AddOnBeginRequestAsync(beginRequest.BeginEventHandler, beginRequest.EndEventHandler);
            
            var postMapRequestHandler = new EventHandlerTaskAsyncHelper(OnPostMapRequestHandler);
            context.AddOnPostMapRequestHandlerAsync(postMapRequestHandler.BeginEventHandler, postMapRequestHandler.EndEventHandler);
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
		}
	}
}
