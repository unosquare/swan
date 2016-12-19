using System.IO;
using Unosquare.Swan.Formatters;

namespace Unosquare.Swan.Runtime
{
    using Abstractions;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Represents a polyfill class to replace interoperability with .net core
    /// Idea taken from: http://www.michael-whelan.net/replacing-appdomain-in-dotnet-core/
    /// </summary>
    public class AppDomain : SingletonBase<AppDomain>
    {
        private static readonly string DepsFilesProperty = "APP_CONTEXT_DEPS_FILES";

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
            var dependencies = GetDependencyContext();
            var assemblies = new List<Assembly>();

            //foreach (var library in dependencies.Where(IsCandidateCompilationLibrary))
            //{
            //    assemblies.Add(SafeLoadAssemblyByName(library.Name));
            //    assemblies.AddRange(library.Dependencies.Select(x => SafeLoadAssemblyByName(x.Name)));
            //}

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

        //private static bool IsCandidateCompilationLibrary(RuntimeLibrary compilationLibrary)
        //{
        //    return compilationLibrary.Name == CurrentApp.EntryAssembly.GetName().Name
        //           ||
        //           compilationLibrary.Dependencies.Any(d => d.Name.StartsWith(CurrentApp.EntryAssembly.GetName().Name));
        //}

        /// <summary>
        /// Gets the dependency context.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> GetDependencyContext()
        {
#if NET452
            var deps = System.AppDomain.CurrentDomain.GetData(DepsFilesProperty);
#else
            var deps = System.AppContext.GetData(DepsFilesProperty);
#endif
            var fileToLoad = (deps as string)?.Split(new[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            var jsonData = JsonFormatter.Deserialize(File.ReadAllText(fileToLoad));

            return jsonData;
        }
    }
}