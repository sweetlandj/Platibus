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
using System.IO;
using System.Threading.Tasks;
using Platibus.Http;
using Platibus.RabbitMQ;
using Platibus.Security;

namespace Platibus.IntegrationTests
{
    internal class With
    {
        public static async Task LoopbackInstance(Func<IBus, Task> test)
        {
            await LoopbackInstance(async (bus) =>
            {
                await test(bus);
                return true;
            });
        }

        public static async Task<TResult> LoopbackInstance<TResult>(Func<IBus, Task<TResult>> test)
        {
            Cleanup();
            using (var loobackHost = await LoopbackHost.Start("platibus.loopback"))
            {
                return await test(loobackHost.Bus);
            }
        }

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

            using (var server0 = await HttpServer.Start("platibus.http0"))
            using (var server1 = await HttpServer.Start("platibus.http1"))
            {
                // Give HTTP servers time to initialize
                await Task.Delay(TimeSpan.FromSeconds(1));

                return await test(server0.Bus, server1.Bus);
            }
        }

        public static async Task HttpHostedBusInstancesBasicAuth(IAuthorizationService authService, Func<IBus, IBus, Task> test)
        {
            await HttpHostedBusInstancesBasicAuth(authService, async (bus0, bus1) =>
            {
                await test(bus0, bus1);
                return true;
            });
        }

        public static async Task<TResult> HttpHostedBusInstancesBasicAuth<TResult>(IAuthorizationService authService, Func<IBus, IBus, Task<TResult>> test)
        {
            Cleanup();

            var config0 = await HttpServerConfigurationManager.LoadConfiguration("platibus.http-basic0");
            var config1 = await HttpServerConfigurationManager.LoadConfiguration("platibus.http-basic1");

            config0.AuthorizationService = authService;
            config1.AuthorizationService = authService;

            using (var server0 = await HttpServer.Start(config0))
            using (var server1 = await HttpServer.Start(config1))
            {
                // Give HTTP servers time to initialize
                await Task.Delay(TimeSpan.FromSeconds(1));

                return await test(server0.Bus, server1.Bus);
            }
        }

        public static async Task RabbitMQHostedBusInstances(Func<IBus, IBus, Task> test)
        {
            await RabbitMQHostedBusInstances(async (bus0, bus1) =>
            {
                await test(bus0, bus1);
                return true;
            });
        }

        public static async Task<TResult> RabbitMQHostedBusInstances<TResult>(Func<IBus, IBus, Task<TResult>> test)
        {
            Cleanup();

            using (var host0 = await RabbitMQHost.Start("platibus.rabbitmq0"))
            using (var host1 = await RabbitMQHost.Start("platibus.rabbitmq1"))
            {
                // Give Rabbit MQ hosts time to initialize
                await Task.Delay(TimeSpan.FromSeconds(1));

                return await test(host0.Bus, host1.Bus);
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
            catch (Exception)
            {
            }
        }
    }
}