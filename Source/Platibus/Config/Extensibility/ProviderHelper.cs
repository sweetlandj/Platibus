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

namespace Platibus.Config.Extensibility
{
    /// <summary>
    /// Factory class used to initialize service providers
    /// </summary>
    public static class ProviderHelper
    {
        /// <summary>
        /// Finds the most appropriate type or subtype of 
        /// <typeparamref name="TProvider"/> whose type name is <paramref name="providerName"/>
        /// or which has been decorated with a <see cref="ProviderAttribute"/> with the
        /// specified <paramref name="providerName"/>.
        /// </summary>
        /// <typeparam name="TProvider">The provider type or at type from which the provider 
        /// must inherit</typeparam>
        /// <param name="providerName">The provider name associated with the type</param>
        /// <returns>Returns a new provider instance with the specified 
        /// <paramref name="providerName"/> and type.</returns>
        /// <exception cref="NullReferenceException">Thrown if <paramref name="providerName"/>
        /// is <c>null</c> or whitespace.</exception>
        /// <exception cref="ProviderNotFoundException">Thrown if no suitable providers are
        /// found in the application domain.</exception>
        /// <exception cref="MultipleProvidersFoundException">Thrown if there are multiple
        /// providers found with the same priority.</exception>
        /// <seealso cref="ProviderAttribute"/>
        public static TProvider GetProvider<TProvider>(string providerName)
        {
            var providerType = Type.GetType(providerName);
            if (providerType == null)
            {
                var prioritizedProviders = ReflectionHelper
                    .FindConcreteSubtypes<TProvider>()
                    .WithProviderName(providerName)
                    .GroupByPriorityDescending()
                    .ToList();

                var highestPriority = prioritizedProviders.FirstOrDefault();
                if (highestPriority == null) throw new ProviderNotFoundException(providerName);

                var providers = highestPriority.ToList();
                if (providers.Count > 1) throw new MultipleProvidersFoundException(providerName, providers);
                
                providerType = providers.First();
            }

            return (TProvider) Activator.CreateInstance(providerType);
        }
        
        private static IEnumerable<Type> WithProviderName(this IEnumerable<Type> types, string providerName)
        {
            return string.IsNullOrWhiteSpace(providerName) 
                ? Enumerable.Empty<Type>() 
                : types.With<ProviderAttribute>(a => providerName.Equals(a.Name, StringComparison.OrdinalIgnoreCase));
        }

        private static IEnumerable<IGrouping<int, Type>> GroupByPriorityDescending(this IEnumerable<Type> types)
        {
            return types
                .GroupBy<ProviderAttribute, int>(a => a.Priority)
                .OrderByDescending(g => g.Key);
        }
    }
}