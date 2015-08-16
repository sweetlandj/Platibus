using System;
using Common.Logging;

namespace Platibus.Http
{
    /// <summary>
    /// Updates an HTTP response with the HTTP status code that is most appopriate for
    /// a given exception
    /// </summary>
    public class HttpExceptionHandler
    {
        private readonly ILog _log;
        private readonly IHttpResourceRequest _request;
        private readonly IHttpResourceResponse _response;

        /// <summary>
        /// Initializes a new <see cref="HttpExceptionHandler"/> for the specified HTTP 
        /// <paramref name="request"/> and <paramref name="response"/>
        /// </summary>
        /// <param name="request">The HTTP request being processed</param>
        /// <param name="response">The HTTP response being constructed</param>
        /// <param name="log">(Optional) A log in which errors will be recorded</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="request"/> or
        /// <paramref name="response"/> are <c>null</c></exception>
        public HttpExceptionHandler(IHttpResourceRequest request, IHttpResourceResponse response, ILog log = null)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (response == null) throw new ArgumentNullException("response");
            _request = request;
            _response = response;
            _log = log ?? LogManager.GetLogger(LoggingCategories.Http);
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
            var aggregateException = ex as AggregateException;
            if (aggregateException != null)
            {
                _log.ErrorFormat("One or more errors occurred processing {0} request for resource {1}:", ex,
                    _request.HttpMethod, _request.Url);
                aggregateException.Handle(HandleException);
                return true;
            }

            var unauthorizedAccessException = ex as UnauthorizedAccessException;
            if (unauthorizedAccessException != null)
            {
                _log.ErrorFormat("{0} request for resource {1} not authorized for user {2}", ex, _request.HttpMethod,
                    _request.Url, _request.Principal.GetName());
                _response.StatusCode = 401;
                return true;
            }

            var notAcknowledgedException = ex as MessageNotAcknowledgedException;
            if (notAcknowledgedException != null)
            {
                _log.ErrorFormat("{0} request for resource {1} was not acknowledged", ex, _request.HttpMethod,
                    _request.Url);
                // HTTP 422: Unprocessable Entity
                _response.StatusCode = 422;
                return true;
            }

            _log.ErrorFormat("Unknown error processing {0} request for resource {1}", ex, _request.HttpMethod,
                _request.Url);
            // HTTP 500: Unknown error
            _response.StatusCode = 500;
            return true;
        }
    }
}