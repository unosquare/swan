namespace Unosquare.Swan.Abstractions
{
    using Formatters;
    using Reflection;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Represents a provider to save and load settings using a plain JSON file
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SettingsProvider<T> : SingletonBase<SettingsProvider<T>>
    {
        /// <summary>
        /// A synchronization root that is commonly used for cross-thread operations.
        /// </summary>
        protected readonly object SyncRoot = new object();

        private T _global;

        /// <summary>
        /// Gets or sets the configuration file path. By default the entry assembly directory is used
        /// and the filename is appsettings.json.
        /// </summary>
        public virtual string ConfigurationFilePath { get; set; } =
#if NETSTANDARD1_3 || UWP
            Path.Combine(Runtime.LocalStoragePath, "appsettings.json");
#else
            Path.Combine(Runtime.EntryAssemblyDirectory, "appsettings.json");
#endif

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
                else
                {
                    _global = Json.Deserialize<T>(File.ReadAllText(ConfigurationFilePath));
                }
            }
        }

        /// <summary>
        /// Persists the global settings.
        /// </summary>
        public void PersistGlobalSettings()
        {
            lock (SyncRoot)
            {
                File.WriteAllText(ConfigurationFilePath, Json.Serialize(Global));
            }
        }

        /// <summary>
        /// Updates settings from list.
        /// </summary>
        /// <param name="propertyList">The list.</param>
        public List<string> RefreshFromList(List<ExtendedPropertyInfo<T>> propertyList)
        {
            var changedSettings = new List<string>();

            foreach (var property in propertyList)
            {
                var propertyInfo = Global.GetType().GetProperty(property.Property);
                var originalValue = propertyInfo.GetValue(Global);
                var isChanged = false;

                if (propertyInfo.PropertyType.IsArray)
                {
                    var elementType = propertyInfo.PropertyType.GetElementType();

                    if (property.Value is IEnumerable == false)
                        continue;

                    var sourceArray = ((IEnumerable) property.Value).Cast<object>().ToArray();
                    var targetArray = Array.CreateInstance(elementType, sourceArray.Length);

                    var i = 0;
                    foreach (var sourceElement in sourceArray)
                    {
                        try
                        {
                            if (sourceElement == null)
                            {
                                targetArray.SetValue(null, i++);
                                continue;
                            }

                            object itemvalue;
                            if (Definitions.BasicTypesInfo[elementType].TryParse(sourceElement.ToString(), out itemvalue))
                                targetArray.SetValue(itemvalue, i++);
                        }
                        catch
                        {
                            // swallow
                        }
                    }

                    isChanged = true;
                    propertyInfo.SetValue(Global, targetArray);
                }
                else
                {
                    if (property.Value == null)
                    {
                        if (originalValue == null) continue;

                        isChanged = true;
                        propertyInfo.SetValue(Global, null);
                    }
                    else
                    {
                        object propertyValue;
                        if (Definitions.BasicTypesInfo[propertyInfo.PropertyType].TryParse(property.Value.ToString(),
                            out propertyValue))
                        {
                            if (propertyValue.Equals(originalValue)) continue;

                            isChanged = true;
                            propertyInfo.SetValue(Instance.Global, propertyValue);
                        }
                    }
                }

                if (!isChanged) continue;

                changedSettings.Add(property.Property);
                PersistGlobalSettings();
            }

            return changedSettings;
        }

        /// <summary>
        /// Gets the list.
        /// </summary>
        /// <returns></returns>
        public List<ExtendedPropertyInfo<T>> GetList()
        {
            var jsonData = Json.Deserialize(Json.Serialize(Global)) as Dictionary<string, object>;

            return jsonData?.Keys
                .Select(p => new ExtendedPropertyInfo<T>(p) {Value = jsonData[p]})
                .ToList();
        }

        /// <summary>
        /// Resets the global settings.
        /// </summary>
        public void ResetGlobalSettings()
        {
            lock (SyncRoot)
            {
                var stringData = Json.Serialize(Activator.CreateInstance<T>());
                File.WriteAllText(ConfigurationFilePath, stringData);
            }
        }
    }
}