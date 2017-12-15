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

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Platibus.Diagnostics;
using Platibus.Http;
using System;
using System.Threading.Tasks;

namespace Platibus.AspNetCore
{
    public class PlatibusMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDiagnosticService _diagnosticService;
        private readonly IHttpResourceRouter _resourceRouter;

        public PlatibusMiddleware(RequestDelegate next, IHttpResourceRouter resourceRouter, IDiagnosticService diagnosticService)
        {
            _next = next;
            _diagnosticService = diagnosticService ?? DiagnosticService.DefaultInstance;
            _resourceRouter = resourceRouter ?? throw new ArgumentNullException(nameof(resourceRouter));
        }
        
        public async Task Invoke(HttpContext context)
        {
            if (IsPlatibusUri(context.Request.GetEncodedUrl()))
            {
                await HandlePlatibusRequest(context);
            }
            else if (_next != null)
            {
                await _next(context);
            }
        }

        private async Task HandlePlatibusRequest(HttpContext context)
        {
            await _diagnosticService.EmitAsync(
                new HttpEventBuilder(this, HttpEventType.HttpRequestReceived)
                {
                    Remote = context.Connection.RemoteIpAddress?.ToString(),
                    Uri = context.Request.GetUri(),
                    Method = context.Request.Method
                }.Build());

            var resourceRequest = new HttpRequestAdapter(context.Request);
            var resourceResponse = new HttpResponseAdapter(context.Response);

            try
            {
                await _resourceRouter.Route(resourceRequest, resourceResponse);
            }
            catch (Exception ex)
            {
                var exceptionHandler = new HttpExceptionHandler(resourceRequest, resourceResponse, _diagnosticService);
                exceptionHandler.HandleException(ex);
            }

            await _diagnosticService.EmitAsync(
                new HttpEventBuilder(this, HttpEventType.HttpResponseSent)
                {
                    Remote = context.Connection.RemoteIpAddress?.ToString(),
                    Uri = context.Request.GetUri(),
                    Method = context.Request.Method,
                    Status = context.Response.StatusCode
                }.Build());
        }
        
        private bool IsPlatibusUri(string uri)
        {
            return !string.IsNullOrWhiteSpace(uri) && IsPlatibusUri(new Uri(uri));
        }

        private bool IsPlatibusUri(Uri uri)
        {
            return _resourceRouter.IsRoutable(uri);
        }
    }
}
