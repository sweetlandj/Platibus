using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Platibus.Serialization;

namespace Platibus.Http.Controllers
{
    /// <summary>
    /// An HTTP resource controller for reporting metrics
    /// </summary>
    public class MetricsController : IHttpResourceController
    {
        private readonly NewtonsoftJsonSerializer _serializer = new NewtonsoftJsonSerializer();
        private readonly HttpMetricsCollector _metricsCollector;

        /// <summary>
        /// Initializes a new <see cref="MetricsController"/>
        /// </summary>
        /// <param name="metricsCollector">The data sink from which metrics can be sampled</param>
        public MetricsController(HttpMetricsCollector metricsCollector)
        {
            _metricsCollector = metricsCollector; 
        }
        
        /// <inheritdoc />
        public async Task Process(IHttpResourceRequest request, IHttpResourceResponse response,
            IEnumerable<string> subPath)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (response == null) throw new ArgumentNullException(nameof(response));

            if (!request.IsGet())
            {
                response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                response.AddHeader("Allow", "GET");
                return;
            }

            var metricsSegments = subPath.ToList();
            if (!metricsSegments.Any() && request.IsGet())
            {
                await GetMetrics(response);
                return;
            }
            
            response.StatusCode = 400;
        }

        private async Task GetMetrics(IHttpResourceResponse response)
        {
            if (_metricsCollector == null)
            {
                // Message journaling is not enabled
                response.StatusCode = (int)HttpStatusCode.NotImplemented;
                return;
            }

            response.ContentType = "application/json";
            var encoding = response.ContentEncoding;
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
                response.ContentEncoding = encoding;
            }

            var metrics = _metricsCollector.Sample;
            var responseContent = _serializer.Serialize(metrics);
            var encodedContent = encoding.GetBytes(responseContent);
            await response.OutputStream.WriteAsync(encodedContent, 0, encodedContent.Length);
            response.StatusCode = 200;
        }
    }
}
