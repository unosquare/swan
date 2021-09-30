namespace Swan.Reflection
{
#pragma warning disable CA1031 // Do not catch general exception types
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// The concrete and hidden implementation of the <see cref="IPropertyProxy"/> implementation.
    /// </summary>
    /// <seealso cref="IPropertyProxy" />
    internal sealed class PropertyProxy : IPropertyProxy
    {
        private readonly Func<object, object?>? Getter;
        private readonly Action<object, object?>? Setter;
        private readonly Lazy<object[]> PropertyAttributesLazy;
        private readonly Lazy<ITypeInfo> PropertyTypeLazy;
        private readonly Lazy<bool> HasPublicGetterLazy;
        private readonly Lazy<bool> HasPublicSetterLazy;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyProxy"/> class.
        /// </summary>
        /// <param name="parentType">The type that owns this property proxy.</param>
        /// <param name="propertyInfo">The property information.</param>
        public PropertyProxy(ITypeInfo parentType, PropertyInfo propertyInfo)
        {
            ParentType = parentType;
            PropertyInfo = propertyInfo;
            Getter = CreateLambdaGetter(parentType, propertyInfo);
            Setter = CreateLambdaSetter(parentType, propertyInfo);
            HasPublicGetterLazy = new(() => propertyInfo.GetGetMethod()?.IsPublic ?? false, true);
            HasPublicSetterLazy = new(() => propertyInfo.GetSetMethod()?.IsPublic ?? false, true);
            PropertyAttributesLazy = new(() => propertyInfo.GetCustomAttributes(true), true);
            PropertyTypeLazy = new(() => propertyInfo.PropertyType.TypeInfo(), true);
        }

        /// <inheritdoc />
        public ITypeInfo ParentType { get; }

        /// <inheritdoc />
        public ITypeInfo PropertyType => PropertyTypeLazy.Value;

        /// <inheritdoc />
        public PropertyInfo PropertyInfo { get; }

        /// <inheritdoc />
        public object? DefaultValue => PropertyType.DefaultValue;

        /// <inheritdoc />
        public string PropertyName => PropertyInfo.Name;

        /// <inheritdoc />
        public bool CanRead => PropertyInfo.CanRead;

        /// <inheritdoc />
        public bool CanWrite => PropertyInfo.CanWrite;

        /// <inheritdoc />
        public bool HasPublicGetter => HasPublicGetterLazy.Value;

        /// <inheritdoc />
        public bool HasPublicSetter => HasPublicSetterLazy.Value;

        /// <inheritdoc />
        public IReadOnlyList<object> PropertyAttributes => PropertyAttributesLazy.Value;

        /// <inheritdoc />
        public object? Read(object instance) => instance is null
            ? throw new ArgumentNullException(nameof(instance))
            : Getter is null
            ? throw new MissingMethodException($"Object of type '{instance.GetType().Name}' has no getter for property '{PropertyName}'")
            : Getter.Invoke(instance);

        /// <inheritdoc />
        public void Write(object instance, object? value)
        {
            if (instance is null)
                throw new ArgumentNullException(nameof(instance));

            if (Setter is null)
                throw new MissingMethodException($"Object of type '{instance.GetType().Name}' has no setter for property '{PropertyName}'");

            Setter.Invoke(instance, value);
        }

        /// <inheritdoc />
        public bool TryRead(object instance, [MaybeNullWhen(false)] out object? value)
        {

            value = DefaultValue;
            try
            {
                if (!CanRead)
                    return false;

                value = Read(instance);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc />
        public bool TryWrite(object instance, object? value)
        {
            if (!PropertyInfo.CanWrite)
                return false;

            try
            {
                if (TypeManager.TryChangeType(value, PropertyType, out var sourceValue))
                {
                    Write(instance, sourceValue);
                    return true;
                }
            }
            catch
            {
                // ignore
            }

            return false;
        }

        /// <inheritdoc />
        public override string ToString() =>
            $"Property: {ParentType.ShortName}.{PropertyName} ({PropertyType.ShortName})";

        private static Func<object, object?>? CreateLambdaGetter(ITypeInfo instanceType, PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanRead)
                return null;

            var instanceParameter = Expression.Parameter(typeof(object), "instance");
            var typedInstance = Expression.Convert(instanceParameter, instanceType.NativeType);
            var property = Expression.Property(typedInstance, propertyInfo);
            var convert = Expression.Convert(property, typeof(object));
            var dynamicGetter = (Func<object, object>)Expression.Lambda(convert, instanceParameter).Compile();
            return dynamicGetter;
        }

        private static Action<object, object?>? CreateLambdaSetter(ITypeInfo instanceType, PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanWrite)
                return null;

            var instanceParameter = Expression.Parameter(typeof(object), "instance");
            var valueParameter = Expression.Parameter(typeof(object), "value");

            var typedInstance = Expression.Convert(instanceParameter, instanceType.NativeType);
            var property = Expression.Property(typedInstance, propertyInfo);
            var propertyValue = Expression.Convert(valueParameter, propertyInfo.PropertyType);

            var body = Expression.Assign(property, propertyValue);
            var dynamicSetter = Expression.Lambda<Action<object, object?>>(body, instanceParameter, valueParameter).Compile();

            return dynamicSetter;
        }
    }
}
#pragma warning restore CA1031 // Do not catch general exception types