// The MIT License (MIT)
// 
// Copyright (c) 2016 Jesse Sweetland
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
using Microsoft.Extensions.DependencyModel;
using Platibus.Diagnostics;

namespace Platibus.Utils
{
    /// <inheritdoc />
    /// <summary>
    /// A basic <see cref="T:Platibus.Utils.IReflectionService" /> implementation that loads types from the app domain,
    /// the assemblies found in the app domain base directory, and default assemblies from the
    /// dependency context.
    /// </summary>
    public class DefaultReflectionService : IReflectionService
    {
        private readonly IDiagnosticService _diagnosticService;

        /// <inheritdoc />
        /// <summary>
        /// Initialies a new <see cref="T:Platibus.Utils.DefaultReflectionService" />
        /// </summary>
        public DefaultReflectionService() : this(null)
        {
            _diagnosticService = DiagnosticService.DefaultInstance;
        }

        /// <summary>
        /// Initializes a new <see cref="DefaultReflectionService"/>
        /// </summary>
        /// <param name="diagnosticService">The diagnostic service through which events related to assembly
        /// or type loading will be reported</param>
        public DefaultReflectionService(IDiagnosticService diagnosticService)
        {
            _diagnosticService = diagnosticService ?? DiagnosticService.DefaultInstance;
        }

        /// <inheritdoc />
        public IEnumerable<Type> EnumerateTypes()
        {
            var appDomain = AppDomain.CurrentDomain;
            var assemblyNames = GetAssemblyNames(appDomain);
            foreach (var assemblyName in assemblyNames)
            {
                var assemblyTypes = Enumerable.Empty<Type>();
                try
                {
                    var assembly = appDomain.GetAssemblies()
                                       .FirstOrDefault(a => a.GetName() == assemblyName)
                                   ?? appDomain.Load(assemblyName);

                    assemblyTypes = assembly.GetTypes();
                }
                catch (Exception ex)
                {
                    _diagnosticService.Emit(new DiagnosticEventBuilder(this, DiagnosticEventType.TypeLoadFailed)
                    {
                        Detail = $"Error loading assembly {assemblyName}",
                        Exception = ex
                    }.Build());
                }

                foreach (var assemblyType in assemblyTypes)
                {
                    yield return assemblyType;
                }
            }
        }
        
        protected IEnumerable<AssemblyName> GetAssemblyNames(AppDomain appDomain)
        {
            return GetDefaultAssemblyNames()
                .Union(GetAppDomainBaseDirectoryAssemblyNames(appDomain))
                .Distinct(new AssemblyNameEqualityComparer());
        }

        protected IEnumerable<AssemblyName> GetDefaultAssemblyNames()
        {
            var dependencyContext = DependencyContext.Default;
            return dependencyContext?.GetDefaultAssemblyNames() ?? Enumerable.Empty<AssemblyName>();
        }

        protected IEnumerable<AssemblyName> GetAppDomainBaseDirectoryAssemblyNames(AppDomain appDomain)
        {
            var appDomainBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var directories = new[]
            {
                new DirectoryInfo(appDomainBaseDirectory),
                new DirectoryInfo(Path.Combine(appDomainBaseDirectory, "bin"))
            };

            var filenamePatterns = new[] {"*.dll", "*.exe"};
            var assemblyFiles = directories
                .SelectMany(dir => filenamePatterns, (dir, pattern) => new
                {
                    Directory = dir,
                    FilenamePattern = pattern
                })
                .Where(dir => dir.Directory.Exists)
                .SelectMany(x => x.Directory.GetFiles(x.FilenamePattern, SearchOption.TopDirectoryOnly));

            var assemblyNames = new List<AssemblyName>();
            foreach (var assemblyFile in assemblyFiles)
            {
                try
                {
                    var assemblyName = AssemblyName.GetAssemblyName(assemblyFile.FullName);
                    assemblyNames.Add(assemblyName);
                }
                catch (Exception ex)
                {
                    _diagnosticService.Emit(new DiagnosticEventBuilder(this, DiagnosticEventType.TypeLoadFailed)
                    {
                        Detail = $"Error loading assembly file {assemblyFile.FullName}",
                        Exception = ex
                    }.Build());
                } 
            }

            return assemblyNames;
        }

        private class AssemblyNameEqualityComparer : IEqualityComparer<AssemblyName>
        {
            public bool Equals(AssemblyName x, AssemblyName y)
            {
                if (x is null) return false;
                if (y is null) return false;
                return Equals(x.Name, y.Name);
            }

            public int GetHashCode(AssemblyName obj)
            {
                return obj.Name.GetHashCode();
            }
        }
    }
}