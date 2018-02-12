#if NET452 || NET461
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
    /// <inheritdoc />
    /// <summary>
    /// Configuration element for message encryption
    /// </summary>
    public class EncryptionElement : ExtensibleConfigurationElement
    {
        private const string EnabledPropertyName = "enabled";
        private const string ProviderPropertyName = "provider";
        private const string KeyPropertyName = "key";
        private const string FallbackKeysPropertyName = "fallbackKeys";

        /// <summary>
        /// Indicates whether message journaling is enabled
        /// </summary>
        [ConfigurationProperty(EnabledPropertyName, DefaultValue = true)]
        public bool Enabled
        {
            get => (bool) base[EnabledPropertyName];
            set => base[EnabledPropertyName] = value;
        }

        /// <summary>
        /// The name of the message journaling service provider
        /// </summary>
        /// <seealso cref="Platibus.Config.Extensibility.IMessageJournalProvider"/>
        /// <seealso cref="Platibus.Config.Extensibility.ProviderAttribute"/>
        [ConfigurationProperty(ProviderPropertyName)]
        public string Provider
        {
            get => (string) base[ProviderPropertyName];
            set => base[ProviderPropertyName] = value;
        }

        /// <summary>
        /// The hexadecimal encoded key used to encrypt and decrypt messages
        /// </summary>
        [ConfigurationProperty(KeyPropertyName)]
        public string Key
        {
            get => (string) base[KeyPropertyName];
            set => base[KeyPropertyName] = value;
        }

        /// <summary>
        /// The hexadecimal encoded fallback keys used to decrypt messages
        /// during key rotation
        /// </summary>
        [ConfigurationProperty(FallbackKeysPropertyName)]
        public KeyElementCollection FallbackKeys
        {
            get => (KeyElementCollection) base[FallbackKeysPropertyName];
            set => base[FallbackKeysPropertyName] = value;
        }
    }
}
#endif