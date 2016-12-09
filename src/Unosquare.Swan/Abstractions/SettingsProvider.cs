namespace Unosquare.Swan.Abstractions
{
    using System;
    using System.IO;
    using System.Web.Script.Serialization;

    public class SettingsProvider<T> : SingletonBase<SettingsProvider<T>>
    {
        private JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
        private T _global;

        public string ConfigurationFilePath { get; set; } = CurrentApp.EntryAssemblyDirectory;

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

        public void ReloadGlobalSettings()
        {
            lock (SyncRoot)
            {
                if (File.Exists(ConfigurationFilePath) == false)
                {
                    _global = Activator.CreateInstance<T>();
                    PersistGlobalSettings();
                }

                _global = javaScriptSerializer.Deserialize<T>(File.ReadAllText(ConfigurationFilePath));
            }
        }

        public string GetJsonData()
        {
            lock (SyncRoot)
            {
                return File.ReadAllText(ConfigurationFilePath);
            }
        }

        public void PersistGlobalSettings()
        {
            lock (SyncRoot)
            {
                var stringData = javaScriptSerializer.Serialize(Global);
                File.WriteAllText(ConfigurationFilePath, stringData);
            }
        }

    }
}
