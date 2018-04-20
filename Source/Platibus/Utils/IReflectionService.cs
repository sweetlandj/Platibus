using System;
using System.Collections.Generic;

namespace Platibus.Utils
{
    /// <summary>
    /// Services for reflecting over and interacting with referenced types
    /// </summary>
    public interface IReflectionService
    {
        /// <summary>
        /// Enumerates all available types
        /// </summary>
        /// <remarks>
        /// For example this may include types from assemblies found in the app domain
        /// base directory; the default types identified by the .NET Standard dependency
        /// model; or types that have already been loaded into the app domain.
        /// </remarks>
        /// <returns>All available types</returns>
        IEnumerable<Type> EnumerateTypes();
    }
}