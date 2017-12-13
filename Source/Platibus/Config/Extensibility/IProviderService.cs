using System;

namespace Platibus.Config.Extensibility
{
    public interface IProviderService
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
        TProvider GetProvider<TProvider>(string providerName);
    }
}