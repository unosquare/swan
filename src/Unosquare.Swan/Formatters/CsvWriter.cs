namespace Unosquare.Swan.Formatters
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// A CSV writer for 
    /// </summary>
    public class CsvWriter : IDisposable
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> TypeCache = new ConcurrentDictionary<Type, PropertyInfo[]>();
        private Stream OutputStream = null;
        private Encoding Encoding = null;

        public string Separator { get; set; }
        public List<string> IgnoreProperties { get; private set; }
        public string EndOfLine { get; set; }

        private CsvWriter()
        {
            this.Separator = ",";
            this.IgnoreProperties = new List<string>();
        }

        public CsvWriter(Stream outputStream, Encoding encoding)
            : this()
        {
            OutputStream = outputStream;
            Encoding = encoding;
            EndOfLine = Environment.NewLine;
        }

        public async Task WriteLineAsync(object item)
        {
            var properties = GetFilteredProperties(item.GetType());
            var valuesList = new List<object>();

            foreach (var property in properties)
            {
                try
                {
                    var value = property.GetValue(item, null);
                    valuesList.Add(value);
                }
                catch
                {
                    valuesList.Add(null);
                }
            }

            await WriteValuesAsync(valuesList.ToArray());

        }

        public async Task WriteHeaderAsync(Type type)
        {
            var properties = GetFilteredProperties(type);
            var valuesList = properties.Select(p => p.Name).ToArray();
            await WriteValuesAsync(valuesList);
        }

        public async Task WriteHeaderAsync<T>()
        {
            await WriteHeaderAsync(typeof(T));
        }

        public async Task WriteValuesAsync(object[] values)
        {
            var length = values.Count();
            var separatorBytes = Encoding.GetBytes(Separator);
            var endOfLineBytes = Encoding.GetBytes(EndOfLine);

            for (var i = 0; i < length; i++)
            {
                var value = values[i];
                var stringVal = value == null ? string.Empty : value.ToString();
                var enclose = stringVal.IndexOf(',') >= 0 || stringVal.IndexOf('"') >= 0;
                stringVal = stringVal.Replace("\"", "\"\"");
                if (enclose) stringVal = string.Format("\"{0}\"", stringVal);

                var output = Encoding.GetBytes(stringVal);
                await this.OutputStream.WriteAsync(output, 0, output.Length);

                if (i < length - 1)
                    await this.OutputStream.WriteAsync(separatorBytes, 0, separatorBytes.Length);
            }

            await this.OutputStream.WriteAsync(endOfLineBytes, 0, endOfLineBytes.Length);
        }

        private PropertyInfo[] GetFilteredProperties(Type type)
        {
            var properties = GetPropertyInfo(type);
            properties = properties.Where(p => IgnoreProperties.Contains(p.Name) == false && p.CanRead).ToArray();
            return properties;
        }

        private static PropertyInfo[] GetPropertyInfo(Type type)
        {
            if (TypeCache.ContainsKey(type))
                return TypeCache[type];

            PropertyInfo[] properties = type.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            TypeCache[type] = properties;
            return properties;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                //if (managedResource != null)
                //{
                //    managedResource.Dispose();
                //    managedResource = null;
                //}
            }
        }
    }
}
