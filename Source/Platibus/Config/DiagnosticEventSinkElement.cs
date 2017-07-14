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
using System.Configuration;

namespace Platibus.Config
{
    /// <summary>
    /// Configuration element for a diagnostic event sink
    /// </summary>
    public class DiagnosticEventSinkElement : ExtensibleConfigurationElement
    {
        private const string NamePropertyName = "name";
        private const string ProviderPropertyName = "provider";

        /// <summary>
        /// The name of the diagnostic event sink instance
        /// </summary>
        [ConfigurationProperty(NamePropertyName, IsRequired = true)]
        public string Name
        {
            get { return (string)base[NamePropertyName]; }
            set { base[NamePropertyName] = value; }
        }

        /// <summary>
        /// The name of the diagnostic event sink provider
        /// </summary>
        /// <seealso cref="Platibus.Config.Extensibility.IDiagnosticEventSinkProvider"/>
        /// <seealso cref="Platibus.Config.Extensibility.ProviderAttribute"/>
        [ConfigurationProperty(ProviderPropertyName, IsRequired = true)]
        public string Provider
        {
            get { return (string)base[ProviderPropertyName]; }
            set { base[ProviderPropertyName] = value; }
        }
    }
}
