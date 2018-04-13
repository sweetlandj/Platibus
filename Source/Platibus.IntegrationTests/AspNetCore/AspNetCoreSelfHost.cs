#if NETCOREAPP2_0
// The MIT License (MIT)
// 
// Copyright (c) 2017 Jesse Sweetland
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

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Platibus.AspNetCore;
using Platibus.Utils;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Platibus.IntegrationTests.AspNetCore
{
    public class AspNetCoreSelfHost : IDisposable
    {
        public IBus Bus { get; }
        public IWebHost WebHost { get; }
        
        public AspNetCoreSelfHost(AspNetCoreConfiguration configuration)
        {
            var startup = new Startup(configuration);
            var busSource = new TaskCompletionSource<IBus>();
            var busInitialized = busSource.Task;

            var webHostBuilder = Microsoft.AspNetCore.WebHost
                .CreateDefaultBuilder()
                .UseUrls(configuration.BaseUri.WithoutTrailingSlash().ToString())
                .ConfigureServices(services =>
                {
                    startup.ConfigureServices(services);
                    var bus = services.BuildServiceProvider().GetService<IBus>();
                    busSource.TrySetResult(bus);
                })
                .Configure(app => startup.Configure(app, null))
                .UseKestrel();

            WebHost = webHostBuilder.Build();
            Bus = busInitialized.Result;

            WebHost.Start();
        }

        public static AspNetCoreSelfHost Start(string sectionName, Action<AspNetCoreConfiguration> configure = null)
        {
            return StartAsync(sectionName, configure).GetResultFromCompletionSource();
        }

        public static async Task<AspNetCoreSelfHost> StartAsync(string sectionName, Action<AspNetCoreConfiguration> configure = null)
        {
            var serverPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sectionName);
            var serverDirectory = new DirectoryInfo(serverPath);
            serverDirectory.Refresh();
            if (serverDirectory.Exists)
            {
                serverDirectory.Delete(true);
            }

            var configuration = new AspNetCoreConfiguration();
            var configurationManager = new AspNetCoreConfigurationManager();
            await configurationManager.Initialize(configuration, sectionName);
            await configurationManager.FindAndProcessConfigurationHooks(configuration);
            configure?.Invoke(configuration);
            return new AspNetCoreSelfHost(configuration);
        }

        protected virtual void Dispose(bool disposing)
        {
            var tcs = new TaskCompletionSource<bool>();
            var webHostStopped = tcs.Task;

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var cancellationToken = cancellationTokenSource.Token;
                cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(5));
                WebHost.StopAsync(cancellationToken).ContinueWith(t => tcs.TrySetResult(true), cancellationToken);
                webHostStopped.Wait(cancellationToken);
            }
           
            WebHost.Dispose();
            if (Bus is IDisposable disposableBus)
            {
                disposableBus.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~AspNetCoreSelfHost()
        {
            Dispose(false);
        }
    }
}
#endif