using System;
using System.Collections.Generic;

namespace Platibus
{
    /// <summary>
    /// Determines the equality of two endpoints based on the left part of their respective address URIs
    /// </summary>
    public class EndpointAddressEqualityComparer : IEqualityComparer<Uri>
    {
        /// <summary>
        /// Determines whether the specified endpoint address URIs are equal.
        /// </summary>
        /// <returns>
        /// true if the specified URIs are equal; otherwise, false.
        /// </returns>
        /// <param name="x">The first endpoint address URI to compare.</param>
        /// <param name="y">The second endpoint address URI to compare.</param>
        public bool Equals(Uri x, Uri y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(null, x)) return false;
            if (ReferenceEquals(null, y)) return false;
            return string.Equals(
                x.GetLeftPart(UriPartial.Path).TrimEnd('/'),
                y.GetLeftPart(UriPartial.Path).TrimEnd('/'),
                StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <returns>
        /// A hash code for the specified object.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param>
        /// <exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
        public int GetHashCode(Uri obj)
        {
            return obj == null ? 0 : obj
                .GetLeftPart(UriPartial.Path).TrimEnd('/')
                .ToLower()
                .GetHashCode();
        }
    }
}
