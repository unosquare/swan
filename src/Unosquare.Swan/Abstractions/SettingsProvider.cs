namespace Unosquare.Swan.Abstractions
{
    using Reflection;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Unosquare.Swan.Formatters;

    /// <summary>
    /// Represents a provider to save and load settings using a plain JSON file
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SettingsProvider<T> : SingletonBase<SettingsProvider<T>>
    {
        private T _global;

        /// <summary>
        /// Gets or sets the configuration file path. By default the entry assembly directory is used
        /// and the filename is appsettings.json.
        /// </summary>
        public string ConfigurationFilePath { get; set; } = Path.Combine(CurrentApp.EntryAssemblyDirectory, "appsettings.json");

        /// <summary>
        /// Gets the global settings object
        /// </summary>
        public T Global
        {
            get
            {
                lock (SyncRoot)
                {
                    if (_global == null)
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
            lock (SyncRoot)
            {
                if (File.Exists(ConfigurationFilePath) == false || File.ReadAllText(ConfigurationFilePath).Length == 0)
                {
                    _global = Activator.CreateInstance<T>();
                    PersistGlobalSettings();
                }
                
                _global = Json.Deserialize<T>(File.ReadAllText(ConfigurationFilePath));
            }
        }

        /// <summary>
        /// Gets the json data.
        /// </summary>
        /// <returns></returns>
        public string GetJsonData()
        {
            lock (SyncRoot)
            {
                return File.ReadAllText(ConfigurationFilePath);
            }
        }

        /// <summary>
        /// Persists the global settings.
        /// </summary>
        public void PersistGlobalSettings()
        {
            lock (SyncRoot)
            {
                var stringData = Json.Serialize(Global);
                File.WriteAllText(ConfigurationFilePath, stringData);
            }
        }

        /// <summary>
        /// Updates settings from list.
        /// </summary>
        /// <param name="list">The list.</param>
        public void UpdateFromList(List<ExtendedPropertyInfo> list)
        {
            foreach (var property in list)
            {
                var prop = Instance.Global.GetType().GetTypeInfo().GetProperty(property.Property);
                // TODO: evaluate if the value changed and report back
                //var originalValue = prop.GetValue(Current.Global);

                if (prop.PropertyType == typeof(bool))
                    prop.SetValue(Instance.Global, Convert.ToBoolean(property.Value));
                else if (prop.PropertyType == typeof(int))
                    prop.SetValue(Instance.Global, Convert.ToInt32(property.Value));
                else if (prop.PropertyType == typeof(int?))
                {
                    prop.SetValue(Instance.Global,
                        property.Value == null ? property.Value : Convert.ToInt32(property.Value));
                }
                else if (prop.PropertyType == typeof(int[]))
                    prop.SetValue(Instance.Global, property.Value.ToString().Split(',').Select(int.Parse).ToArray());
                else if (prop.PropertyType == typeof(string[]))
                    prop.SetValue(Instance.Global, property.Value.ToString().Split(',').ToArray());
                else
                    prop.SetValue(Instance.Global, property.Value);

                Instance.PersistGlobalSettings();
            }
        }
        
        /// <summary>
        /// Gets the list.
        /// </summary>
        /// <returns></returns>
        internal List<ExtendedPropertyInfo<T>> GetList()
        {
            var dict = Json.Deserialize<Dictionary<string, object>>(GetJsonData());

            return dict.Keys
                    .Select(x => new ExtendedPropertyInfo<T>(x) { Value = dict[x] })
                    .ToList();
        }
    }
}