using System;
using Common.Logging;

namespace Platibus.Http
{
    public class HttpExceptionHandler
    {
        private readonly ILog _log;
        private readonly IHttpResourceRequest _request;
        private readonly IHttpResourceResponse _response;

        public HttpExceptionHandler(IHttpResourceRequest request, IHttpResourceResponse response, ILog log = null)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (response == null) throw new ArgumentNullException("response");
            _request = request;
            _response = response;
            _log = log ?? LogManager.GetLogger(LoggingCategories.Http);
        }

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