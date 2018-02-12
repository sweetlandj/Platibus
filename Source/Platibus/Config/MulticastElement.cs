#if NET452 || NET461
// The MIT License (MIT)
// 
// Copyright (c) 2018 Jesse Sweetland
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

using System.ComponentModel;
using System.Configuration;
using System.Net;

namespace Platibus.Config
{
    /// <summary>
    /// Configuration element for configuring multicast groups
    /// </summary>
    public class MulticastElement : ConfigurationElement
    {
        private const string EnabledPropertyName = "enabled";
        private const string AddressPropertyName = "address";
        private const string PortPropertyName = "port";


        /// <summary>
        /// Whether to broadcast subscription changes to members of a multicast group
        /// </summary>
        [ConfigurationProperty(EnabledPropertyName, DefaultValue = false)]
        public bool Enabled
        {
            get => (bool)base[EnabledPropertyName];
            set => base[EnabledPropertyName] = value;
        }

        /// <summary>
        /// The IP address of the multicast group to join
        /// </summary>
        [ConfigurationProperty(AddressPropertyName, DefaultValue = MulticastDefaults.Address)]
        [TypeConverter(typeof(IPAddressConverter))]
        public IPAddress Address
        {
            get => (IPAddress)base[AddressPropertyName];
            set => base[AddressPropertyName] = value;
        }

        /// <summary>
        /// The port of the multicast group to join
        /// </summary>
        [ConfigurationProperty(PortPropertyName, DefaultValue = MulticastDefaults.Port)]
        public int Port
        {
            get => (int)base[PortPropertyName];
            set => base[PortPropertyName] = value;
        }
    }
}

#endif