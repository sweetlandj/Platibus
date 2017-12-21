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
using Platibus.Diagnostics;

namespace Platibus.Http
{
    /// <summary>
    /// Updates an HTTP response with the HTTP status code that is most appopriate for
    /// a given exception
    /// </summary>
    public class HttpExceptionHandler
    {
        private readonly IHttpResourceRequest _request;
        private readonly IHttpResourceResponse _response;
        private readonly IDiagnosticService _diagnosticService;
        private readonly object _source;

        /// <summary>
        /// Initializes a new <see cref="HttpExceptionHandler"/> for the specified HTTP 
        /// <paramref name="request"/> and <paramref name="response"/>
        /// </summary>
        /// <param name="request">The HTTP request being processed</param>
        /// <param name="response">The HTTP response being constructed</param>
        /// <param name="source">(Optional) The object in which the exception occurred</param>
        /// <param name="diagnosticService">(Optional) The service through which diagnostic events
        /// are reported and processed</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="request"/> or
        /// <paramref name="response"/> are <c>null</c></exception>
        public HttpExceptionHandler(IHttpResourceRequest request, IHttpResourceResponse response, IDiagnosticService diagnosticService, object source = null)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _response = response ?? throw new ArgumentNullException(nameof(response));
            _diagnosticService = diagnosticService ?? DiagnosticService.DefaultInstance;
            _source = source ?? this;
        }   

        /// <summary>
        /// Handles an exception by recording exception details in the log and updating
        /// the HTTP response with the appropriate status code
        /// </summary>
        /// <param name="ex">The exception to handle</param>
        /// <returns>Returns <c>true</c> if the exception was successfully handled;
        /// <c>false</c> otherwise</returns>
        /// <remarks>
        /// This method always returns <c>true</c>.  The return value is provided in order to
        /// match the signature of the <see cref="AggregateException.Handle"/> method.
        /// </remarks>
        public bool HandleException(Exception ex)
        {
            if (ex is AggregateException aggregateException)
            {
                aggregateException.Handle(HandleException);
                return true;
            }

            if (ex is UnauthorizedAccessException unauthorizedAccessException)
            {
                _response.StatusCode = 401;
                _diagnosticService.Emit(new HttpEventBuilder(_source, DiagnosticEventType.AccessDenied)
                {
                    Detail = "Unauthorized",
                    Uri = _request.Url,
                    Method = _request.HttpMethod,
                    Status = 401,
                    Exception = unauthorizedAccessException
                }.Build());
                return true;
            }

            if (ex is MessageNotAcknowledgedException notAcknowledgedException)
            {
                // HTTP 422: Unprocessable Entity
                _response.StatusCode = 422;
                _diagnosticService.Emit(new HttpEventBuilder(_source, DiagnosticEventType.MessageNotAcknowledged)
                {
                    Detail = "Message not acknowledged",
                    Uri = _request.Url,
                    Method = _request.HttpMethod,
                    Status = 422,
                    Exception = notAcknowledgedException
                }.Build());
                return true;
            }

            // HTTP 500: Unknown error
            _response.StatusCode = 500;
            _diagnosticService.Emit(new HttpEventBuilder(_source, DiagnosticEventType.UnhandledException)
            {
                Detail = "Unexpected error",
                Uri = _request.Url,
                Method = _request.HttpMethod,
                Status = 500,
                Exception = ex
            }.Build());
            return true;
        }
    }
}