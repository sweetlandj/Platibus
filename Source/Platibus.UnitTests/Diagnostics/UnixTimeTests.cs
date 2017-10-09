// The MIT License (MIT)
// 
// Copyright (c) 2017 Jesse Sweetland
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
using Platibus.Diagnostics;
using Xunit;

namespace Platibus.UnitTests.Diagnostics
{
    [Trait("Category", "UnitTests")]
    public class UnixTimeTests
    {
        [Fact]
        public void DateTimeToUnixTimeRoundTrip()
        {
            var dateTime = DateTime.UtcNow;
            var unixTime = new UnixTime(dateTime);
            var roundTripDateTime = unixTime.ToDateTime();
            Assert.Equal(dateTime, roundTripDateTime, new NearestSecondDateTimeEqualityComparer());

        }

        [Fact]
        public void UnixTimeToDateTimeRoundTrip()
        {
            var unixTime = UnixTime.Current;
            var dateTime = unixTime.ToDateTime();
            var roundTripUnixTime = new UnixTime(dateTime);

            var milliseconds = unixTime.Milliseconds;
            var roundTrimeMilliseconds = roundTripUnixTime.Milliseconds;
            var difference = Math.Abs(milliseconds - roundTrimeMilliseconds);

            // Within 1 millisecond (to account for rounding errors)
            Assert.True(difference <= 1);
        }
    }
}
