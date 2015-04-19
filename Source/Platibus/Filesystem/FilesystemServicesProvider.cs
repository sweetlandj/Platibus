using Platibus.Config;
using Platibus.Config.Extensibility;
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
using System.IO;
using System.Threading.Tasks;

namespace Platibus.Filesystem
{
    [ProviderAttribute("Filesystem")]
    public class FilesystemServicesProvider : IMessageQueueingServiceProvider, IMessageJournalingServiceProvider, ISubscriptionTrackingServiceProvider
    {
        public Task<IMessageQueueingService> CreateMessageQueueingService(QueueingElement configuration)
        {
            var path = configuration.GetString("path");
            var fsQueueingBaseDir = new DirectoryInfo(GetRootedPath(path));
            var fsQueueingService = new FilesystemMessageQueueingService(fsQueueingBaseDir);
            fsQueueingService.Init();
            return Task.FromResult<IMessageQueueingService>(fsQueueingService);
        }

        public Task<IMessageJournalingService> CreateMessageJournalingService(JournalingElement configuration)
        {
            var path = configuration.GetString("path");
            var fsJournalingBaseDir = new DirectoryInfo(GetRootedPath(path));
            var fsJournalingService = new FilesystemMessageJournalingService(fsJournalingBaseDir);
            fsJournalingService.Init();
            return Task.FromResult<IMessageJournalingService>(fsJournalingService);
        }

        public Task<ISubscriptionTrackingService> CreateSubscriptionTrackingService(SubscriptionTrackingElement configuration)
        {
            var path = configuration.GetString("path");
            var fsTrackingBaseDir = new DirectoryInfo(GetRootedPath(path));
            var fsTrackingService = new FilesystemSubscriptionTrackingService(fsTrackingBaseDir);
            fsTrackingService.Init();
            return Task.FromResult<ISubscriptionTrackingService>(fsTrackingService);
        }

        public static string GetRootedPath(string path)
        {
            var defaultRootedPath = AppDomain.CurrentDomain.BaseDirectory;
            if (string.IsNullOrWhiteSpace(path))
            {
                return defaultRootedPath;
            }

            if (Path.IsPathRooted(path))
            {
                return path;
            }

            return Path.Combine(defaultRootedPath, path);
        }
    }
}
