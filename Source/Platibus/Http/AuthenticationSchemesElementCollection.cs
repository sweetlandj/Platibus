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

using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;

namespace Platibus.Http
{
    /// <summary>
    /// A configuration element collection that contains all of the authentication
    /// schemes that should be supported or enabled on the HTTP server
    /// </summary>
    public class AuthenticationSchemesElementCollection : ConfigurationElementCollection,
        IEnumerable<AuthenticationSchemeElement>
    {
        /// <summary>
        /// When overridden in a derived class, creates a new <see cref="T:System.Configuration.ConfigurationElement"/>.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Configuration.ConfigurationElement"/>.
        /// </returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new AuthenticationSchemeElement();
        }

        /// <summary>
        /// Gets the element key for a specified configuration element when overridden in a derived class.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Object"/> that acts as the key for the specified <see cref="T:System.Configuration.ConfigurationElement"/>.
        /// </returns>
        /// <param name="element">The <see cref="T:System.Configuration.ConfigurationElement"/> to return the key for. </param>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((AuthenticationSchemeElement) element).Scheme;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        IEnumerator<AuthenticationSchemeElement> IEnumerable<AuthenticationSchemeElement>.GetEnumerator()
        {
            return this.OfType<AuthenticationSchemeElement>().GetEnumerator();
        }

        /// <summary>
        /// Combines all of the elements in this collection into a single flagged
        /// <see cref="AuthenticationSchemes"/> value
        /// </summary>
        /// <returns>Returns the combination of all of the authentication scheme values
        /// in the collection as a single value</returns>
        public AuthenticationSchemes GetFlags()
        {
            if (Count == 0) return AuthenticationSchemes.Anonymous;

            var authSchemes = this.Select(e => e.Scheme);
            var flags = authSchemes.Aggregate(default(AuthenticationSchemes),
                (current, scheme) => current | scheme);

            return flags;
        }
    }
}