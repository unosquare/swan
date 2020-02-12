using Swan.Formatters;
using System;
using System.IO;

namespace Swan.Configuration
{
    /// <summary>
    /// Represents a provider to save and load settings using a plain JSON file.
    /// </summary>
    /// <example>
    /// The following example shows how to save and load settings.
    /// <code>
    /// using Swan.Configuration;
    /// 
    /// public class Example
    /// { 
    ///     public static void Main()
    ///     {
    ///         // get user from settings
    ///         var user = SettingsProvider&lt;Settings&gt;.Instance.Global.User;
    ///             
    ///         // modify the port
    ///         SettingsProvider&lt;Settings&gt;.Instance.Global.Port = 20;
    ///             
    ///         // if we want these settings to persist
    ///         SettingsProvider&lt;Settings&gt;.Instance.PersistGlobalSettings();
    ///     }
    ///         
    ///     public class Settings
    ///     {
    ///         public int Port { get; set; } = 9696;
    ///         
    ///         public string User { get; set; } = "User";
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <typeparam name="T">The type of settings model.</typeparam>
    public sealed class SettingsProvider<T>
        : SingletonBase<SettingsProvider<T>>
    {
        private readonly object _syncRoot = new object();

        private T _global;

        /// <summary>
        /// Gets or sets the configuration file path. By default the entry assembly directory is used
        /// and the filename is 'appsettings.json'.
        /// </summary>
        /// <value>
        /// The configuration file path.
        /// </value>
        public string ConfigurationFilePath { get; set; } =
            Path.Combine(SwanRuntime.EntryAssemblyDirectory, "appsettings.json");

        /// <summary>
        /// Gets the global settings object.
        /// </summary>
        /// <value>
        /// The global settings object.
        /// </value>
        public T Global
        {
            get
            {
                lock (_syncRoot)
                {
                    if (Equals(_global, default(T)))
                        ReloadGlobalSettings();

                    return _global;
                }
            }
        }

        /// <summary>
        /// Reloads the global settings.
        /// </summary>
        public void ReloadGlobalSettings()
        {
            if (File.Exists(ConfigurationFilePath) == false || File.ReadAllText(ConfigurationFilePath).Length == 0)
            {
                ResetGlobalSettings();
                return;
            }

            lock (_syncRoot)
                _global = Json.Deserialize<T>(File.ReadAllText(ConfigurationFilePath));
        }

        /// <summary>
        /// Persists the global settings.
        /// </summary>
        public void PersistGlobalSettings() => File.WriteAllText(ConfigurationFilePath, Json.Serialize(Global, true));

        /// <summary>
        /// Resets the global settings.
        /// </summary>
        public void ResetGlobalSettings()
        {
            lock (_syncRoot)
                _global = Activator.CreateInstance<T>();

            PersistGlobalSettings();
        }
    }
}
