namespace Unosquare.Swan.Components
{
    using System.Linq;
    using System.Reflection;
    using Attributes;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides methods to parse command line arguments.
    /// Based on CommandLine (Copyright 2005-2015 Giacomo Stelluti Scala and Contributors.)
    /// </summary>
    public partial class ArgumentParser
    {
        internal class TypeResolver<T>
        {
            private readonly string _selectedVerb;
            
            private PropertyInfo[] _properties;
            
            public TypeResolver(string selectedVerb)
            {
                _selectedVerb = selectedVerb;
            }
            
            public PropertyInfo[] GetProperties() => _properties?.Any() == true ? _properties : null;

            public object GetOptionsObject(T instance)
            {
                _properties = Runtime.PropertyTypeCache.RetrieveAllProperties<T>(true).ToArray();

                if (!_properties.Any(x => x.GetCustomAttributes(typeof(VerbOptionAttribute), false).Any()))
                    return instance;

                var selectedVerb = string.IsNullOrWhiteSpace(_selectedVerb)
                    ? null
                    : _properties.FirstOrDefault(x =>
                        Runtime.AttributeCache.RetrieveOne<VerbOptionAttribute>(x).Name.Equals(_selectedVerb));

                if (selectedVerb == null)
                {
                    return null;
                }

                var type = instance.GetType();
                var verbName = selectedVerb.Name;

                if (type.GetProperty(verbName).GetValue(instance) == null)
                {
                    var propertyInstance = Activator.CreateInstance(selectedVerb.PropertyType);
                    type.GetProperty(verbName).SetValue(instance, propertyInstance);
                }

                _properties = Runtime.PropertyTypeCache.RetrieveAllProperties(selectedVerb.PropertyType, true)
                    .ToArray();

                return type.GetProperty(verbName).GetValue(instance);
            }
        }
    }
}