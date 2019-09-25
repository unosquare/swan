﻿namespace Swan.DependencyInjection
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Represents an abstract class for Object Factory.
    /// </summary>
    public abstract class ObjectFactoryBase
    {
        /// <summary>
        /// Whether to assume this factory successfully constructs its objects
        /// 
        /// Generally set to true for delegate style factories as CanResolve cannot delve
        /// into the delegates they contain.
        /// </summary>
        public virtual bool AssumeConstruction => false;

        /// <summary>
        /// The type the factory instantiates.
        /// </summary>
        public abstract Type CreatesType { get; }

        /// <summary>
        /// Constructor to use, if specified.
        /// </summary>
        public ConstructorInfo Constructor { get; private set; }

        /// <summary>
        /// Gets the singleton variant.
        /// </summary>
        /// <value>
        /// The singleton variant.
        /// </value>
        /// <exception cref="DependencyContainerRegistrationException">singleton.</exception>
        public virtual ObjectFactoryBase SingletonVariant =>
            throw new DependencyContainerRegistrationException(GetType(), "singleton");

        /// <summary>
        /// Gets the multi instance variant.
        /// </summary>
        /// <value>
        /// The multi instance variant.
        /// </value>
        /// <exception cref="DependencyContainerRegistrationException">multi-instance.</exception>
        public virtual ObjectFactoryBase MultiInstanceVariant =>
            throw new DependencyContainerRegistrationException(GetType(), "multi-instance");

        /// <summary>
        /// Gets the strong reference variant.
        /// </summary>
        /// <value>
        /// The strong reference variant.
        /// </value>
        /// <exception cref="DependencyContainerRegistrationException">strong reference.</exception>
        public virtual ObjectFactoryBase StrongReferenceVariant =>
            throw new DependencyContainerRegistrationException(GetType(), "strong reference");

        /// <summary>
        /// Gets the weak reference variant.
        /// </summary>
        /// <value>
        /// The weak reference variant.
        /// </value>
        /// <exception cref="DependencyContainerRegistrationException">weak reference.</exception>
        public virtual ObjectFactoryBase WeakReferenceVariant =>
            throw new DependencyContainerRegistrationException(GetType(), "weak reference");

        /// <summary>
        /// Create the type.
        /// </summary>
        /// <param name="requestedType">Type user requested to be resolved.</param>
        /// <param name="container">Container that requested the creation.</param>
        /// <param name="options">The options.</param>
        /// <returns> Instance of type. </returns>
        public abstract object GetObject(
            Type requestedType,
            DependencyContainer container,
            DependencyContainerResolveOptions options);

        /// <summary>
        /// Gets the factory for child container.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="child">The child.</param>
        /// <returns></returns>
        public virtual ObjectFactoryBase GetFactoryForChildContainer(
            Type type,
            DependencyContainer parent,
            DependencyContainer child)
        {
            return this;
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// IObjectFactory that creates new instances of types for each resolution.
    /// </summary>
    internal class MultiInstanceFactory : ObjectFactoryBase
    {
        private readonly Type _registerType;
        private readonly Type _registerImplementation;

        public MultiInstanceFactory(Type registerType, Type registerImplementation)
        {
            if (registerImplementation.IsAbstract || registerImplementation.IsInterface)
            {
                throw new DependencyContainerRegistrationException(registerImplementation,
                    "MultiInstanceFactory",
                    true);
            }

            if (!DependencyContainer.IsValidAssignment(registerType, registerImplementation))
            {
                throw new DependencyContainerRegistrationException(registerImplementation,
                    "MultiInstanceFactory",
                    true);
            }

            _registerType = registerType;
            _registerImplementation = registerImplementation;
        }

        public override Type CreatesType => _registerImplementation;

        public override ObjectFactoryBase SingletonVariant =>
            new SingletonFactory(_registerType, _registerImplementation);

        public override ObjectFactoryBase MultiInstanceVariant => this;

        public override object GetObject(
            Type requestedType,
            DependencyContainer container,
            DependencyContainerResolveOptions options)
        {
            try
            {
                return container.RegisteredTypes.ConstructType(_registerImplementation, Constructor, options);
            }
            catch (DependencyContainerResolutionException ex)
            {
                throw new DependencyContainerResolutionException(_registerType, ex);
            }
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// IObjectFactory that invokes a specified delegate to construct the object.
    /// </summary>
    internal class DelegateFactory : ObjectFactoryBase
    {
        private readonly Type _registerType;

        private readonly Func<DependencyContainer, Dictionary<string, object>, object> _factory;

        public DelegateFactory(
            Type registerType,
            Func<DependencyContainer, Dictionary<string, object>, object> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));

            _registerType = registerType;
        }

        public override bool AssumeConstruction => true;

        public override Type CreatesType => _registerType;

        public override ObjectFactoryBase WeakReferenceVariant => new WeakDelegateFactory(_registerType, _factory);

        public override ObjectFactoryBase StrongReferenceVariant => this;

        public override object GetObject(
            Type requestedType,
            DependencyContainer container,
            DependencyContainerResolveOptions options)
        {
            try
            {
                return _factory.Invoke(container, options.ConstructorParameters);
            }
            catch (Exception ex)
            {
                throw new DependencyContainerResolutionException(_registerType, ex);
            }
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// IObjectFactory that invokes a specified delegate to construct the object
    /// Holds the delegate using a weak reference.
    /// </summary>
    internal class WeakDelegateFactory : ObjectFactoryBase
    {
        private readonly Type _registerType;

        private readonly WeakReference _factory;

        public WeakDelegateFactory(
            Type registerType,
            Func<DependencyContainer, Dictionary<string, object>, object> factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            _factory = new WeakReference(factory);

            _registerType = registerType;
        }

        public override bool AssumeConstruction => true;

        public override Type CreatesType => _registerType;

        public override ObjectFactoryBase StrongReferenceVariant
        {
            get
            {
                if (!(_factory.Target is Func<DependencyContainer, Dictionary<string, object>, object> factory))
                    throw new DependencyContainerWeakReferenceException(_registerType);

                return new DelegateFactory(_registerType, factory);
            }
        }

        public override ObjectFactoryBase WeakReferenceVariant => this;

        public override object GetObject(
            Type requestedType,
            DependencyContainer container,
            DependencyContainerResolveOptions options)
        {
            if (!(_factory.Target is Func<DependencyContainer, Dictionary<string, object>, object> factory))
                throw new DependencyContainerWeakReferenceException(_registerType);

            try
            {
                return factory.Invoke(container, options.ConstructorParameters);
            }
            catch (Exception ex)
            {
                throw new DependencyContainerResolutionException(_registerType, ex);
            }
        }
    }

    /// <summary>
    /// Stores an particular instance to return for a type.
    /// </summary>
    internal class InstanceFactory : ObjectFactoryBase, IDisposable
    {
        private readonly Type _registerType;
        private readonly Type _registerImplementation;
        private readonly object _instance;

        public InstanceFactory(Type registerType, Type registerImplementation, object instance)
        {
            if (!DependencyContainer.IsValidAssignment(registerType, registerImplementation))
                throw new DependencyContainerRegistrationException(registerImplementation, "InstanceFactory", true);

            _registerType = registerType;
            _registerImplementation = registerImplementation;
            _instance = instance;
        }

        public override bool AssumeConstruction => true;

        public override Type CreatesType => _registerImplementation;

        public override ObjectFactoryBase MultiInstanceVariant =>
            new MultiInstanceFactory(_registerType, _registerImplementation);

        public override ObjectFactoryBase WeakReferenceVariant =>
            new WeakInstanceFactory(_registerType, _registerImplementation, _instance);

        public override ObjectFactoryBase StrongReferenceVariant => this;

        public override object GetObject(
            Type requestedType,
            DependencyContainer container,
            DependencyContainerResolveOptions options)
        {
            return _instance;
        }

        public void Dispose()
        {
            var disposable = _instance as IDisposable;

            disposable?.Dispose();
        }
    }

    /// <summary>
    /// Stores the instance with a weak reference.
    /// </summary>
    internal class WeakInstanceFactory : ObjectFactoryBase, IDisposable
    {
        private readonly Type _registerType;
        private readonly Type _registerImplementation;
        private readonly WeakReference _instance;

        public WeakInstanceFactory(Type registerType, Type registerImplementation, object instance)
        {
            if (!DependencyContainer.IsValidAssignment(registerType, registerImplementation))
            {
                throw new DependencyContainerRegistrationException(
                    registerImplementation,
                    "WeakInstanceFactory",
                    true);
            }

            _registerType = registerType;
            _registerImplementation = registerImplementation;
            _instance = new WeakReference(instance);
        }

        public override Type CreatesType => _registerImplementation;

        public override ObjectFactoryBase MultiInstanceVariant =>
            new MultiInstanceFactory(_registerType, _registerImplementation);

        public override ObjectFactoryBase WeakReferenceVariant => this;

        public override ObjectFactoryBase StrongReferenceVariant
        {
            get
            {
                var instance = _instance.Target;

                if (instance == null)
                    throw new DependencyContainerWeakReferenceException(_registerType);

                return new InstanceFactory(_registerType, _registerImplementation, instance);
            }
        }

        public override object GetObject(
            Type requestedType,
            DependencyContainer container,
            DependencyContainerResolveOptions options)
        {
            var instance = _instance.Target;

            if (instance == null)
                throw new DependencyContainerWeakReferenceException(_registerType);

            return instance;
        }

        public void Dispose() => (_instance.Target as IDisposable)?.Dispose();
    }

    /// <summary>
    /// A factory that lazy instantiates a type and always returns the same instance.
    /// </summary>
    internal class SingletonFactory : ObjectFactoryBase, IDisposable
    {
        private readonly Type _registerType;
        private readonly Type _registerImplementation;
        private readonly object _singletonLock = new object();
        private object _current;

        public SingletonFactory(Type registerType, Type registerImplementation)
        {
            if (registerImplementation.IsAbstract || registerImplementation.IsInterface)
            {
                throw new DependencyContainerRegistrationException(registerImplementation, nameof(SingletonFactory), true);
            }

            if (!DependencyContainer.IsValidAssignment(registerType, registerImplementation))
            {
                throw new DependencyContainerRegistrationException(registerImplementation, nameof(SingletonFactory), true);
            }

            _registerType = registerType;
            _registerImplementation = registerImplementation;
        }

        public override Type CreatesType => _registerImplementation;

        public override ObjectFactoryBase SingletonVariant => this;

        public override ObjectFactoryBase MultiInstanceVariant =>
            new MultiInstanceFactory(_registerType, _registerImplementation);

        public override object GetObject(
            Type requestedType,
            DependencyContainer container,
            DependencyContainerResolveOptions options)
        {
            if (options.ConstructorParameters.Count != 0)
                throw new ArgumentException("Cannot specify parameters for singleton types");

            lock (_singletonLock)
            {
                if (_current == null)
                    _current = container.RegisteredTypes.ConstructType(_registerImplementation, Constructor, options);
            }

            return _current;
        }

        public override ObjectFactoryBase GetFactoryForChildContainer(
            Type type,
            DependencyContainer parent,
            DependencyContainer child)
        {
            // We make sure that the singleton is constructed before the child container takes the factory.
            // Otherwise the results would vary depending on whether or not the parent container had resolved
            // the type before the child container does.
            GetObject(type, parent, DependencyContainerResolveOptions.Default);
            return this;
        }

        public void Dispose() => (_current as IDisposable)?.Dispose();
    }
}
