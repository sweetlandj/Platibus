using System;
using System.Collections.Generic;

namespace Platibus
{
    /// <summary>
    /// Determines the equality of two endpoints based on the left part of their respective address URIs
    /// </summary>
    public class EndpointAddressEqualityComparer : IEqualityComparer<Uri>
    {
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

        public int GetHashCode(Uri obj)
        {
            return obj == null ? 0 : obj
                .GetLeftPart(UriPartial.Path).TrimEnd('/')
                .ToLower()
                .GetHashCode();
        }
    }
}
