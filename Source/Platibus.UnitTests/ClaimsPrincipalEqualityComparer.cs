using System.Collections.Generic;
using System.Security.Claims;
using KellermanSoftware.CompareNetObjects;

namespace Platibus.UnitTests
{
    internal class ClaimsPrincipalEqualityComparer : IEqualityComparer<ClaimsPrincipal>
    {
        public bool Equals(ClaimsPrincipal x, ClaimsPrincipal y)
        {
            var compareLogic = new CompareLogic();
            return compareLogic.Compare(x, y).AreEqual;
        }

        public int GetHashCode(ClaimsPrincipal obj)
        {
            return obj == null ? 0 : obj.GetHashCode();
        }
    }
}