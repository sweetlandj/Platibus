// The MIT License (MIT)
// 
// Copyright (c) 2014 Jesse Sweetland
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

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pluribus.Filesystem
{
    internal static class PathExtensions
    {
        public static string WithInvalidCharsReplaced(this string path, char replacement = '_')
        {
            var pathChars = path.ToCharArray();
            var pathCharsWithInvalidCharsReplaced = pathChars.WithInvalidCharsReplaced(replacement);
            return new string(pathCharsWithInvalidCharsReplaced.ToArray());
        }

        public static IEnumerable<char> WithInvalidCharsReplaced(this IEnumerable<char> chars, char replacement = '_')
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var c in chars)
            {
                if (invalidChars.Contains(c)) yield return replacement;
                else yield return c;
            }
        }
    }
}