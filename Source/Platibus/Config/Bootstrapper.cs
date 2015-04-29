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

using System.Threading.Tasks;
using Common.Logging;
using Platibus.Http;

namespace Platibus.Config
{
    public static class Bootstrapper
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.Config);

        public static Task<Bus> InitBus()
        {
            return InitBus("platibus");
        }

        public static async Task<Bus> InitBus(string sectionName)
        {
            Log.InfoFormat("Loading configuration from section \"{0}\"...", sectionName);
            var configuration = await PlatibusConfigurationManager.LoadConfiguration(sectionName).ConfigureAwait(false);

            Log.Info("Initializing bus...");
            var bus = new Bus(configuration, new HttpTransportService());
            await bus.Init().ConfigureAwait(false);

            Log.Info("Bus initialized successfully");
            return bus;
        }
    }
}