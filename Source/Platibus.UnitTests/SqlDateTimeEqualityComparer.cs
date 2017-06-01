using System;
using System.Collections.Generic;
using System.Data.SqlTypes;

namespace Platibus.UnitTests
{
    /// <summary>
    /// Rounds date time to precision supported by SQL server to ensure equality assertions
    /// pass where expected
    /// </summary>
    internal class SqlDateTimeEqualityComparer : IEqualityComparer<DateTime>
    {
        public bool Equals(DateTime x, DateTime y)
        {
            return Round(x).Equals(Round(y));
        }

        public int GetHashCode(DateTime obj)
        {
            return Round(obj).GetHashCode();
        }

        protected virtual DateTime Round(DateTime dt)
        {
            return new SqlDateTime(dt).Value;
        }
    }
}
