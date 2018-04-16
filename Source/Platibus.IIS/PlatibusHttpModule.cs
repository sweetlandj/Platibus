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
using Platibus.Http;

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
        private readonly IBus _bus;
        private readonly HttpTransportService _transportService;
        private bool _disposed;

        public IBus Bus => _bus;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.IIS.PlatibusHttpModule" /> with the default configuration
        /// using any configuration hooks present in the app domain assemblies
        /// </summary>
        public PlatibusHttpModule()
            : this(IISConfigurationCache.SingletonInstance.GetConfiguration(null))
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.IIS.PlatibusHttpModule" /> with the default configuration
        /// using any configuration hooks present in the app domain assemblies
        /// </summary>
        public PlatibusHttpModule(string sectionName)
            : this(IISConfigurationCache.SingletonInstance.GetConfiguration(sectionName))
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.IIS.PlatibusHttpModule" /> with the default configuration
        /// using any configuration hooks present in the app domain assemblies
        /// </summary>
        public PlatibusHttpModule(string sectionName, Action<IIISConfiguration> configure)
            : this(IISConfigurationCache.SingletonInstance.GetConfiguration(sectionName, configure))
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.IIS.PlatibusHttpModule" /> with the default configuration
        /// using any configuration hooks present in the app domain assemblies
        /// </summary>
        public PlatibusHttpModule(string sectionName, Func<IIISConfiguration, Task> configure)
            : this(IISConfigurationCache.SingletonInstance.GetConfiguration(sectionName, configure))
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.IIS.PlatibusHttpModule" /> with the default configuration
        /// using any configuration hooks present in the app domain assemblies
        /// </summary>
        public PlatibusHttpModule(Action<IIISConfiguration> configure)
            : this(IISConfigurationCache.SingletonInstance.GetConfiguration(null, configure))
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.IIS.PlatibusHttpModule" /> with the default configuration
        /// using any configuration hooks present in the app domain assemblies
        /// </summary>
        public PlatibusHttpModule(Func<IIISConfiguration, Task> configure)
            : this(IISConfigurationCache.SingletonInstance.GetConfiguration(null, configure))
        {
        }
        
        /// <summary>
        /// Initializes a new <see cref="T:Platibus.IIS.PlatibusHttpModule" /> with the specified configuration
        /// and any configuration hooks present in the app domain assemblies
        /// </summary>
        public PlatibusHttpModule(IIISConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            var managedBus = BusManager.SingletonInstance.GetManagedBus(_configuration);
            _bus = managedBus.Bus;
            _transportService = managedBus.TransportService;
        }
        
        /// <inheritdoc />
        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication" /> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application </param>
        public void Init(HttpApplication context)
        {
            context.BeginRequest += OnBeginRequest;
            context.PostMapRequestHandler += OnPostMapRequestHandler;
        }

        private void OnBeginRequest(object source, EventArgs args)
        {
            var application = (HttpApplication)source;
            var context = application.Context;
            context.SetBus(_bus);
        }
        
        private void OnPostMapRequestHandler(object source, EventArgs args)
		{
            var application = (HttpApplication)source;
            var context = application.Context;
            var request = context.Request;
            var baseUri = _configuration.BaseUri;
            if (IsPlatibusUri(request.Url, baseUri))
            {
                context.Handler = new PlatibusHttpHandler(_configuration, _bus, _transportService);
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
