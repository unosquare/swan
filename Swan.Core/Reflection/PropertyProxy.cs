using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace Swan.Reflection
{
    /// <summary>
    /// The concrete and hidden implementation of the <see cref="IPropertyProxy"/> implementation.
    /// </summary>
    /// <seealso cref="IPropertyProxy" />
    internal sealed class PropertyProxy : IPropertyProxy
    {
        private readonly ITypeProxy TypeProxy;
        private readonly Func<object, object?>? Getter;
        private readonly Action<object, object?>? Setter;
        private readonly Lazy<object[]> PropertyAttributesLazy;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyProxy"/> class.
        /// </summary>
        /// <param name="reflectedType">Type of the declaring.</param>
        /// <param name="propertyInfo">The property information.</param>
        public PropertyProxy(Type reflectedType, PropertyInfo propertyInfo)
        {
            TypeProxy = propertyInfo.PropertyType.TypeInfo();
            PropertyInfo = propertyInfo;
            EnclosingType = reflectedType;
            Getter = CreateLambdaGetter(reflectedType, propertyInfo);
            Setter = CreateLambdaSetter(reflectedType, propertyInfo);
            HasPublicGetter = propertyInfo.GetGetMethod()?.IsPublic ?? false;
            HasPublicSetter = propertyInfo.GetSetMethod()?.IsPublic ?? false;
            PropertyAttributesLazy = new(() => propertyInfo.GetCustomAttributes(true), true);
        }

        /// <inheritdoc />
        public Type PropertyType => PropertyInfo.PropertyType;

        /// <inheritdoc />
        public PropertyInfo PropertyInfo { get; }

        /// <inheritdoc />
        public Type EnclosingType { get; }

        /// <inheritdoc />
        public string PropertyName => PropertyInfo.Name;

        /// <inheritdoc />
        public bool CanRead => PropertyInfo.CanRead;

        /// <inheritdoc />
        public bool CanWrite => PropertyInfo.CanWrite;

        /// <inheritdoc />
        public bool HasPublicGetter { get; }

        /// <inheritdoc />
        public bool HasPublicSetter { get; }

        /// <inheritdoc />
        public Type ProxiedType => TypeProxy.ProxiedType;

        /// <inheritdoc />
        public bool IsNullableValueType => TypeProxy.IsNullableValueType;

        /// <inheritdoc />
        public bool IsNumeric => TypeProxy.IsNumeric;

        /// <inheritdoc />
        public bool IsValueType => TypeProxy.IsValueType;

        /// <inheritdoc />
        public bool IsAbstract => TypeProxy.IsAbstract;

        /// <inheritdoc />
        public bool IsInterface => TypeProxy.IsInterface;

        /// <inheritdoc />
        public bool IsEnum => TypeProxy.IsEnum;

        /// <inheritdoc />
        public bool IsArray => TypeProxy.IsArray;

        /// <inheritdoc />
        public bool IsBasicType => TypeProxy.IsBasicType;

        /// <inheritdoc />
        public Type UnderlyingType => TypeProxy.UnderlyingType;

        /// <inheritdoc />
        public object? DefaultValue => TypeProxy.DefaultValue;

        /// <inheritdoc />
        public bool CanParseNatively => TypeProxy.CanParseNatively;

        /// <inheritdoc />
        public bool CanCreateInstance => TypeProxy.CanCreateInstance;

        /// <inheritdoc />
        public IReadOnlyDictionary<string, IPropertyProxy> Properties => TypeProxy.Properties;

        /// <inheritdoc />
        public IReadOnlyList<FieldInfo> Fields => TypeProxy.Fields;

        /// <inheritdoc />
        public IReadOnlyList<object> PropertyAttributes => PropertyAttributesLazy.Value;

        /// <inheritdoc />
        public IReadOnlyList<object> TypeAttributes => TypeProxy.TypeAttributes;

        /// <inheritdoc />
        public bool IsConstructedGenericType => TypeProxy.IsConstructedGenericType;

        /// <inheritdoc />
        public ITypeProxy? ElementType => TypeProxy.ElementType;

        /// <inheritdoc />
        public bool IsEnumerable => TypeProxy.IsEnumerable;

        /// <inheritdoc />
        public bool IsList => TypeProxy.IsList;

        /// <inheritdoc />
        public IReadOnlyList<Type> Interfaces => TypeProxy.Interfaces;

        /// <inheritdoc />
        public object? GetValue(object instance) => instance is null
            ? throw new ArgumentNullException(nameof(instance))
            : Getter is null
            ? throw new MissingMethodException($"Object of type '{instance.GetType().Name}' has no getter for property '{PropertyName}'")
            : Getter.Invoke(instance);

        /// <inheritdoc />
        public void SetValue(object instance, object? value)
        {
            if (instance is null)
                throw new ArgumentNullException(nameof(instance));

            if (Setter is null)
                throw new MissingMethodException($"Object of type '{instance.GetType().Name}' has no setter for property '{PropertyName}'");

            Setter.Invoke(instance, value);
        }

        /// <inheritdoc />
        public bool TryGetValue(object instance, out object? value)
        {
            
            value = PropertyType.GetDefault();
            try
            {
                if (!CanRead)
                    return false;

                value = GetValue(instance);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc />
        public bool TrySetValue(object instance, object? value)
        {
            if (!PropertyInfo.CanWrite)
                return false;

            var sourceType = value is null ? PropertyType : value.GetType();
            var sourceValue = value ?? DefaultValue;

            try
            {
                if (!PropertyType.IsAssignableFrom(sourceType))
                    sourceValue = Convert.ChangeType(sourceValue, PropertyType, CultureInfo.InvariantCulture);

                SetValue(instance, sourceValue);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc />
        public object CreateInstance() => TypeProxy.CreateInstance();

        /// <inheritdoc />
        public string ToStringInvariant(object? instance) => TypeProxy.ToStringInvariant(instance);

        /// <inheritdoc />
        public bool TryParse(string s, out object? result) => TypeProxy.TryParse(s, out result);

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Property: {EnclosingType}.{PropertyName} ({PropertyType.Name})";
        }

        private static Func<object, object?>? CreateLambdaGetter(Type instanceType, PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanRead)
                return null;

            var instanceParameter = Expression.Parameter(typeof(object), "instance");
            var typedInstance = Expression.Convert(instanceParameter, instanceType);
            var property = Expression.Property(typedInstance, propertyInfo);
            var convert = Expression.Convert(property, typeof(object));
            var dynamicGetter = (Func<object, object>)Expression.Lambda(convert, instanceParameter).Compile();
            return dynamicGetter;
        }

        private static Action<object, object?>? CreateLambdaSetter(Type instanceType, PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanWrite)
                return null;

            var instanceParameter = Expression.Parameter(typeof(object), "instance");
            var valueParameter = Expression.Parameter(typeof(object), "value");

            var typedInstance = Expression.Convert(instanceParameter, instanceType);
            var property = Expression.Property(typedInstance, propertyInfo);
            var propertyValue = Expression.Convert(valueParameter, propertyInfo.PropertyType);

            var body = Expression.Assign(property, propertyValue);
            var dynamicSetter = Expression.Lambda<Action<object, object?>>(body, instanceParameter, valueParameter).Compile();

            return dynamicSetter;
        }
    }
}