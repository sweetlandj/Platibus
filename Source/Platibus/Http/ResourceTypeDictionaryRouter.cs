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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Platibus.Http
{
    /// <inheritdoc cref="IHttpResourceRouter"/>
    /// <summary>
    /// An <see cref="T:Platibus.Http.IHttpResourceRouter" /> that treats the first segment of the
    /// request URL path as the <see cref="T:Platibus.Http.ResourceType" /> and routes to the
    /// <see cref="T:Platibus.Http.IHttpResourceController" /> associated with that resource type
    /// </summary>
    public sealed class ResourceTypeDictionaryRouter : IHttpResourceRouter,
        IEnumerable<KeyValuePair<ResourceType, IHttpResourceController>>
    {
        private readonly Uri _baseUri;
        private readonly IDictionary<ResourceType, IHttpResourceController> _resourceHandlers =
            new Dictionary<ResourceType, IHttpResourceController>();

        /// <summary>
        /// Initializes a new <see cref="ResourceTypeDictionaryRouter"/>
        /// </summary>
        /// <param name="baseUri">The base URI of the application</param>
        public ResourceTypeDictionaryRouter(Uri baseUri)
        {
            _baseUri = baseUri;
        }

        /// <inheritdoc />
        public bool IsRoutable(Uri uri)
        {
            var baseUriPath = _baseUri.AbsolutePath.ToLower();
            var uriPath = uri.AbsolutePath.ToLower();
            return uriPath.StartsWith(baseUriPath);
        }

        /// <summary>
        /// The resource types that can be routed
        /// </summary>
        /// <remarks>
        /// This set consists of the resource types that have been associated with
        /// controllers via the 
        /// <see cref="Add(ResourceType,IHttpResourceController)"/>
        /// or
        /// <see cref="Add(KeyValuePair{ResourceType, IHttpResourceController})"/>
        /// methods
        /// </remarks>
        public IEnumerable<ResourceType> ResourceTypes => _resourceHandlers.Keys;

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        IEnumerator<KeyValuePair<ResourceType, IHttpResourceController>>
            IEnumerable<KeyValuePair<ResourceType, IHttpResourceController>>.GetEnumerator()
        {
            return _resourceHandlers.GetEnumerator();
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _resourceHandlers.GetEnumerator();
        }

        /// <inheritdoc />
        /// <summary>
        /// Routes a <paramref name="request" /> and <paramref name="response" /> to
        /// the appropriate controller
        /// </summary>
        /// <param name="request">The request to route</param>
        /// <param name="response">The response to route</param>
        /// <returns>
        /// Returns a task that completes once the request has been routed and handled
        /// </returns>
        public async Task Route(IHttpResourceRequest request, IHttpResourceResponse response)
        {
            var requestPath = request.Url.AbsolutePath;
            if (string.IsNullOrWhiteSpace(requestPath)) return;

            var pathSegments = requestPath.Split('/');
            var supportedResourceTypes = ResourceTypes.ToList();
            var resourceSegments = pathSegments
                .SkipWhile(segment => !supportedResourceTypes.Contains(segment))
                .ToList();

            var resourceType = resourceSegments.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(resourceType))
            {
                response.StatusCode = 400;
                return;
            }

            var controller = GetController(resourceType);
            if (string.IsNullOrWhiteSpace(resourceType))
            {
                response.StatusCode = 400;
                return;
            }

            var subPath = resourceSegments.Skip(1); // Skip resource type
            await controller.Process(request, response, subPath);
        }

        /// <summary>
        /// Adds a route for the specified <paramref name="resourceType"/> and
        /// <paramref name="controller"/>
        /// </summary>
        /// <param name="resourceType">The type of resource to which the route pertains</param>
        /// <param name="controller">The controller to which requests related to the 
        /// <paramref name="resourceType"/> should be routed</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="resourceType"/>
        /// or <paramref name="controller"/> are <c>null</c></exception>
        public void Add(ResourceType resourceType, IHttpResourceController controller)
        {
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType));
            if (controller == null) throw new ArgumentNullException(nameof(controller));
            _resourceHandlers.Add(resourceType, controller);
        }

        /// <summary>
        /// Adds a route
        /// </summary>
        /// <param name="route">A key-value pair consisting of the resource type and controller
        /// to which requests pertaining to the resource type should be routed</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="route"/>
        /// is <c>null</c></exception>
        public void Add(KeyValuePair<ResourceType, IHttpResourceController> route)
        {
            _resourceHandlers.Add(route.Key, route.Value);
        }

        private IHttpResourceController GetController(ResourceType resourceType)
        {
            if (_resourceHandlers.TryGetValue(resourceType, out var resourceHandler))
            {
                return resourceHandler;
            }
            throw new ArgumentOutOfRangeException(nameof(resourceType), resourceType);
        }
    }
}