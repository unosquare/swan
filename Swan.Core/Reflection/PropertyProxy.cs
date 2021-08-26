using Swan.Extensions;
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
        private readonly Func<object, object?>? Getter;
        private readonly Action<object, object?>? Setter;
        private readonly Lazy<object[]> AttributesLazy;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyProxy"/> class.
        /// </summary>
        /// <param name="declaringType">Type of the declaring.</param>
        /// <param name="propertyInfo">The property information.</param>
        public PropertyProxy(Type declaringType, PropertyInfo propertyInfo)
        {
            Property = propertyInfo;
            EnclosingType = declaringType;
            Getter = CreateLambdaGetter(declaringType, propertyInfo);
            Setter = CreateLambdaSetter(declaringType, propertyInfo);
            HasPublicGetter = propertyInfo.GetGetMethod()?.IsPublic ?? false;
            HasPublicSetter = propertyInfo.GetSetMethod()?.IsPublic ?? false;
            AttributesLazy = new(() => propertyInfo.GetCustomAttributes(true), true);
        }

        /// <inheritdoc />
        public PropertyInfo Property { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<object> Attributes => AttributesLazy.Value;

        /// <inheritdoc />
        public Type EnclosingType { get; }

        /// <inheritdoc />
        public string Name => Property.Name;

        /// <inheritdoc />
        public Type PropertyType => Property.PropertyType;

        /// <inheritdoc />
        public bool CanRead => Property.CanRead;

        /// <inheritdoc />
        public bool CanWrite => Property.CanWrite;

        /// <summary>
        /// Gets whether the property getter is declared as public.
        /// </summary>
        public bool HasPublicGetter { get; }

        /// <summary>
        /// Gets whether the property setter is declared as public.
        /// </summary>
        public bool HasPublicSetter { get; }

        /// <inheritdoc />
        public object? GetValue(object instance) => instance is null
            ? throw new ArgumentNullException(nameof(instance))
            : Getter is null
            ? throw new MissingMethodException($"Object of type '{instance.GetType().Name}' has no getter for property '{Name}'")
            : Getter.Invoke(instance);

        /// <inheritdoc />
        public void SetValue(object instance, object? value)
        {
            if (instance is null)
                throw new ArgumentNullException(nameof(instance));

            if (Setter is null)
                throw new MissingMethodException($"Object of type '{instance.GetType().Name}' has no setter for property '{Name}'");

            Setter.Invoke(instance, value);
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
            if (!Property.CanWrite)
                return false;

            var sourceType = value is null ? PropertyType : value.GetType();
            var sourceValue = value ?? sourceType.GetDefault();

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
    }
}