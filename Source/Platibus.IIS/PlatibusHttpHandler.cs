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

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Common.Logging;
using Platibus.Http;

namespace Platibus.IIS
{
    public class PlatibusHttpHandler : HttpTaskAsyncHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(IISLoggingCategories.IIS);

        private readonly Guid _instanceId;

        public override bool IsReusable
        {
            get { return true; }
        }

        public PlatibusHttpHandler()
        {
            _instanceId = Guid.NewGuid();
        }

        public override Task ProcessRequestAsync(HttpContext context)
        {
            return ProcessRequestAsync(new HttpContextWrapper(context));
        }

        public async Task ProcessRequestAsync(HttpContextBase context)
        {
            Log.DebugFormat("[Process {0}, Thread {1}, AppDomain {2}]",
                Process.GetCurrentProcess().Id,
                Thread.CurrentThread.ManagedThreadId,
                AppDomain.CurrentDomain.Id);

            Log.DebugFormat("Processing {0} request for resource {1} (HTTP handler instance: {2})...", context.Request.HttpMethod, context.Request.Url, _instanceId);
            var resourceRequest = new HttpRequestAdapter(context.Request, context.User);
            var resourceResponse = new HttpResponseAdapter(context.Response);
            try
            {
                var router = await GetRouter().ConfigureAwait(false);
                await router.Route(resourceRequest, resourceResponse).ConfigureAwait(false);
                Log.DebugFormat("Processing {0} request for resource {1} (HTTP handler instance: {2})...", context.Request.HttpMethod, context.Request.Url, _instanceId);
            }
            catch (Exception ex)
            {
                var exceptionHandler = new HttpExceptionHandler(resourceRequest, resourceResponse, Log);
                exceptionHandler.HandleException(ex);
            }
        }

        protected virtual async Task<IHttpResourceRouter> GetRouter()
        {
            var bus = await BusManager.GetInstance();
            var transportService = (HttpTransportService)bus.TransportService;
            return new ResourceTypeDictionaryRouter 
            {
                {"message", new MessageController(transportService)},
                {"topic", new TopicController(transportService, bus.Topics)}
            };
        }
    }
}
