using Common.Logging;
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Platibus.Config
{
    static class ReflectionHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(LoggingCategories.Config);

        public static IEnumerable<Type> FindConcreteSubtypes<TBase>()
        {
            var appDomainBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var directories = new[]
            {
                new DirectoryInfo(appDomainBaseDirectory),
                new DirectoryInfo(Path.Combine(appDomainBaseDirectory, "bin"))
            };

            var filenamePatterns = new[] { "*.dll", "*.exe" };
            var assemblyFiles = directories
                .SelectMany(dir => filenamePatterns, (dir, pattern) => new
                {
                    Directory = dir,
                    FilenamePattern = pattern
                })
                .Where(dir => dir.Directory.Exists)
                .SelectMany(x => x.Directory.GetFiles(x.FilenamePattern, SearchOption.TopDirectoryOnly));

            var subtypes = new List<Type>();
            foreach (var assemblyFile in assemblyFiles)
            {
                try
                {
                    var assembly = Assembly.ReflectionOnlyLoadFrom(assemblyFile.FullName);
                    Log.DebugFormat("Scanning assembly {0} for concrete subtypes of {1}...", assembly.GetName().FullName, typeof(TBase).FullName);
                    subtypes.AddRange(AppDomain.CurrentDomain.Load(assembly.GetName())
                        .GetTypes()
                        .Where(typeof(TBase).IsAssignableFrom)
                        .Where(t => !t.IsInterface && !t.IsAbstract));
                }
                catch (Exception ex)
                {
                    Log.WarnFormat("Error scanning assembly file {0}", ex, assemblyFile);
                }
            }
            return subtypes;
        }

        public static bool Has<TAttribute>(this Type type) where TAttribute : Attribute
        {
            return type.GetCustomAttributes(typeof(TAttribute), false).Any();
        }

        public static bool Has<TAttribute>(this Type type, Func<TAttribute, bool> where) where TAttribute : Attribute
        {
            return type.GetCustomAttributes(typeof(TAttribute), false)
                .OfType<TAttribute>()
                .Any(where);
        }

        public static IEnumerable<Type> With<TAttribute>(this IEnumerable<Type> source) where TAttribute : Attribute
        {
            return source.Where(t => t.Has<TAttribute>());
        }

        public static IEnumerable<Type> With<TAttribute>(this IEnumerable<Type> source, Func<TAttribute, bool> where) where TAttribute : Attribute
        {
            return source.Where(t => t.Has<TAttribute>(where));
        }

        public static IEnumerable<Type> OrderBy<TAttribute>(this IEnumerable<Type> source, Func<TAttribute, object> attributeMember) where TAttribute : Attribute
        {
            return source.Select(t => new
            {
                Type = t,
                Attribute = t.GetCustomAttribute<TAttribute>()
            })
            .Where(x => x.Attribute != null)
            .OrderBy(x => attributeMember(x.Attribute))
            .Select(x => x.Type);
        }

        public static IEnumerable<Type> OrderByDescending<TAttribute>(this IEnumerable<Type> source, Func<TAttribute, object> attributeMember) where TAttribute : Attribute
        {
            return source.Select(t => new
            {
                Type = t,
                Attribute = t.GetCustomAttribute<TAttribute>()
            })
            .Where(x => x.Attribute != null)
            .OrderByDescending(x => attributeMember(x.Attribute))
            .Select(x => x.Type);
        }
    }
}
