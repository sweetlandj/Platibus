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
using Platibus.Diagnostics;

namespace Platibus.Config
{
    internal class ReflectionService
    {
        private readonly IDiagnosticService _diagnosticService;

        public ReflectionService()
        {
            _diagnosticService = DiagnosticService.DefaultInstance;
        }

        public ReflectionService(IDiagnosticService diagnosticService)
        {
            _diagnosticService = diagnosticService ?? DiagnosticService.DefaultInstance;
        }

        public IEnumerable<Type> FindConcreteSubtypes<TBase>()
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

            var subtypes = new List<Type>();
            foreach (var assemblyFile in assemblyFiles)
            {
                try
                {
                    var assemblyName = AssemblyName.GetAssemblyName(assemblyFile.FullName);
                    var appDomain = AppDomain.CurrentDomain;
                    var assembly = appDomain.GetAssemblies()
                        .FirstOrDefault(a => a.GetName() == assemblyName)
                        ?? appDomain.Load(assemblyName);
                    
                    subtypes.AddRange(assembly.GetTypes()
                        .Where(typeof(TBase).IsAssignableFrom)
                        .Where(t => !t.IsInterface && !t.IsAbstract));
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
            return subtypes;
        }
    }
}