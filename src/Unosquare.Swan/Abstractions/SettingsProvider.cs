namespace Unosquare.Swan.Abstractions
{
    using Reflection;
    using System;
    using System.Collections;
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
        public List<string> RefreshFromList(List<ExtendedPropertyInfo<T>> list)
        {
            List<string> changedSettings = new List<string>();

            foreach (var property in list)
            {
                var prop = Current.Global.GetType().GetTypeInfo().GetProperty(property.Property);
                var originalValue = prop.GetValue(Current.Global);
                bool isChanged = false;

                if (prop.PropertyType.IsArray)
                {
                    var itemType = prop.PropertyType.GetElementType();

                    var coll = property.Value as IEnumerable;
                    if (coll == null) continue;

                    var arr = Array.CreateInstance(itemType, coll.Cast<object>().Count());

                    var i = 0;
                    foreach (var value in coll)
                    {
                        object itemvalue;
                        if (Constants.BasicTypesInfo[itemType].TryParse(value.ToString(), out itemvalue))
                            arr.SetValue(itemvalue, i++);
                    }

                    prop.SetValue(Current.Global, arr);
                }
                else
                {
                    if (property.Value == null)
                    {
                        if (originalValue == null) continue;

                        isChanged = true;
                        prop.SetValue(Current.Global, null);
                    }
                    else
                    {
                        object propertyValue;
                        if (Constants.BasicTypesInfo[prop.PropertyType].TryParse(property.Value.ToString(), out propertyValue))
                        {
                            if (propertyValue == originalValue) continue;

                            isChanged = true;
                            prop.SetValue(Current.Global, property.Value);
                        }
                    }
                }

                if (isChanged)
                {
                    changedSettings.Add(property.Property);
                    Current.PersistGlobalSettings();
                }
            }

            return changedSettings;
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