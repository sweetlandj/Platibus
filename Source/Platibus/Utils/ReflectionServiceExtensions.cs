using System;
using System.Collections.Generic;
using System.Linq;

namespace Platibus.Utils
{
    /// <summary>
    /// Convenience methods for working with <see cref="IReflectionService"/> implementations
    /// </summary>
    public  static class ReflectionServiceExtensions
    {
        /// <summary>
        /// Finds non-abstract implementations or subtypes of the specified base type
        /// <typeparamref name="TBase"/>
        /// </summary>
        /// <typeparam name="TBase">The base type</typeparam>
        /// <param name="reflectionService">The reflection service</param>
        /// <returns>Returns non-abstract implementations or subtypes of <typeparamref name="TBase"/></returns>
        public static IEnumerable<Type> FindConcreteSubtypes<TBase>(this IReflectionService reflectionService)
        {
            return reflectionService.EnumerateTypes()
                .Where(typeof(TBase).IsAssignableFrom)
                .Where(t => !t.IsInterface && !t.IsAbstract);
        }
        
        /// <summary>
        /// Finds types with the specified <paramref name="typeName"/>
        /// </summary>
        /// <param name="reflectionService">The reflection service</param>
        /// <param name="typeName">The type name</param>
        /// <returns>Returns the types with the specified <paramref name="typeName"/></returns>
        public static IEnumerable<Type> FindTypesByName(this IReflectionService reflectionService, string typeName)
        {
            return reflectionService.EnumerateTypes()
                .Where(t => Equals(t.FullName, typeName));
        }
    }
}
