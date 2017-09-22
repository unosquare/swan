#if !NETSTANDARD1_3 && !UWP
namespace Unosquare.Swan.Reflection
{
    using Abstractions;
    using System.Reflection;
#if !NET452
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;
    using Formatters;
#endif

    /// <summary>
    /// Represents a polyfill class to replace interoperability with .net core
    /// Idea taken from: http://www.michael-whelan.net/replacing-appdomain-in-dotnet-core/
    /// </summary>
    internal class AppDomain : SingletonBase<AppDomain>
    {
#if !NET452
        private const string DepsFilesProperty = "APP_CONTEXT_DEPS_FILES";

        /// <summary>
        /// The dependency context
        /// </summary>
        private readonly System.Lazy<Dictionary<string, object>> dependencyContext = new System.Lazy<Dictionary<string, object>>(() =>
        {
            var deps = System.AppContext.GetData(DepsFilesProperty);
            var fileToLoad =
                (deps as string)?.Split(new[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            var jsonData = Json.Deserialize(File.ReadAllText(fileToLoad)) as Dictionary<string, object>;

            return jsonData;
        });
#endif

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
        /// <returns>The assemblies in the AppDomain</returns>
        public Assembly[] GetAssemblies()
        {
#if NET452
            return System.AppDomain.CurrentDomain.GetAssemblies();
#else
            var assemblies = new List<Assembly>();
            var runtimeAssemblies = GetRuntimeAssemblies().Where(x => x.Key.Name == Runtime.EntryAssembly.GetName().Name);

            // TODO: Check at dependencies?
            foreach (var library in runtimeAssemblies)
            {
                assemblies.Add(SafeLoadAssemblyByName(library.Key.Name));
                var depInfo = library.Value as Dictionary<string, object>;

                if (depInfo == null || !depInfo.ContainsKey("dependencies")) continue;
                var deps = depInfo["dependencies"] as Dictionary<string, object>;

                if (deps != null)
                    assemblies.AddRange(deps.Select(x => SafeLoadAssemblyByName(x.Key)));
            }

            return assemblies.Where(x => x != null).ToArray();
#endif
        }

#if !NET452
        /// <summary>
        /// Gets the dependency context.
        /// </summary>
        /// <returns>The Dependency Context</returns>
        public Dictionary<string, object> GetDependencyContext() => dependencyContext.Value;

        /// <summary>
        /// Gets the get runtime target.
        /// </summary>
        /// <value>
        /// The get runtime target.
        /// </value>
        public string GetRuntimeTarget
        {
            get
            {
                var runtimeDict = GetDependencyContext()["runtimeTarget"] as Dictionary<string, object>;

                return runtimeDict != null && runtimeDict.ContainsKey("name")
                    ? runtimeDict["name"].ToString()
                    : string.Empty;
            }
        }

        /// <summary>
        /// Gets the runtime assemblies.
        /// </summary>
        /// <returns>The runtime assemblies in the AppDomain</returns>
        public Dictionary<AssemblyInfo, object> GetRuntimeAssemblies()
        {
            var targets = GetDependencyContext()["targets"] as Dictionary<string, object>;
            var runtimeTarget = GetRuntimeTarget;

            var assemblies = targets != null && targets.ContainsKey(runtimeTarget)
                ? targets[runtimeTarget] as Dictionary<string, object>
                : new Dictionary<string, object>();

            return assemblies.ToDictionary(x => new AssemblyInfo(x.Key), x => x.Value);
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
#endif
    }
}
#endif