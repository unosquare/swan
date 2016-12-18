namespace Unosquare.Swan.Runtime
{
    using Abstractions;
    using Microsoft.Extensions.DependencyModel;
    using System.Collections.Generic;
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
        static public AppDomain CurrentDomain { get { return Instance; } }

        /// <summary>
        /// Gets all the assemblies in teh current app domain.
        /// </summary>
        /// <returns></returns>
        public Assembly[] GetAssemblies()
        {
            var assemblies = new List<Assembly>();
            var dependencies = DependencyContext.Default.RuntimeLibraries;
            foreach (var library in dependencies)
            {
                if (IsCandidateCompilationLibrary(library))
                {
                    var assembly = Assembly.Load(new AssemblyName(library.Name));
                    assemblies.Add(assembly);
                }
            }
            return assemblies.ToArray();
        }

        private static bool IsCandidateCompilationLibrary(RuntimeLibrary compilationLibrary)
        {
            return compilationLibrary.Name == ("Specify")
                || compilationLibrary.Dependencies.Any(d => d.Name.StartsWith("Specify"));
        }
    }
}
