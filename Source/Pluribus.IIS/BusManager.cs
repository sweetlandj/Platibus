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

using System.Threading;
using System.Threading.Tasks;
using Pluribus.Config;

namespace Pluribus.IIS
{
    public static class BusManager
    {
        private static readonly SemaphoreSlim SingletonWriteAccess = new SemaphoreSlim(1);
        private static volatile Bus _instance;

        public static async Task<Bus> GetInstance()
        {
            if (_instance != null) return _instance;
            await SingletonWriteAccess.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_instance != null) return _instance;
                var tempInstance = await Bootstrapper.InitBus().ConfigureAwait(false);
                _instance = tempInstance;
            }
            finally
            {
                SingletonWriteAccess.Release();
            }
            return _instance;
        }

        public static void Shutdown()
        {
            if (_instance != null)
            {
                _instance.Dispose();
            }
        }
    }
}
