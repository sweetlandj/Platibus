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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Platibus.Config.Extensibility
{
    /// <summary>
    /// An exception thrown to indicate that there were more than one suitable types 
    /// found in the application domain associated with a provider name and having
    /// the same priority.
    /// </summary>
    /// <seealso cref="ProviderAttribute"/>
    /// <seealso cref="ProviderHelper"/>
    [Serializable]
    public class MultipleProvidersFoundException : Exception
    {
        private readonly Type[] _providers;

        /// <summary>
        /// The provider name for which multiple types were found in the
        /// application domain.
        /// </summary>
        public string ProviderName { get; }

        /// <summary>
        /// The types found in the app domain having the same
        /// <see cref="ProviderName"/>.
        /// </summary>
        public IEnumerable<Type> Providers => _providers;

        /// <summary>
        /// Initializes a new <see cref="MultipleProvidersFoundException"/>
        /// for the specified <paramref name="providerName"/> and types.
        /// </summary>
        /// <param name="providerName">The provider name</param>
        /// <param name="providers">The types found in the application
        /// domain having the provider name</param>
        public MultipleProvidersFoundException(string providerName, IEnumerable<Type> providers) : base(providerName)
        {
            ProviderName = providerName;
            _providers = providers != null ? providers.Where(t => t != null).ToArray() : new Type[0];
        }

        /// <summary>
        /// Initializes a serialized <see cref="MultipleProvidersFoundException"/>
        /// from a streaming context.
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The streaming context</param>
        public MultipleProvidersFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ProviderName = info.GetString("providerName");
            _providers = (Type[]) info.GetValue("providers", typeof (Type[]));
        }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown. </param><param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination. </param><exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is a null reference (Nothing in Visual Basic). </exception><filterpriority>2</filterpriority><PermissionSet><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter"/></PermissionSet>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("providerName", ProviderName);
            info.AddValue("providers", _providers);
        }
    }
}