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
using System.Threading.Tasks;
using Microsoft.Owin.BuilderProperties;
using Owin;

namespace Platibus.Owin
{
    public static class PlatibusAppBuilderExtensions
    {
        public static IAppBuilder UsePlatibusMiddleware(this IAppBuilder app, string sectionName = null)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            return app.UsePlatibusMiddleware(new PlatibusMiddleware(sectionName), true);
        }

        public static IAppBuilder UsePlatibusMiddleware(this IAppBuilder app, IOwinConfiguration configuration)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            return app.UsePlatibusMiddleware(new PlatibusMiddleware(configuration), true);
        }

        public static IAppBuilder UsePlatibusMiddleware(this IAppBuilder app, Task<IOwinConfiguration> configuration)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            return app.UsePlatibusMiddleware(new PlatibusMiddleware(configuration), true);
        }

        public static IAppBuilder UsePlatibusMiddleware(this IAppBuilder app, PlatibusMiddleware middleware, bool disposeMiddleware = false)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (middleware == null) throw new ArgumentNullException(nameof(app));

            if (disposeMiddleware)
            {
                var appProperties = new AppProperties(app.Properties);
                appProperties.OnAppDisposing.Register(middleware.Dispose);
            }

            return app.Use(middleware.Invoke);
        }
    }
}
