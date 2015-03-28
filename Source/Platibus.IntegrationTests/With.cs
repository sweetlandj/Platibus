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

using Platibus.Config;
using Platibus.Http;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Platibus.IntegrationTests
{
    class With
    {
        public static async Task HttpHostedBusInstances(Func<IBus, IBus, Task> test)
        {
            await HttpHostedBusInstances(async (bus0, bus1) =>
            {
                await test(bus0, bus1);
                return true;
            });
        }

        public static async Task<TResult> HttpHostedBusInstances<TResult>(Func<IBus, IBus, Task<TResult>> test)
        {
            Cleanup();

            using (var platibus0 = await Bootstrapper.InitBus("platibus0").ConfigureAwait(false))
            using (var platibus1 = await Bootstrapper.InitBus("platibus1").ConfigureAwait(false))
            using (var server0 = new HttpServer(platibus0))
            using (var server1 = new HttpServer(platibus1))
            {
                server0.Start();
                server1.Start();

                // Give HTTP listeners time to initialize
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

                return await test(platibus0, platibus1);
            }
        }

        public static async Task HttpHostedBusInstancesBasicAuth(Func<IBus, IBus, Task> test)
        {
            await HttpHostedBusInstancesBasicAuth(async (bus0, bus1) =>
            {
                await test(bus0, bus1);
                return true;
            });
        }

        public static async Task<TResult> HttpHostedBusInstancesBasicAuth<TResult>(Func<IBus, IBus, Task<TResult>> test)
        {
            Cleanup();

            using (var platibus0 = await Bootstrapper.InitBus("platibus0-basic").ConfigureAwait(false))
            using (var platibus1 = await Bootstrapper.InitBus("platibus1-basic").ConfigureAwait(false))
            using (var server0 = new HttpServer(platibus0, AuthenticationSchemes.Basic))
            using (var server1 = new HttpServer(platibus1, AuthenticationSchemes.Basic))
            {
                server0.Start();
                server1.Start();

                // Give HTTP listeners time to initialize
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

                return await test(platibus0, platibus1);
            }
        }

        private static void Cleanup()
        {
            if (Directory.Exists("platibus0"))
            {
                TryTo(() => Directory.Delete("platibus0", true));
            }

            if (Directory.Exists("platibus1"))
            {
                TryTo(() => Directory.Delete("platibus1", true));
            }
        }

        private static void TryTo(Action action)
        {
            try
            {
                action();
            }
            catch (Exception) { }
        }
    }
}
