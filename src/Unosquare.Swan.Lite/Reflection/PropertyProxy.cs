#if !NETSTANDARD1_3
namespace Unosquare.Swan.Reflection
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Abstractions;

    /// <summary>
    /// Represents a generic class to store getters and setters.
    /// </summary>
    /// <typeparam name="TClass">The type of the class.</typeparam>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <seealso cref="IPropertyProxy" />
    public sealed class PropertyProxy<TClass, TProperty> : IPropertyProxy
        where TClass : class
    {
        private readonly Func<TClass, TProperty> _getter;
        private readonly Action<TClass, TProperty> _setter;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyProxy{TClass, TProperty}"/> class.
        /// </summary>
        /// <param name="property">The property.</param>
        public PropertyProxy(PropertyInfo property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            var getterInfo = property.GetGetMethod(false);
            if (getterInfo != null)
                _getter = (Func<TClass, TProperty>)Delegate.CreateDelegate(typeof(Func<TClass, TProperty>), getterInfo);

            var setterInfo = property.GetSetMethod(false);
            if (setterInfo != null)
                _setter = (Action<TClass, TProperty>)Delegate.CreateDelegate(typeof(Action<TClass, TProperty>), setterInfo);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        object IPropertyProxy.GetValue(object instance) =>
            _getter(instance as TClass);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IPropertyProxy.SetValue(object instance, object value) =>
            _setter(instance as TClass, (TProperty)value);
    }
}
#endif