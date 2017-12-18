namespace Unosquare.Swan.Components
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Reflection;

    /// <summary>
    /// Represents an AutoMapper-like object to map from one object type
    /// to another using defined properties map or using the default behaviour
    /// to copy same named properties from one object to another.
    /// 
    /// The extension methods like CopyPropertiesTo use the default behaviour.
    /// </summary>
    public class ObjectMapper
    {
        private readonly List<IObjectMap> _maps = new List<IObjectMap>();

        /// <summary>
        /// Copies the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="propertiesToCopy">The properties to copy.</param>
        /// <param name="ignoreProperties">The ignore properties.</param>
        /// <returns>
        /// Copied properties count
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// source
        /// or
        /// target
        /// </exception>
        public static int Copy(
            object source,
            object target,
            string[] propertiesToCopy = null,
            string[] ignoreProperties = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (target == null)
                throw new ArgumentNullException(nameof(target));

            // select distinct properties because they can be duplicated by inheritance
            var sourceProperties = GetTypeProperties(source.GetType())
                .Where(x => x.CanRead)
                .ToArray();

            return Copy(
                target,
                propertiesToCopy,
                ignoreProperties,
                sourceProperties
                    .Select(x => x.Name)
                    .Distinct()
                    .ToDictionary(
                        x => x.ToLowerInvariant(),
                        x => new TypeValuePair(sourceProperties.First(y => y.Name == x).PropertyType,
                            sourceProperties.First(y => y.Name == x).GetValue(source))));
        }

        /// <summary>
        /// Copies the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="propertiesToCopy">The properties to copy.</param>
        /// <param name="ignoreProperties">The ignore properties.</param>
        /// <returns>
        /// Copied properties count
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// source
        /// or
        /// target
        /// </exception>
        public static int Copy(
            IDictionary<string, object> source,
            object target,
            string[] propertiesToCopy = null,
            string[] ignoreProperties = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (target == null)
                throw new ArgumentNullException(nameof(target));

            return Copy(target, propertiesToCopy, ignoreProperties, source.ToDictionary(x => x.Key.ToLowerInvariant(), x => new TypeValuePair(typeof(object), x.Value)));
        }

        /// <summary>
        /// Creates the map.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TDestination">The type of the destination.</typeparam>
        /// <returns>
        /// An object map representation of type of the destination property 
        /// and type of the source property
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// You can't create an existing map
        /// or
        /// Types doesn't match
        /// </exception>
        public ObjectMap<TSource, TDestination> CreateMap<TSource, TDestination>()
        {
            if (_maps.Any(x => x.SourceType == typeof(TSource) && x.DestinationType == typeof(TDestination)))
            {
                throw new InvalidOperationException("You can't create an existing map");
            }

            var sourceType = GetTypeProperties(typeof(TSource));
            var destinationType = GetTypeProperties(typeof(TDestination));

            var intersect = sourceType.Intersect(destinationType, new PropertyInfoComparer()).ToArray();

            if (intersect.Any() == false)
            {
                throw new InvalidOperationException("Types doesn't match");
            }

            var objMap = new ObjectMap<TSource, TDestination>(intersect);

            _maps.Add(objMap);

            return objMap;
        }

        /// <summary>
        /// Maps the specified source.
        /// </summary>
        /// <typeparam name="TDestination">The type of the destination.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="autoResolve">if set to <c>true</c> [automatic resolve].</param>
        /// <returns>
        /// A new instance of the map
        /// </returns>
        /// <exception cref="ArgumentNullException">source</exception>
        /// <exception cref="InvalidOperationException">You can't map from type {source.GetType().Name} to {typeof(TDestination).Name}</exception>
        public TDestination Map<TDestination>(object source, bool autoResolve = true)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var destination = Activator.CreateInstance<TDestination>();
            var map = _maps
                .FirstOrDefault(x => x.SourceType == source.GetType() && x.DestinationType == typeof(TDestination));

            if (map != null)
            {
                foreach (var property in map.Map)
                {
                    var finalSource = property.Value.Aggregate(source, (current, sourceProperty) => sourceProperty.GetValue(current));

                    property.Key.SetValue(destination, finalSource);
                }
            }
            else
            {
                if (autoResolve == false)
                {
                    throw new InvalidOperationException(
                        $"You can't map from type {source.GetType().Name} to {typeof(TDestination).Name}");
                }

                // Missing mapping, try to use default behavior
                Copy(source, destination);
            }

            return destination;
        }

        private static int Copy(
            object target,
            IEnumerable<string> propertiesToCopy,
            IEnumerable<string> ignoreProperties,
            Dictionary<string, TypeValuePair> sourceProperties)
        {
            var copiedProperties = 0;

            // Targets
            var targetType = target.GetType();
            var targetProperties = GetTypeProperties(targetType)
                .Where(x => x.CanWrite)
                .ToList();

            // Filter properties
            var targetPropertyNames = targetProperties
                .Select(t => t.Name.ToLowerInvariant());

            var filteredSourceProperties = sourceProperties
                .Where(s => targetPropertyNames.Contains(s.Key))
                .ToArray();

            var requiredProperties = propertiesToCopy?.Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.ToLowerInvariant())
                .ToArray();

            var ignoredProperties = ignoreProperties?.Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.ToLowerInvariant())
                .ToArray();

            // Copy source properties
            foreach (var sourceProperty in filteredSourceProperties)
            {
                var targetProperty = targetProperties
                    .First(s => s.Name.ToLowerInvariant() == sourceProperty.Key);

                if (requiredProperties != null && !requiredProperties.Contains(targetProperty.Name.ToLowerInvariant()))
                    continue;

                if (ignoredProperties != null && ignoredProperties.Contains(targetProperty.Name.ToLowerInvariant()))
                    continue;

                try
                {
                    var valueType = sourceProperty.Value;

                    if (valueType.Type.GetTypeInfo().IsEnum)
                    {
                        targetProperty.SetValue(target,
                            Enum.ToObject(targetProperty.PropertyType, valueType.Value));
                        continue;
                    }

                    if (!valueType.Type.IsValueType() && targetProperty.PropertyType == valueType.Type)
                    {
                        targetProperty.SetValue(
                            target,
                            valueType.Value != null ? GetValue(valueType.Value, targetProperty.PropertyType) : null);

                        copiedProperties++;
                        continue;
                    }

                    // String to target type conversion
                    if (targetProperty.PropertyType.TryParseBasicType(valueType.Value.ToStringInvariant(),
                        out var targetValue))
                    {
                        targetProperty.SetValue(target, targetValue);
                        copiedProperties++;
                    }
                }
                catch
                {
                    // swallow
                }
            }

            return copiedProperties;
        }

        private static object GetValue(object source, Type targetType)
        {
            object target = null;

            source.CreateTarget(targetType, false, ref target);

            if (source is string)
            {
                target = source;
            }
            else if (source is IList sourceList)
            {
                var targetArray = target as Array;
                var targetList = target as IList;

                // Case 2.1: Source is List, Target is Array
                if (targetArray != null)
                {
                    for (var i = 0; i < sourceList.Count; i++)
                    {
                        try
                        {
                            targetArray.SetValue(
                                sourceList[i].GetType().IsValueType()
                                    ? sourceList[i]
                                    : sourceList[i].CopyPropertiesToNew<object>(), i);
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
                else if (targetList != null)
                {
                    // Case 2.2: Source is List,  Target is IList
                    // find the add method of the target list
                    var addMethod = targetType.GetMethods()
                        .FirstOrDefault(
                            m => m.Name.Equals(Formatters.Json.AddMethodName) && m.IsPublic &&
                                 m.GetParameters().Length == 1);

                    if (addMethod == null) return target;

                    foreach (var item in sourceList)
                    {
                        try
                        {
                            targetList.Add(item.GetType().IsValueType() ? item : item.CopyPropertiesToNew<object>());
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
            }
            else
            {
                source.CopyPropertiesTo(target);
            }

            return target;
        }

        private static IEnumerable<PropertyInfo> GetTypeProperties(Type type)
            => Runtime.PropertyTypeCache.Value.Retrieve(type, PropertyTypeCache.GetAllPublicPropertiesFunc(type));

        internal class TypeValuePair
        {
            public TypeValuePair(Type type, object value)
            {
                Type = type;
                Value = value;
            }

            public Type Type { get; }    

            public object Value { get; }
        }

        internal class PropertyInfoComparer : IEqualityComparer<PropertyInfo>
        {
            public bool Equals(PropertyInfo x, PropertyInfo y)
            {
                // TODO: Include mapping matcher and types proximity
                return x.Name == y.Name && x.PropertyType == y.PropertyType;
            }

            public int GetHashCode(PropertyInfo obj)
                => obj.Name.GetHashCode() + obj.PropertyType.Name.GetHashCode();
        }
    }
}