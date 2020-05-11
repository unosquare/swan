using System;
using System.Linq;
using System.Reflection;
using Swan.Reflection;

namespace Swan.Parsers
{
    /// <summary>
    /// Provides methods to parse command line arguments.
    /// </summary>
    public partial class ArgumentParser
    {
        private sealed class TypeResolver<T>
        {
            public bool HasVerb { get; }

            private bool _hasVerb = false;

            private readonly string _selectedVerb;

            private PropertyInfo[]? _properties;

            public TypeResolver(string selectedVerb)
            {
                _selectedVerb = selectedVerb;
            }

            public PropertyInfo[]? Properties => _properties?.Any() == true ? _properties : null;

            public object? GetOptionsObject(T instance)
            {
                _properties = PropertyTypeCache.DefaultCache.Value.RetrieveAllProperties<T>(true).ToArray();

                if (!_properties.Any(x => x.GetCustomAttributes(typeof(VerbOptionAttribute), false).Any()))
                    return instance;

                _hasVerb = true;

                var selectedVerb = string.IsNullOrWhiteSpace(_selectedVerb)
                    ? null
                    : _properties.FirstOrDefault(x =>
                        AttributeCache.DefaultCache.Value.RetrieveOne<VerbOptionAttribute>(x).Name == _selectedVerb);

                if (selectedVerb == null) return null;

                var type = instance.GetType();

                var verbProperty = type.GetProperty(selectedVerb.Name);

                if (verbProperty?.GetValue(instance) == null)
                {
                    var propertyInstance = Activator.CreateInstance(selectedVerb.PropertyType);
                    verbProperty?.SetValue(instance, propertyInstance);
                }

                _properties = PropertyTypeCache.DefaultCache.Value.RetrieveAllProperties(selectedVerb.PropertyType, true)
                    .ToArray();

                return verbProperty?.GetValue(instance);
            }
        }
    }
}
