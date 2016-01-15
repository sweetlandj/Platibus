// The MIT License (MIT)
// 
// Copyright (c) 2015 Jesse Sweetland
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
using Common.Logging;
using Platibus.Http;

namespace Platibus.IIS
{
    /// <summary>
    /// HTTP handler used to handle requests in IIS
    /// </summary>
    public class PlatibusHttpHandler : HttpTaskAsyncHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(IISLoggingCategories.IIS);

        private readonly Task<IHttpResourceRouter> _resourceRouter;
        
        /// <summary>
        /// When overridden in a derived class, gets a value that indicates whether the task handler class instance can be reused for another asynchronous task.
        /// </summary>
        /// <returns>
        /// true if the handler can be reused; otherwise, false.  The default is false.
        /// </returns>
        public override bool IsReusable
        {
            get { return true; }
        }

        public PlatibusHttpHandler()
        {
            _resourceRouter = BusManager.SingletonInstance.GetResourceRouter();
        }

        public PlatibusHttpHandler(IHttpResourceRouter resourceRouter)
        {
            _resourceRouter = Task.FromResult(resourceRouter);
        }

        public PlatibusHttpHandler(Task<IHttpResourceRouter> resourceRouter)
        {
            _resourceRouter = resourceRouter;
        }

        /// <summary>
        /// When overridden in a derived class, provides code that handles an asynchronous task.
        /// </summary>
        /// <returns>
        /// The asynchronous task.
        /// </returns>
        /// <param name="context">The HTTP context.</param>
        public override Task ProcessRequestAsync(HttpContext context)
        {
            return ProcessRequestAsync(new HttpContextWrapper(context));
        }

        private async Task ProcessRequestAsync(HttpContextBase context)
        {
            Log.DebugFormat("Processing {0} request for resource {1}...",
                context.Request.HttpMethod, context.Request.Url);

            var resourceRequest = new HttpRequestAdapter(context.Request, context.User);
            var resourceResponse = new HttpResponseAdapter(context.Response);
            try
            {
                var resourceRouter = await _resourceRouter;
                await resourceRouter.Route(resourceRequest, resourceResponse);

                Log.DebugFormat("{0} request for resource {1} processed successfully",
                context.Request.HttpMethod, context.Request.Url);
            }
            catch (Exception ex)
            {
                var exceptionHandler = new HttpExceptionHandler(resourceRequest, resourceResponse, Log);
                exceptionHandler.HandleException(ex);
            }
        }
    }
}