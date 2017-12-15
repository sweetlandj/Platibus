using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;

namespace Platibus.AspNetCore
{
    internal static class HttpRequestExtensions
    {
        public static Uri GetUri(this HttpRequest request)
        {
            var url = request.GetEncodedUrl();
            return string.IsNullOrWhiteSpace(url) ? null : new Uri(url);
        }
    }
}
