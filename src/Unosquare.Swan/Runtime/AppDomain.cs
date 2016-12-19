using System.Collections.Generic;

namespace Unosquare.Swan.Runtime
{
    using Abstractions;
    using Microsoft.Extensions.DependencyModel;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Represents a polyfill class to replace interoperability with .net core
    /// Idea taken from: http://www.michael-whelan.net/replacing-appdomain-in-dotnet-core/
    /// </summary>
    public class AppDomain : SingletonBase<AppDomain>
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="AppDomain"/> class from being created.
        /// </summary>
        private AppDomain()
        {
            // placeholder
        }

        /// <summary>
        /// Gets the current domain.
        /// </summary>
        /// <value>
        /// The current domain.
        /// </value>
        public static AppDomain CurrentDomain => Instance;

        /// <summary>
        /// Gets all the assemblies in the current app domain.
        /// </summary>
        /// <returns></returns>
        public Assembly[] GetAssemblies()
        {
            var dependencies = DependencyContext.Default.RuntimeLibraries;
            var assemblies = new List<Assembly>();

            foreach (var library in dependencies.Where(IsCandidateCompilationLibrary))
            {
                assemblies.Add(SafeLoadAssemblyByName(library.Name));
                assemblies.AddRange(library.Dependencies.Select(x => SafeLoadAssemblyByName(x.Name)));
            }

            return assemblies.Where(x => x != null).ToArray();
        }

        private static Assembly SafeLoadAssemblyByName(string assemblyName)
        {
            try
            {
                return Assembly.Load(new AssemblyName(assemblyName));
            }
            catch
            {
                return null;
            }
        }

        private static bool IsCandidateCompilationLibrary(RuntimeLibrary compilationLibrary)
        {
            return compilationLibrary.Name == CurrentApp.EntryAssemblyName.Name
                   ||
                   compilationLibrary.Dependencies.Any(d => d.Name.StartsWith(CurrentApp.EntryAssemblyName.Name));
        }
    }
}