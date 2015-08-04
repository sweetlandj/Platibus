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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Platibus.Http
{
    public sealed class ResourceTypeDictionaryRouter : IHttpResourceRouter,
        IEnumerable<KeyValuePair<ResourceType, IHttpResourceController>>
    {
        private readonly IDictionary<ResourceType, IHttpResourceController> _resourceHandlers =
            new Dictionary<ResourceType, IHttpResourceController>();

        public IEnumerable<ResourceType> ResourceTypes
        {
            get { return _resourceHandlers.Keys; }
        }

        IEnumerator<KeyValuePair<ResourceType, IHttpResourceController>>
            IEnumerable<KeyValuePair<ResourceType, IHttpResourceController>>.GetEnumerator()
        {
            return _resourceHandlers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _resourceHandlers.GetEnumerator();
        }

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

        public void Add(ResourceType resourceType, IHttpResourceController resourceHandler)
        {
            if (resourceType == null) throw new ArgumentNullException("resourceType");
            if (resourceHandler == null) throw new ArgumentNullException("resourceHandler");
            _resourceHandlers.Add(resourceType, resourceHandler);
        }

        public void Add(KeyValuePair<ResourceType, IHttpResourceController> entry)
        {
            _resourceHandlers.Add(entry.Key, entry.Value);
        }

        public IHttpResourceController GetController(ResourceType resourceType)
        {
            IHttpResourceController resourceHandler;
            if (_resourceHandlers.TryGetValue(resourceType, out resourceHandler))
            {
                return resourceHandler;
            }
            throw new ArgumentOutOfRangeException("resourceType", resourceType);
        }
    }
}