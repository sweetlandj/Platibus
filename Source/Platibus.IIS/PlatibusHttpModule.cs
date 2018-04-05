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
using Platibus.Utils;

namespace Platibus.IIS
{
    /// <inheritdoc cref="IHttpModule" />
    /// <inheritdoc cref="IDisposable" />
    /// <summary>
    /// HTTP module for routing Platibus resource requests
    /// </summary>
    public class PlatibusHttpModule : IHttpModule, IDisposable
    {
        private readonly IIISConfiguration _configuration;
        private readonly Lazy<Task<Bus>> _bus;
        private bool _disposed;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.IIS.PlatibusHttpModule" /> with the default configuration
        /// using any configuration hooks present in the app domain assemblies
        /// </summary>
        public PlatibusHttpModule()
            : this(LoadConfiguration(null))
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.IIS.PlatibusHttpModule" /> with the default configuration
        /// using any configuration hooks present in the app domain assemblies
        /// </summary>
        public PlatibusHttpModule(string sectionName)
            : this(LoadConfiguration(sectionName))
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.IIS.PlatibusHttpModule" /> with the default configuration
        /// using any configuration hooks present in the app domain assemblies
        /// </summary>
        public PlatibusHttpModule(string sectionName, Action<IIISConfiguration> configure)
            : this(LoadConfiguration(sectionName, configure))
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.IIS.PlatibusHttpModule" /> with the default configuration
        /// using any configuration hooks present in the app domain assemblies
        /// </summary>
        public PlatibusHttpModule(string sectionName, Func<IIISConfiguration, Task> configure)
            : this(LoadConfiguration(sectionName, configure))
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.IIS.PlatibusHttpModule" /> with the default configuration
        /// using any configuration hooks present in the app domain assemblies
        /// </summary>
        public PlatibusHttpModule(Action<IIISConfiguration> configure)
            : this(LoadConfiguration(null, configure))
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.IIS.PlatibusHttpModule" /> with the default configuration
        /// using any configuration hooks present in the app domain assemblies
        /// </summary>
        public PlatibusHttpModule(Func<IIISConfiguration, Task> configure)
            : this(LoadConfiguration(null, configure))
        {
        }
        
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.IIS.PlatibusHttpModule" /> with the specified configuration
        /// and any configuration hooks present in the app domain assemblies
        /// </summary>
        public PlatibusHttpModule(IIISConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _bus = new Lazy<Task<Bus>>(InitBusAsync);
        }

        private static IIISConfiguration LoadConfiguration(string sectionName)
        {
            var configuration = LoadConfigurationAsync(sectionName).GetResultUsingContinuation();
            return configuration;
        }
        
        private static IIISConfiguration LoadConfiguration(string sectionName, Action<IIISConfiguration> configure)
        {
            var configuration = LoadConfigurationAsync(sectionName).GetResultUsingContinuation();
            configure?.Invoke(configuration);
            return configuration;
        }

        private static IIISConfiguration LoadConfiguration(string sectionName, Func<IIISConfiguration, Task> configure)
        {
            return LoadConfigurationAsync(sectionName, configure).GetResultUsingContinuation();
        }

        private static async Task<IIISConfiguration> LoadConfigurationAsync(string sectionName = null, Func<IIISConfiguration, Task> configure = null)
        {
            var configuration = new IISConfiguration();
            var configManager = new IISConfigurationManager();
            await configManager.Initialize(configuration, sectionName);
            await configManager.FindAndProcessConfigurationHooks(configuration);
            if (configure != null)
            {
                await configure(configuration);
            }
            return configuration;
        }
        
        private async Task<Bus> InitBusAsync()
        {
            var managedBus = await BusManager.SingletonInstance.GetManagedBus(_configuration);
            return await managedBus.GetBus();
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication" /> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application </param>
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
            var bus = await _bus.Value;
            context.SetBus(bus);
        }
        
        private async Task OnPostMapRequestHandler(object source, EventArgs args)
		{
            var application = (HttpApplication)source;
            var context = application.Context;
            var request = context.Request;
            var baseUri = _configuration.BaseUri;
            if (IsPlatibusUri(request.Url, baseUri))
            {
                var bus = context.GetBus() as Bus ?? await _bus.Value;
                context.Handler = new PlatibusHttpHandler(bus, _configuration);
            }
		}

        private static bool IsPlatibusUri(Uri uri, Uri baseUri)
        {
            var baseUriPath = baseUri.AbsolutePath.ToLower();
            var uriPath = uri.AbsolutePath.ToLower();
            return uriPath.StartsWith(baseUriPath);
        }
        
		/// <inheritdoc cref="IDisposable.Dispose"/>
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
