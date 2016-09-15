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

using System.Configuration;
using Platibus.Config.Extensibility;

namespace Platibus.Config
{
    /// <summary>
    /// Configuration element for subscription tracking
    /// </summary>
    /// <remarks>
    /// This element may or may not be applicable depending on the type of
    /// host and transport
    /// </remarks>
    public class SubscriptionTrackingElement : ExtensibleConfigurationElement
    {
        private const string ProviderPropertyName = "provider";

        /// <summary>
        /// The name of the subscriptin tracking service provider
        /// </summary>
        /// <seealso cref="ISubscriptionTrackingService"/>
        /// <seealso cref="ProviderAttribute"/>
        [ConfigurationProperty(ProviderPropertyName, DefaultValue = "Filesystem")]
        public string Provider
        {
            get { return (string) base[ProviderPropertyName]; }
            set { base[ProviderPropertyName] = value; }
        }
    }
}