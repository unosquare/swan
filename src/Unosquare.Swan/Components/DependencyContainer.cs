// ===============================================================================
// TinyIoC
//
// An easy to use, hassle free, Inversion of Control Container for small projects
// and beginners alike.
//
// https://github.com/grumpydev/TinyIoC
// ===============================================================================
// Copyright © Steven Robbins.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
// ===============================================================================

#define USE_OBJECT_CONSTRUCTOR

namespace Unosquare.Swan.Components
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Exceptions;

    #region Enumerations

    /// <summary>
    /// Defines Resolution actions
    /// </summary>
    public enum DependencyContainerUnregisteredResolutionActions
    {
        /// <summary>
        /// Attempt to resolve type, even if the type isn't registered.
        /// 
        /// Registered types/options will always take precedence.
        /// </summary>
        AttemptResolve,

        /// <summary>
        /// Fail resolution if type not explicitly registered
        /// </summary>
        Fail,

        /// <summary>
        /// Attempt to resolve unregistered type if requested type is generic
        /// and no registration exists for the specific generic parameters used.
        /// 
        /// Registered types/options will always take precedence.
        /// </summary>
        GenericsOnly
    }

    /// <summary>
    /// Enumerates failure actions
    /// </summary>
    public enum DependencyContainerNamedResolutionFailureActions
    {
        /// <summary>
        /// The attempt unnamed resolution
        /// </summary>
        AttemptUnnamedResolution,
        
        /// <summary>
        /// The fail
        /// </summary>
        Fail
    }

    /// <summary>
    /// Enumerates duplicate definition actions
    /// </summary>
    public enum DependencyContainerDuplicateImplementationActions
    {
        /// <summary>
        /// The register single
        /// </summary>
        RegisterSingle,
        
        /// <summary>
        /// The register multiple
        /// </summary>
        RegisterMultiple,
        
        /// <summary>
        /// The fail
        /// </summary>
        Fail
    }

    #endregion

    #region Support Classes

    /// <summary>
    /// Define overload on named parameters
    /// </summary>
    public sealed class DependencyContainerNamedParameterOverloads : Dictionary<string, object>
    {
        /// <summary>
        /// Creates a new instance from a Dictionary
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>method returns a DependencyContainerNamedParameterOverloads object</returns>
        public static DependencyContainerNamedParameterOverloads FromIDictionary(IDictionary<string, object> data)
        {
            return data as DependencyContainerNamedParameterOverloads ?? new DependencyContainerNamedParameterOverloads(data);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerNamedParameterOverloads"/> class.
        /// </summary>
        public DependencyContainerNamedParameterOverloads()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerNamedParameterOverloads"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public DependencyContainerNamedParameterOverloads(IDictionary<string, object> data)
            : base(data)
        {
        }

        /// <summary>
        /// Gets the default instance.
        /// </summary>
        /// <value>
        /// The default.
        /// </value>
        public static DependencyContainerNamedParameterOverloads Default { get; } = new DependencyContainerNamedParameterOverloads();
    }

    /// <summary>
    /// Resolution settings
    /// </summary>
    public sealed class DependencyContainerResolveOptions
    {
        /// <summary>
        /// Gets or sets the unregistered resolution action.
        /// </summary>
        /// <value>
        /// The unregistered resolution action.
        /// </value>
        public DependencyContainerUnregisteredResolutionActions UnregisteredResolutionAction { get; set; } = DependencyContainerUnregisteredResolutionActions.AttemptResolve;

        /// <summary>
        /// Gets or sets the named resolution failure action.
        /// </summary>
        /// <value>
        /// The named resolution failure action.
        /// </value>
        public DependencyContainerNamedResolutionFailureActions NamedResolutionFailureAction { get; set; } = DependencyContainerNamedResolutionFailureActions.Fail;

        /// <summary>
        /// Gets the default options (attempt resolution of unregistered types, fail on named resolution if name not found)
        /// </summary>
        public static DependencyContainerResolveOptions Default { get; } = new DependencyContainerResolveOptions();

        /// <summary>
        /// Preconfigured option for attempting resolution of unregistered types and failing on named resolution if name not found
        /// </summary>
        public static DependencyContainerResolveOptions FailNameNotFoundOnly { get; } = new DependencyContainerResolveOptions() { NamedResolutionFailureAction = DependencyContainerNamedResolutionFailureActions.Fail, UnregisteredResolutionAction = DependencyContainerUnregisteredResolutionActions.AttemptResolve };

        /// <summary>
        /// Preconfigured option for failing on resolving unregistered types and on named resolution if name not found
        /// </summary>
        public static DependencyContainerResolveOptions FailUnregisteredAndNameNotFound { get; } = new DependencyContainerResolveOptions() { NamedResolutionFailureAction = DependencyContainerNamedResolutionFailureActions.Fail, UnregisteredResolutionAction = DependencyContainerUnregisteredResolutionActions.Fail };

        /// <summary>
        /// Preconfigured option for failing on resolving unregistered types, but attempting unnamed resolution if name not found
        /// </summary>
        public static DependencyContainerResolveOptions FailUnregisteredOnly { get; } = new DependencyContainerResolveOptions() { NamedResolutionFailureAction = DependencyContainerNamedResolutionFailureActions.AttemptUnnamedResolution, UnregisteredResolutionAction = DependencyContainerUnregisteredResolutionActions.Fail };
    }

    #endregion

    /// <summary>
    /// The concrete implementation of a simple IoC container
    /// based largely on TinyIoC
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public sealed class DependencyContainer : IDisposable
    {
        #region "Fluent" API
        
        /// <summary>
        /// Registration options for "fluent" API
        /// </summary>
        public sealed class RegisterOptions
        {
            private readonly DependencyContainer _container;
            private readonly TypeRegistration _registration;

            /// <summary>
            /// Initializes a new instance of the <see cref="RegisterOptions"/> class.
            /// </summary>
            /// <param name="container">The container.</param>
            /// <param name="registration">The registration.</param>
            public RegisterOptions(DependencyContainer container, TypeRegistration registration)
            {
                _container = container;
                _registration = registration;
            }

            /// <summary>
            /// Make registration a singleton (single instance) if possible
            /// </summary>
            /// <returns>RegisterOptions</returns>
            /// <exception cref="DependencyContainerRegistrationException">Generic constraint registration exception</exception>
            public RegisterOptions AsSingleton()
            {
                var currentFactory = _container.GetCurrentFactory(_registration);

                if (currentFactory == null)
                    throw new DependencyContainerRegistrationException(_registration.Type, "singleton");

                return _container.AddUpdateRegistration(_registration, currentFactory.SingletonVariant);
            }

            /// <summary>
            /// Make registration multi-instance if possible
            /// </summary>
            /// <returns>RegisterOptions</returns>
            /// <exception cref="DependencyContainerRegistrationException">Generic constraint registration exception</exception>
            public RegisterOptions AsMultiInstance()
            {
                var currentFactory = _container.GetCurrentFactory(_registration);

                if (currentFactory == null)
                    throw new DependencyContainerRegistrationException(_registration.Type, "multi-instance");

                return _container.AddUpdateRegistration(_registration, currentFactory.MultiInstanceVariant);
            }

            /// <summary>
            /// Make registration hold a weak reference if possible
            /// </summary>
            /// <returns>RegisterOptions</returns>
            /// <exception cref="DependencyContainerRegistrationException">Generic constraint registration exception</exception>
            public RegisterOptions WithWeakReference()
            {
                var currentFactory = _container.GetCurrentFactory(_registration);

                if (currentFactory == null)
                    throw new DependencyContainerRegistrationException(_registration.Type, "weak reference");

                return _container.AddUpdateRegistration(_registration, currentFactory.WeakReferenceVariant);
            }

            /// <summary>
            /// Make registration hold a strong reference if possible
            /// </summary>
            /// <returns>RegisterOptions</returns>
            /// <exception cref="DependencyContainerRegistrationException">Generic constraint registration exception</exception>
            public RegisterOptions WithStrongReference()
            {
                var currentFactory = _container.GetCurrentFactory(_registration);

                if (currentFactory == null)
                    throw new DependencyContainerRegistrationException(_registration.Type, "strong reference");

                return _container.AddUpdateRegistration(_registration, currentFactory.StrongReferenceVariant);
            }

            /// <summary>
            /// Sets the constructor to use
            /// </summary>
            /// <typeparam name="RegisterType">The type of the register type.</typeparam>
            /// <param name="constructor">The constructor.</param>
            /// <returns>method returns a RegisterOptions object</returns>
            /// <exception cref="DependencyContainerConstructorResolutionException">Constructor resolution exception</exception>
            public RegisterOptions UsingConstructor<RegisterType>(Expression<Func<RegisterType>> constructor)
            {
                var lambda = constructor as LambdaExpression;
                if (lambda == null)
                    throw new DependencyContainerConstructorResolutionException(typeof(RegisterType));

                var newExpression = lambda.Body as NewExpression;
                if (newExpression == null)
                    throw new DependencyContainerConstructorResolutionException(typeof(RegisterType));

                var constructorInfo = newExpression.Constructor;
                if (constructorInfo == null)
                    throw new DependencyContainerConstructorResolutionException(typeof(RegisterType));

                var currentFactory = _container.GetCurrentFactory(_registration);
                if (currentFactory == null)
                    throw new DependencyContainerConstructorResolutionException(typeof(RegisterType));

                currentFactory.SetConstructor(constructorInfo);

                return this;
            }

            /// <summary>
            /// Switches to a custom lifetime manager factory if possible.
            /// 
            /// Usually used for RegisterOptions "To*" extension methods such as the ASP.Net per-request one.
            /// </summary>
            /// <param name="instance">RegisterOptions instance</param>
            /// <param name="lifetimeProvider">Custom lifetime manager</param>
            /// <param name="errorString">Error string to display if switch fails</param>
            /// <returns>RegisterOptions</returns>
            public static RegisterOptions ToCustomLifetimeManager(RegisterOptions instance, ITinyIoCObjectLifetimeProvider lifetimeProvider, string errorString)
            {
                if (instance == null)
                    throw new ArgumentNullException(nameof(instance), "instance is null.");

                if (lifetimeProvider == null)
                    throw new ArgumentNullException(nameof(lifetimeProvider), "lifetimeProvider is null.");

                if (string.IsNullOrEmpty(errorString))
                    throw new ArgumentException("errorString is null or empty.", nameof(errorString));

                var currentFactory = instance._container.GetCurrentFactory(instance._registration);

                if (currentFactory == null)
                    throw new DependencyContainerRegistrationException(instance._registration.Type, errorString);

                return instance._container.AddUpdateRegistration(instance._registration, currentFactory.GetCustomObjectLifetimeVariant(lifetimeProvider, errorString));
            }
        }

        /// <summary>
        /// Registration options for "fluent" API when registering multiple implementations
        /// </summary>
        public sealed class MultiRegisterOptions
        {
            private IEnumerable<RegisterOptions> _registerOptions;

            /// <summary>
            /// Initializes a new instance of the <see cref="MultiRegisterOptions"/> class.
            /// </summary>
            /// <param name="registerOptions">The register options.</param>
            public MultiRegisterOptions(IEnumerable<RegisterOptions> registerOptions)
            {
                _registerOptions = registerOptions;
            }

            /// <summary>
            /// Make registration a singleton (single instance) if possible
            /// </summary>
            /// <returns>RegisterOptions</returns>
            /// <exception cref="DependencyContainerRegistrationException">Generic Constraint Registration Exception</exception>
            public MultiRegisterOptions AsSingleton()
            {
                _registerOptions = ExecuteOnAllRegisterOptions(ro => ro.AsSingleton());
                return this;
            }

            /// <summary>
            /// Make registration multi-instance if possible
            /// </summary>
            /// <returns>MultiRegisterOptions</returns>
            /// <exception cref="DependencyContainerRegistrationException">Generic Constraint Registration Exception</exception>
            public MultiRegisterOptions AsMultiInstance()
            {
                _registerOptions = ExecuteOnAllRegisterOptions(ro => ro.AsMultiInstance());
                return this;
            }

            /// <summary>
            /// Switches to a custom lifetime manager factory if possible.
            /// 
            /// Usually used for RegisterOptions "To*" extension methods such as the ASP.Net per-request one.
            /// </summary>
            /// <param name="instance">MultiRegisterOptions instance</param>
            /// <param name="lifetimeProvider">Custom lifetime manager</param>
            /// <param name="errorString">Error string to display if switch fails</param>
            /// <returns>MultiRegisterOptions</returns>
            public static MultiRegisterOptions ToCustomLifetimeManager(
                MultiRegisterOptions instance,
                ITinyIoCObjectLifetimeProvider lifetimeProvider,
                string errorString)
            {
                if (instance == null)
                    throw new ArgumentNullException(nameof(instance), "instance is null.");

                if (lifetimeProvider == null)
                    throw new ArgumentNullException(nameof(lifetimeProvider), "lifetimeProvider is null.");

                if (string.IsNullOrEmpty(errorString))
                    throw new ArgumentException("errorString is null or empty.", nameof(errorString));

                instance._registerOptions = instance.ExecuteOnAllRegisterOptions(ro => RegisterOptions.ToCustomLifetimeManager(ro, lifetimeProvider, errorString));

                return instance;
            }

            private IEnumerable<RegisterOptions> ExecuteOnAllRegisterOptions(Func<RegisterOptions, RegisterOptions> action)
            {
                return _registerOptions.Select(action).ToList();
            }
        }
        #endregion

        #region Public API

        #region Child Containers

        /// <summary>
        /// Gets the child container.
        /// </summary>
        /// <returns>method returns an DependencyContainer object</returns>
        public DependencyContainer GetChildContainer()
        {
            return new DependencyContainer(this);
        }

        #endregion

        #region Registration

#if !NETSTANDARD1_3 && !UWP
        /// <summary>
        /// Attempt to automatically register all non-generic classes and interfaces in the current app domain.
        /// Types will only be registered if they pass the supplied registration predicate.
        /// </summary>
        /// <param name="duplicateAction">What action to take when encountering duplicate implementations of an interface/base class.</param>
        /// <param name="registrationPredicate">Predicate to determine if a particular type should be registered</param>
        /// <exception cref="DependencyContainerAutoRegistrationException">Auto-registration Exception</exception> 
        public void AutoRegister(DependencyContainerDuplicateImplementationActions duplicateAction = DependencyContainerDuplicateImplementationActions.RegisterSingle, Func<Type, bool> registrationPredicate = null)
        {
            AutoRegisterInternal(Runtime.GetAssemblies().Where(a => !IsIgnoredAssembly(a)), duplicateAction, registrationPredicate);
        }
#endif

        /// <summary>
        /// Attempt to automatically register all non-generic classes and interfaces in the specified assemblies
        /// Types will only be registered if they pass the supplied registration predicate.
        /// </summary>
        /// <param name="assemblies">Assemblies to process</param>
        /// <param name="duplicateAction">What action to take when encountering duplicate implementations of an interface/base class.</param>
        /// <param name="registrationPredicate">Predicate to determine if a particular type should be registered</param>
        /// <exception cref="DependencyContainerAutoRegistrationException">Auto-registration Exception</exception> 
        public void AutoRegister(IEnumerable<Assembly> assemblies, DependencyContainerDuplicateImplementationActions duplicateAction = DependencyContainerDuplicateImplementationActions.RegisterSingle, Func<Type, bool> registrationPredicate = null)
        {
            AutoRegisterInternal(assemblies, duplicateAction, registrationPredicate);
        }

        /// <summary>
        /// Creates/replaces a named container class registration with default options.
        /// </summary>
        /// <param name="registerType">Type to register</param>
        /// <param name="name">Name of registration</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register(Type registerType, string name = "")
        {
            return RegisterInternal(registerType, name, GetDefaultObjectFactory(registerType, registerType));
        }

        /// <summary>
        /// Creates/replaces a named container class registration with a given implementation and default options.
        /// </summary>
        /// <param name="registerType">Type to register</param>
        /// <param name="registerImplementation">Type to instantiate that implements RegisterType</param>
        /// <param name="name">Name of registration</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register(Type registerType, Type registerImplementation, string name = "")
        {
            return RegisterInternal(registerType, name, GetDefaultObjectFactory(registerType, registerImplementation));
        }

        /// <summary>
        /// Creates/replaces a named container class registration with a specific, strong referenced, instance.
        /// </summary>
        /// <param name="registerType">Type to register</param>
        /// <param name="instance">Instance of RegisterType to register</param>
        /// <param name="name">Name of registration</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register(Type registerType, object instance, string name = "")
        {
            return RegisterInternal(registerType, name, new InstanceFactory(registerType, registerType, instance));
        }

        /// <summary>
        /// Creates/replaces a named container class registration with a specific, strong referenced, instance.
        /// </summary>
        /// <param name="registerType">Type to register</param>
        /// <param name="registerImplementation">Type of instance to register that implements RegisterType</param>
        /// <param name="instance">Instance of RegisterImplementation to register</param>
        /// <param name="name">Name of registration</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register(Type registerType, Type registerImplementation, object instance, string name = "")
        {
            return RegisterInternal(registerType, name, new InstanceFactory(registerType, registerImplementation, instance));
        }

        /// <summary>
        /// Creates/replaces a container class registration with a user specified factory
        /// </summary>
        /// <param name="registerType">Type to register</param>
        /// <param name="factory">Factory/lambda that returns an instance of RegisterType</param>
        /// <param name="name">Name of registation</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register(Type registerType, Func<DependencyContainer, DependencyContainerNamedParameterOverloads, object> factory, string name = "")
        {
            return RegisterInternal(registerType, name, new DelegateFactory(registerType, factory));
        }

        /// <summary>
        /// Creates/replaces a named container class registration with default options.
        /// </summary>
        /// <typeparam name="RegisterType">Type to register</typeparam>
        /// <param name="name">Name of registration</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register<RegisterType>(string name = "")
            where RegisterType : class
        {
            return Register(typeof(RegisterType), name);
        }

        /// <summary>
        /// Creates/replaces a named container class registration with a given implementation and default options.
        /// </summary>
        /// <typeparam name="RegisterType">Type to register</typeparam>
        /// <typeparam name="RegisterImplementation">Type to instantiate that implements RegisterType</typeparam>
        /// <param name="name">Name of registration</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register<RegisterType, RegisterImplementation>(string name = "")
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            return Register(typeof(RegisterType), typeof(RegisterImplementation), name);
        }

        /// <summary>
        /// Creates/replaces a named container class registration with a specific, strong referenced, instance.
        /// </summary>
        /// <typeparam name="RegisterType">Type to register</typeparam>
        /// <param name="instance">Instance of RegisterType to register</param>
        /// <param name="name">Name of registration</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register<RegisterType>(RegisterType instance, string name = "")
            where RegisterType : class
        {
            return Register(typeof(RegisterType), instance, name);
        }

        /// <summary>
        /// Creates/replaces a named container class registration with a specific, strong referenced, instance.
        /// </summary>
        /// <typeparam name="RegisterType">Type to register</typeparam>
        /// <typeparam name="RegisterImplementation">Type of instance to register that implements RegisterType</typeparam>
        /// <param name="instance">Instance of RegisterImplementation to register</param>
        /// <param name="name">Name of registration</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register<RegisterType, RegisterImplementation>(RegisterImplementation instance, string name = "")
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            return Register(typeof(RegisterType), typeof(RegisterImplementation), instance, name);
        }

        /// <summary>
        /// Creates/replaces a named container class registration with a user specified factory
        /// </summary>
        /// <typeparam name="RegisterType">Type to register</typeparam>
        /// <param name="factory">Factory/lambda that returns an instance of RegisterType</param>
        /// <param name="name">Name of registation</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register<RegisterType>(Func<DependencyContainer, DependencyContainerNamedParameterOverloads, RegisterType> factory, string name = "")
            where RegisterType : class
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            return Register(typeof(RegisterType), factory, name);
        }

        /// <summary>
        /// Register multiple implementations of a type.
        /// 
        /// Internally this registers each implementation using the full name of the class as its registration name.
        /// </summary>
        /// <typeparam name="RegisterType">Type that each implementation implements</typeparam>
        /// <param name="implementationTypes">Types that implement RegisterType</param>
        /// <returns>MultiRegisterOptions for the fluent API</returns>
        public MultiRegisterOptions RegisterMultiple<RegisterType>(IEnumerable<Type> implementationTypes)
        {
            return RegisterMultiple(typeof(RegisterType), implementationTypes);
        }

        /// <summary>
        /// Register multiple implementations of a type.
        /// 
        /// Internally this registers each implementation using the full name of the class as its registration name.
        /// </summary>
        /// <param name="registrationType">Type that each implementation implements</param>
        /// <param name="implementationTypes">Types that implement RegisterType</param>
        /// <returns>MultiRegisterOptions for the fluent API</returns>
        public MultiRegisterOptions RegisterMultiple(Type registrationType, IEnumerable<Type> implementationTypes)
        {
            if (implementationTypes == null)
                throw new ArgumentNullException(nameof(implementationTypes), "types is null.");

            foreach (var type in implementationTypes)
            {
                if (!registrationType.IsAssignableFrom(type))
                    throw new ArgumentException($"types: The type {registrationType.FullName} is not assignable from {type.FullName}");
            }

            if (implementationTypes.Count() != implementationTypes.Distinct().Count())
            {
                var queryForDuplicatedTypes = from i in implementationTypes
                                              group i by i
                                                  into j
                                              where j.Count() > 1
                                              select j.Key.FullName;

                var fullNamesOfDuplicatedTypes = string.Join(",\n", queryForDuplicatedTypes.ToArray());

                throw new ArgumentException($"types: The same implementation type cannot be specified multiple times for {registrationType.FullName}\n\n{fullNamesOfDuplicatedTypes}");
            }

            var registerOptions = implementationTypes.Select(type => Register(registrationType, type, type.FullName)).ToList();

            return new MultiRegisterOptions(registerOptions);
        }
        #endregion

        #region Unregistration

        /// <summary>
        /// Remove a named container class registration.
        /// </summary>
        /// <typeparam name="RegisterType">Type to unregister</typeparam>
        /// <param name="name">Name of registration</param>
        /// <returns>true if the registration is successfully found and removed; otherwise, false.</returns>
        public bool Unregister<RegisterType>(string name = "")
        {
            return Unregister(typeof(RegisterType), name);
        }

        /// <summary>
        /// Remove a named container class registration.
        /// </summary>
        /// <param name="registerType">Type to unregister</param>
        /// <param name="name">Name of registration</param>
        /// <returns>true if the registration is successfully found and removed; otherwise, false.</returns>
        public bool Unregister(Type registerType, string name = "")
        {
            var typeRegistration = new TypeRegistration(registerType, name);

            return RemoveRegistration(typeRegistration);
        }

        #endregion

        #region Resolution

        /// <summary>
        /// Attempts to resolve a type using specified options.
        /// </summary>
        /// <param name="resolveType">Type to resolve</param>
        /// <param name="options">Resolution options</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="DependencyContainerResolutionException">Unable to resolve the type.</exception>
        public object Resolve(Type resolveType, DependencyContainerResolveOptions options = null)
        {
            return ResolveInternal(new TypeRegistration(resolveType), DependencyContainerNamedParameterOverloads.Default, options ?? DependencyContainerResolveOptions.Default);
        }

        /// <summary>
        /// Attempts to resolve a type using specified options and the supplied constructor parameters.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// </summary>
        /// <param name="resolveType">Type to resolve</param>
        /// <param name="parameters">User specified constructor parameters</param>
        /// <param name="options">Resolution options</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="DependencyContainerResolutionException">Unable to resolve the type.</exception>
        public object Resolve(Type resolveType, DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options = null)
        {
            return ResolveInternal(new TypeRegistration(resolveType), parameters, options ?? DependencyContainerResolveOptions.Default);
        }

        /// <summary>
        /// Attempts to resolve a named type using specified options and the supplied constructor parameters.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// </summary>
        /// <param name="resolveType">Type to resolve</param>
        /// <param name="name">Name of registration</param>
        /// <param name="parameters">User specified constructor parameters</param>
        /// <param name="options">Resolution options</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="DependencyContainerResolutionException">Unable to resolve the type.</exception>
        public object Resolve(Type resolveType, string name, DependencyContainerNamedParameterOverloads parameters = null, DependencyContainerResolveOptions options = null)
        {
            return ResolveInternal(new TypeRegistration(resolveType, name), parameters ?? DependencyContainerNamedParameterOverloads.Default, options ?? DependencyContainerResolveOptions.Default);
        }

        /// <summary>
        /// Attempts to resolve a type using specified options.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="options">Resolution options</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="DependencyContainerResolutionException">Unable to resolve the type.</exception>
        public ResolveType Resolve<ResolveType>(DependencyContainerResolveOptions options = null)
            where ResolveType : class
        {
            return (ResolveType)Resolve(typeof(ResolveType), options);
        }

        /// <summary>
        /// Attempts to resolve a type using supplied options and  name.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="name">Name of registration</param>
        /// <param name="options">Resolution options</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="DependencyContainerResolutionException">Unable to resolve the type.</exception>
        public ResolveType Resolve<ResolveType>(string name, DependencyContainerResolveOptions options = null)
            where ResolveType : class
        {
            return (ResolveType)Resolve(typeof(ResolveType), name, null, options);
        }

        /// <summary>
        /// Attempts to resolve a type using specified options and the supplied constructor parameters.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="parameters">User specified constructor parameters</param>
        /// <param name="options">Resolution options</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="DependencyContainerResolutionException">Unable to resolve the type.</exception>
        public ResolveType Resolve<ResolveType>(DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options = null)
            where ResolveType : class
        {
            return (ResolveType)Resolve(typeof(ResolveType), parameters, options);
        }

        /// <summary>
        /// Attempts to resolve a named type using specified options and the supplied constructor parameters.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="name">Name of registration</param>
        /// <param name="parameters">User specified constructor parameters</param>
        /// <param name="options">Resolution options</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="DependencyContainerResolutionException">Unable to resolve the type.</exception>
        public ResolveType Resolve<ResolveType>(string name, DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options = null)
            where ResolveType : class
        {
            return (ResolveType)Resolve(typeof(ResolveType), name, parameters, options);
        }

        /// <summary>
        /// Attempts to predict whether a given type can be resolved with the specified options.
        ///
        /// Note: Resolution may still fail if user defined factory registrations fail to construct objects when called.
        /// </summary>
        /// <param name="resolveType">Type to resolve</param>
        /// <param name="options">Resolution options</param>
        /// <returns>Bool indicating whether the type can be resolved</returns>
        public bool CanResolve(Type resolveType, DependencyContainerResolveOptions options)
        {
            return CanResolveInternal(new TypeRegistration(resolveType), DependencyContainerNamedParameterOverloads.Default, options);
        }

        /// <summary>
        /// Attempts to predict whether a given type can be resolved with the supplied constructor parameters options.
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// Note: Resolution may still fail if user defined factory registrations fail to construct objects when called.
        /// </summary>
        /// <param name="resolveType">Type to resolve</param>
        /// <param name="parameters">User supplied named parameter overloads</param>
        /// <param name="name">The name.</param>
        /// <param name="options">Resolution options</param>
        /// <returns>
        /// Bool indicating whether the type can be resolved
        /// </returns>
        public bool CanResolve(Type resolveType, DependencyContainerNamedParameterOverloads parameters = null, string name = null, DependencyContainerResolveOptions options = null)
        {
            return CanResolveInternal(new TypeRegistration(resolveType, name), parameters ?? DependencyContainerNamedParameterOverloads.Default, options ?? DependencyContainerResolveOptions.Default);
        }

        /// <summary>
        /// Attempts to predict whether a given type can be resolved with the specified options.
        ///
        /// Note: Resolution may still fail if user defined factory registrations fail to construct objects when called.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="options">Resolution options</param>
        /// <returns>Bool indicating whether the type can be resolved</returns>
        public bool CanResolve<ResolveType>(DependencyContainerResolveOptions options)
            where ResolveType : class
        {
            return CanResolve(typeof(ResolveType), options);
        }

        /// <summary>
        /// Attempts to predict whether a given named type can be resolved with the specified options.
        ///
        /// Note: Resolution may still fail if user defined factory registrations fail to construct objects when called.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="name">Name of registration</param>
        /// <param name="options">Resolution options</param>
        /// <returns>Bool indicating whether the type can be resolved</returns>
        public bool CanResolve<ResolveType>(string name = null, DependencyContainerResolveOptions options = null)
            where ResolveType : class
        {
            return CanResolve(typeof(ResolveType), null, name, options);
        }

        /// <summary>
        /// Attempts to predict whether a given type can be resolved with the supplied constructor parameters options.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// 
        /// Note: Resolution may still fail if user defined factory registrations fail to construct objects when called.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="parameters">User supplied named parameter overloads</param>
        /// <param name="options">Resolution options</param>
        /// <returns>Bool indicating whether the type can be resolved</returns>
        public bool CanResolve<ResolveType>(DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options = null)
            where ResolveType : class
        {
            return CanResolve(typeof(ResolveType), parameters, string.Empty, options);
        }

        /// <summary>
        /// Attempts to predict whether a given named type can be resolved with the supplied constructor parameters options.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// 
        /// Note: Resolution may still fail if user defined factory registrations fail to construct objects when called.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="name">Name of registration</param>
        /// <param name="parameters">User supplied named parameter overloads</param>
        /// <param name="options">Resolution options</param>
        /// <returns>Bool indicating whether the type can be resolved</returns>
        public bool CanResolve<ResolveType>(string name, DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options = null)
            where ResolveType : class
        {
            return CanResolve(typeof(ResolveType), parameters, name, options);
        }

        /// <summary>
        /// Attempts to resolve a type using the default options
        /// </summary>
        /// <param name="resolveType">Type to resolve</param>
        /// <param name="resolvedType">Resolved type or default if resolve fails</param>
        /// <returns>True if resolved successfully, false otherwise</returns>
        public bool TryResolve(Type resolveType, out object resolvedType)
        {
            try
            {
                resolvedType = Resolve(resolveType);
                return true;
            }
            catch (DependencyContainerResolutionException)
            {
                resolvedType = null;
                return false;
            }
        }

        /// <summary>
        /// Attempts to resolve a type using the given options
        /// </summary>
        /// <param name="resolveType">Type to resolve</param>
        /// <param name="options">Resolution options</param>
        /// <param name="resolvedType">Resolved type or default if resolve fails</param>
        /// <returns>True if resolved successfully, false otherwise</returns>
        public bool TryResolve(Type resolveType, DependencyContainerResolveOptions options, out object resolvedType)
        {
            try
            {
                resolvedType = Resolve(resolveType, options);
                return true;
            }
            catch (DependencyContainerResolutionException)
            {
                resolvedType = null;
                return false;
            }
        }

        /// <summary>
        /// Attempts to resolve a type using the default options and given name
        /// </summary>
        /// <param name="resolveType">Type to resolve</param>
        /// <param name="name">Name of registration</param>
        /// <param name="resolvedType">Resolved type or default if resolve fails</param>
        /// <returns>True if resolved successfully, false otherwise</returns>
        public bool TryResolve(Type resolveType, string name, out object resolvedType)
        {
            try
            {
                resolvedType = Resolve(resolveType, name);
                return true;
            }
            catch (DependencyContainerResolutionException)
            {
                resolvedType = null;
                return false;
            }
        }

        /// <summary>
        /// Attempts to resolve a type using the given options and name
        /// </summary>
        /// <param name="resolveType">Type to resolve</param>
        /// <param name="name">Name of registration</param>
        /// <param name="options">Resolution options</param>
        /// <param name="resolvedType">Resolved type or default if resolve fails</param>
        /// <returns>True if resolved successfully, false otherwise</returns>
        public bool TryResolve(Type resolveType, string name, DependencyContainerResolveOptions options, out object resolvedType)
        {
            try
            {
                resolvedType = Resolve(resolveType, name, null, options);
                return true;
            }
            catch (DependencyContainerResolutionException)
            {
                resolvedType = null;
                return false;
            }
        }

        /// <summary>
        /// Attempts to resolve a type using the default options and supplied constructor parameters
        /// </summary>
        /// <param name="resolveType">Type to resolve</param>
        /// <param name="parameters">User specified constructor parameters</param>
        /// <param name="resolvedType">Resolved type or default if resolve fails</param>
        /// <returns>True if resolved successfully, false otherwise</returns>
        public bool TryResolve(Type resolveType, DependencyContainerNamedParameterOverloads parameters, out object resolvedType)
        {
            try
            {
                resolvedType = Resolve(resolveType, parameters);
                return true;
            }
            catch (DependencyContainerResolutionException)
            {
                resolvedType = null;
                return false;
            }
        }

        /// <summary>
        /// Attempts to resolve a type using the default options and supplied name and constructor parameters
        /// </summary>
        /// <param name="resolveType">Type to resolve</param>
        /// <param name="name">Name of registration</param>
        /// <param name="parameters">User specified constructor parameters</param>
        /// <param name="resolvedType">Resolved type or default if resolve fails</param>
        /// <returns>True if resolved successfully, false otherwise</returns>
        public bool TryResolve(Type resolveType, string name, DependencyContainerNamedParameterOverloads parameters, out object resolvedType)
        {
            try
            {
                resolvedType = Resolve(resolveType, name, parameters);
                return true;
            }
            catch (DependencyContainerResolutionException)
            {
                resolvedType = null;
                return false;
            }
        }

        /// <summary>
        /// Attempts to resolve a type using the supplied options and constructor parameters
        /// </summary>
        /// <param name="resolveType">Type to resolve</param>
        /// <param name="parameters">User specified constructor parameters</param>
        /// <param name="options">Resolution options</param>
        /// <param name="resolvedType">Resolved type or default if resolve fails</param>
        /// <returns>True if resolved successfully, false otherwise</returns>
        public bool TryResolve(Type resolveType, DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options, out object resolvedType)
        {
            try
            {
                resolvedType = Resolve(resolveType, parameters, options);
                return true;
            }
            catch (DependencyContainerResolutionException)
            {
                resolvedType = null;
                return false;
            }
        }

        /// <summary>
        /// Attempts to resolve a type using the supplied name, options and constructor parameters
        /// </summary>
        /// <param name="resolveType">Type to resolve</param>
        /// <param name="name">Name of registration</param>
        /// <param name="parameters">User specified constructor parameters</param>
        /// <param name="options">Resolution options</param>
        /// <param name="resolvedType">Resolved type or default if resolve fails</param>
        /// <returns>True if resolved successfully, false otherwise</returns>
        public bool TryResolve(Type resolveType, string name, DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options, out object resolvedType)
        {
            try
            {
                resolvedType = Resolve(resolveType, name, parameters, options);
                return true;
            }
            catch (DependencyContainerResolutionException)
            {
                resolvedType = null;
                return false;
            }
        }

        /// <summary>
        /// Attempts to resolve a type using the default options
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="resolvedType">Resolved type or default if resolve fails</param>
        /// <returns>True if resolved successfully, false otherwise</returns>
        public bool TryResolve<ResolveType>(out ResolveType resolvedType)
            where ResolveType : class
        {
            try
            {
                resolvedType = Resolve<ResolveType>();
                return true;
            }
            catch (DependencyContainerResolutionException)
            {
                resolvedType = default(ResolveType);
                return false;
            }
        }

        /// <summary>
        /// Attempts to resolve a type using the given options
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="options">Resolution options</param>
        /// <param name="resolvedType">Resolved type or default if resolve fails</param>
        /// <returns>True if resolved successfully, false otherwise</returns>
        public bool TryResolve<ResolveType>(DependencyContainerResolveOptions options, out ResolveType resolvedType)
            where ResolveType : class
        {
            try
            {
                resolvedType = Resolve<ResolveType>(options);
                return true;
            }
            catch (DependencyContainerResolutionException)
            {
                resolvedType = default(ResolveType);
                return false;
            }
        }

        /// <summary>
        /// Attempts to resolve a type using the default options and given name
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="name">Name of registration</param>
        /// <param name="resolvedType">Resolved type or default if resolve fails</param>
        /// <returns>True if resolved successfully, false otherwise</returns>
        public bool TryResolve<ResolveType>(string name, out ResolveType resolvedType)
            where ResolveType : class
        {
            try
            {
                resolvedType = Resolve<ResolveType>(name);
                return true;
            }
            catch (DependencyContainerResolutionException)
            {
                resolvedType = default(ResolveType);
                return false;
            }
        }

        /// <summary>
        /// Attempts to resolve a type using the given options and name
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="name">Name of registration</param>
        /// <param name="options">Resolution options</param>
        /// <param name="resolvedType">Resolved type or default if resolve fails</param>
        /// <returns>True if resolved successfully, false otherwise</returns>
        public bool TryResolve<ResolveType>(string name, DependencyContainerResolveOptions options, out ResolveType resolvedType)
            where ResolveType : class
        {
            try
            {
                resolvedType = Resolve<ResolveType>(name, options);
                return true;
            }
            catch (DependencyContainerResolutionException)
            {
                resolvedType = default(ResolveType);
                return false;
            }
        }

        /// <summary>
        /// Attempts to resolve a type using the default options and supplied constructor parameters
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="parameters">User specified constructor parameters</param>
        /// <param name="resolvedType">Resolved type or default if resolve fails</param>
        /// <returns>True if resolved successfully, false otherwise</returns>
        public bool TryResolve<ResolveType>(DependencyContainerNamedParameterOverloads parameters, out ResolveType resolvedType)
            where ResolveType : class
        {
            try
            {
                resolvedType = Resolve<ResolveType>(parameters);
                return true;
            }
            catch (DependencyContainerResolutionException)
            {
                resolvedType = default(ResolveType);
                return false;
            }
        }

        /// <summary>
        /// Attempts to resolve a type using the default options and supplied name and constructor parameters
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="name">Name of registration</param>
        /// <param name="parameters">User specified constructor parameters</param>
        /// <param name="resolvedType">Resolved type or default if resolve fails</param>
        /// <returns>True if resolved successfully, false otherwise</returns>
        public bool TryResolve<ResolveType>(string name, DependencyContainerNamedParameterOverloads parameters, out ResolveType resolvedType)
            where ResolveType : class
        {
            try
            {
                resolvedType = Resolve<ResolveType>(name, parameters);
                return true;
            }
            catch (DependencyContainerResolutionException)
            {
                resolvedType = default(ResolveType);
                return false;
            }
        }

        /// <summary>
        /// Attempts to resolve a type using the supplied options and constructor parameters
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="parameters">User specified constructor parameters</param>
        /// <param name="options">Resolution options</param>
        /// <param name="resolvedType">Resolved type or default if resolve fails</param>
        /// <returns>True if resolved successfully, false otherwise</returns>
        public bool TryResolve<ResolveType>(DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options, out ResolveType resolvedType)
            where ResolveType : class
        {
            try
            {
                resolvedType = Resolve<ResolveType>(parameters, options);
                return true;
            }
            catch (DependencyContainerResolutionException)
            {
                resolvedType = default(ResolveType);
                return false;
            }
        }

        /// <summary>
        /// Attempts to resolve a type using the supplied name, options and constructor parameters
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="name">Name of registration</param>
        /// <param name="parameters">User specified constructor parameters</param>
        /// <param name="options">Resolution options</param>
        /// <param name="resolvedType">Resolved type or default if resolve fails</param>
        /// <returns>True if resolved successfully, false otherwise</returns>
        public bool TryResolve<ResolveType>(string name, DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options, out ResolveType resolvedType)
            where ResolveType : class
        {
            try
            {
                resolvedType = Resolve<ResolveType>(name, parameters, options);
                return true;
            }
            catch (DependencyContainerResolutionException)
            {
                resolvedType = default(ResolveType);
                return false;
            }
        }

        /// <summary>
        /// Returns all registrations of a type
        /// </summary>
        /// <param name="resolveType">Type to resolveAll</param>
        /// <param name="includeUnnamed">Whether to include un-named (default) registrations</param>
        /// <returns>IEnumerable</returns>
        public IEnumerable<object> ResolveAll(Type resolveType, bool includeUnnamed = false)
        {
            return ResolveAllInternal(resolveType, includeUnnamed);
        }

        /// <summary>
        /// Returns all registrations of a type
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolveAll</typeparam>
        /// <param name="includeUnnamed">Whether to include un-named (default) registrations</param>
        /// <returns>IEnumerable</returns>
        public IEnumerable<ResolveType> ResolveAll<ResolveType>(bool includeUnnamed = true)
            where ResolveType : class
        {
            return ResolveAll(typeof(ResolveType), includeUnnamed).Cast<ResolveType>();
        }

        /// <summary>
        /// Attempts to resolve all public property dependencies on the given object using the given resolve options.
        /// </summary>
        /// <param name="input">Object to "build up"</param>
        /// <param name="resolveOptions">Resolve options to use</param>
        public void BuildUp(object input, DependencyContainerResolveOptions resolveOptions = null)
        {
            BuildUpInternal(input, resolveOptions ?? DependencyContainerResolveOptions.Default);
        }
        #endregion
        #endregion

        #region Object Factories
        
        /// <summary>
        /// Provides custom lifetime management for ASP.Net per-request lifetimes etc.
        /// </summary>
        public interface ITinyIoCObjectLifetimeProvider
        {
            /// <summary>
            /// Gets the stored object if it exists, or null if not
            /// </summary>
            /// <returns>Object instance or null</returns>
            object GetObject();

            /// <summary>
            /// Store the object
            /// </summary>
            /// <param name="value">Object to store</param>
            void SetObject(object value);

            /// <summary>
            /// Release the object
            /// </summary>
            void ReleaseObject();
        }

        private abstract class ObjectFactoryBase
        {
            /// <summary>
            /// Whether to assume this factory successfully constructs its objects
            /// 
            /// Generally set to true for delegate style factories as CanResolve cannot delve
            /// into the delegates they contain.
            /// </summary>
            public virtual bool AssumeConstruction => false;

            /// <summary>
            /// The type the factory instantiates
            /// </summary>
            public abstract Type CreatesType { get; }

            /// <summary>
            /// Constructor to use, if specified
            /// </summary>
            public ConstructorInfo Constructor { get; private set; }

            /// <summary>
            /// Create the type
            /// </summary>
            /// <param name="requestedType">Type user requested to be resolved</param>
            /// <param name="container">Container that requested the creation</param>
            /// <param name="parameters">Any user parameters passed</param>
            /// <param name="options"></param>
            /// <returns>The method returns an object</returns>
            public abstract object GetObject(Type requestedType, DependencyContainer container, DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options);

            public virtual ObjectFactoryBase SingletonVariant
            {
                get
                {
                    throw new DependencyContainerRegistrationException(GetType(), "singleton");
                }
            }

            public virtual ObjectFactoryBase MultiInstanceVariant
            {
                get
                {
                    throw new DependencyContainerRegistrationException(GetType(), "multi-instance");
                }
            }

            public virtual ObjectFactoryBase StrongReferenceVariant
            {
                get
                {
                    throw new DependencyContainerRegistrationException(GetType(), "strong reference");
                }
            }

            public virtual ObjectFactoryBase WeakReferenceVariant
            {
                get
                {
                    throw new DependencyContainerRegistrationException(GetType(), "weak reference");
                }
            }

            public virtual ObjectFactoryBase GetCustomObjectLifetimeVariant(ITinyIoCObjectLifetimeProvider lifetimeProvider, string errorString)
            {
                throw new DependencyContainerRegistrationException(GetType(), errorString);
            }

            public virtual void SetConstructor(ConstructorInfo constructor)
            {
                Constructor = constructor;
            }

            public virtual ObjectFactoryBase GetFactoryForChildContainer(Type type, DependencyContainer parent, DependencyContainer child)
            {
                return this;
            }
        }

        /// <summary>
        /// IObjectFactory that creates new instances of types for each resolution
        /// </summary>
        private class MultiInstanceFactory : ObjectFactoryBase
        {
            private readonly Type registerType;
            private readonly Type registerImplementation;
            public override Type CreatesType => registerImplementation;

            public MultiInstanceFactory(Type registerType, Type registerImplementation)
            {
                if (registerImplementation.IsAbstract() || registerImplementation.IsInterface())
                    throw new DependencyContainerRegistrationTypeException(registerImplementation, "MultiInstanceFactory");

                if (!IsValidAssignment(registerType, registerImplementation))
                    throw new DependencyContainerRegistrationTypeException(registerImplementation, "MultiInstanceFactory");

                this.registerType = registerType;
                this.registerImplementation = registerImplementation;
            }

            public override object GetObject(Type requestedType, DependencyContainer container, DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options)
            {
                try
                {
                    return container.ConstructType(registerImplementation, Constructor, parameters, options);
                }
                catch (DependencyContainerResolutionException ex)
                {
                    throw new DependencyContainerResolutionException(registerType, ex);
                }
            }

            public override ObjectFactoryBase SingletonVariant => new SingletonFactory(registerType, registerImplementation);

            public override ObjectFactoryBase GetCustomObjectLifetimeVariant(ITinyIoCObjectLifetimeProvider lifetimeProvider, string errorString)
            {
                return new CustomObjectLifetimeFactory(registerType, registerImplementation, lifetimeProvider, errorString);
            }

            public override ObjectFactoryBase MultiInstanceVariant => this;
        }

        /// <summary>
        /// IObjectFactory that invokes a specified delegate to construct the object
        /// </summary>
        private class DelegateFactory : ObjectFactoryBase
        {
            private readonly Type registerType;

            private readonly Func<DependencyContainer, DependencyContainerNamedParameterOverloads, object> _factory;

            public override bool AssumeConstruction => true;

            public override Type CreatesType => registerType;

            public override object GetObject(Type requestedType, DependencyContainer container, DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options)
            {
                try
                {
                    return _factory.Invoke(container, parameters);
                }
                catch (Exception ex)
                {
                    throw new DependencyContainerResolutionException(registerType, ex);
                }
            }

            public DelegateFactory(Type registerType, Func<DependencyContainer, DependencyContainerNamedParameterOverloads, object> factory)
            {
                if (factory == null)
                    throw new ArgumentNullException(nameof(factory));

                _factory = factory;

                this.registerType = registerType;
            }

            public override ObjectFactoryBase WeakReferenceVariant => new WeakDelegateFactory(registerType, _factory);

            public override ObjectFactoryBase StrongReferenceVariant => this;

            public override void SetConstructor(ConstructorInfo constructor)
            {
                throw new DependencyContainerConstructorResolutionException("Constructor selection is not possible for delegate factory registrations");
            }
        }

        /// <summary>
        /// IObjectFactory that invokes a specified delegate to construct the object
        /// Holds the delegate using a weak reference
        /// </summary>
        private class WeakDelegateFactory : ObjectFactoryBase
        {
            private readonly Type registerType;

            private readonly WeakReference _factory;

            public override bool AssumeConstruction => true;

            public override Type CreatesType => registerType;

            public override object GetObject(Type requestedType, DependencyContainer container, DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options)
            {
                var factory = _factory.Target as Func<DependencyContainer, DependencyContainerNamedParameterOverloads, object>;

                if (factory == null)
                    throw new DependencyContainerWeakReferenceException(registerType);

                try
                {
                    return factory.Invoke(container, parameters);
                }
                catch (Exception ex)
                {
                    throw new DependencyContainerResolutionException(registerType, ex);
                }
            }

            public WeakDelegateFactory(Type registerType, Func<DependencyContainer, DependencyContainerNamedParameterOverloads, object> factory)
            {
                if (factory == null)
                    throw new ArgumentNullException(nameof(factory));

                _factory = new WeakReference(factory);

                this.registerType = registerType;
            }

            public override ObjectFactoryBase StrongReferenceVariant
            {
                get
                {
                    var factory = _factory.Target as Func<DependencyContainer, DependencyContainerNamedParameterOverloads, object>;

                    if (factory == null)
                        throw new DependencyContainerWeakReferenceException(registerType);

                    return new DelegateFactory(registerType, factory);
                }
            }

            public override ObjectFactoryBase WeakReferenceVariant => this;

            public override void SetConstructor(ConstructorInfo constructor)
            {
                throw new DependencyContainerConstructorResolutionException("Constructor selection is not possible for delegate factory registrations");
            }
        }

        /// <summary>
        /// Stores an particular instance to return for a type
        /// </summary>
        private class InstanceFactory : ObjectFactoryBase, IDisposable
        {
            private readonly Type registerType;
            private readonly Type registerImplementation;
            private readonly object _instance;

            public override bool AssumeConstruction => true;

            public InstanceFactory(Type registerType, Type registerImplementation, object instance)
            {
                if (!IsValidAssignment(registerType, registerImplementation))
                    throw new DependencyContainerRegistrationTypeException(registerImplementation, "InstanceFactory");

                this.registerType = registerType;
                this.registerImplementation = registerImplementation;
                _instance = instance;
            }

            public override Type CreatesType => registerImplementation;

            public override object GetObject(Type requestedType, DependencyContainer container, DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options)
            {
                return _instance;
            }

            public override ObjectFactoryBase MultiInstanceVariant => new MultiInstanceFactory(registerType, registerImplementation);

            public override ObjectFactoryBase WeakReferenceVariant => new WeakInstanceFactory(registerType, registerImplementation, _instance);

            public override ObjectFactoryBase StrongReferenceVariant => this;

            public override void SetConstructor(ConstructorInfo constructor)
            {
                throw new DependencyContainerConstructorResolutionException("Constructor selection is not possible for instance factory registrations");
            }

            public void Dispose()
            {
                var disposable = _instance as IDisposable;

                disposable?.Dispose();
            }
        }

        /// <summary>
        /// Stores an particular instance to return for a type
        /// 
        /// Stores the instance with a weak reference
        /// </summary>
        private class WeakInstanceFactory : ObjectFactoryBase, IDisposable
        {
            private readonly Type registerType;
            private readonly Type registerImplementation;
            private readonly WeakReference _instance;

            public WeakInstanceFactory(Type registerType, Type registerImplementation, object instance)
            {
                if (!IsValidAssignment(registerType, registerImplementation))
                    throw new DependencyContainerRegistrationTypeException(registerImplementation, "WeakInstanceFactory");

                this.registerType = registerType;
                this.registerImplementation = registerImplementation;
                _instance = new WeakReference(instance);
            }

            public override Type CreatesType => registerImplementation;

            public override object GetObject(Type requestedType, DependencyContainer container, DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options)
            {
                var instance = _instance.Target;

                if (instance == null)
                    throw new DependencyContainerWeakReferenceException(registerType);

                return instance;
            }

            public override ObjectFactoryBase MultiInstanceVariant => new MultiInstanceFactory(registerType, registerImplementation);

            public override ObjectFactoryBase WeakReferenceVariant => this;

            public override ObjectFactoryBase StrongReferenceVariant
            {
                get
                {
                    var instance = _instance.Target;

                    if (instance == null)
                        throw new DependencyContainerWeakReferenceException(registerType);

                    return new InstanceFactory(registerType, registerImplementation, instance);
                }
            }

            public override void SetConstructor(ConstructorInfo constructor)
            {
                throw new DependencyContainerConstructorResolutionException("Constructor selection is not possible for instance factory registrations");
            }

            public void Dispose()
            {
                var disposable = _instance.Target as IDisposable;

                disposable?.Dispose();
            }
        }

        /// <summary>
        /// A factory that lazy instantiates a type and always returns the same instance
        /// </summary>
        private class SingletonFactory : ObjectFactoryBase, IDisposable
        {
            private readonly Type _registerType;
            private readonly Type _registerImplementation;
            private readonly object _singletonLock = new object();
            private object _current;

            public SingletonFactory(Type registerType, Type registerImplementation)
            {
                if (registerImplementation.IsAbstract() || registerImplementation.IsInterface())
                    throw new DependencyContainerRegistrationTypeException(registerImplementation, nameof(SingletonFactory));

                if (!IsValidAssignment(registerType, registerImplementation))
                    throw new DependencyContainerRegistrationTypeException(registerImplementation, nameof(SingletonFactory));

                _registerType = registerType;
                _registerImplementation = registerImplementation;
            }

            public override Type CreatesType => _registerImplementation;

            public override object GetObject(Type requestedType, DependencyContainer container, DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options)
            {
                if (parameters.Count != 0)
                    throw new ArgumentException("Cannot specify parameters for singleton types");

                lock (_singletonLock)
                {
                    if (_current == null)
                        _current = container.ConstructType(_registerImplementation, Constructor, options);
                }

                return _current;
            }

            public override ObjectFactoryBase SingletonVariant => this;

            public override ObjectFactoryBase GetCustomObjectLifetimeVariant(ITinyIoCObjectLifetimeProvider lifetimeProvider, string errorString)
            {
                return new CustomObjectLifetimeFactory(_registerType, _registerImplementation, lifetimeProvider, errorString);
            }

            public override ObjectFactoryBase MultiInstanceVariant => new MultiInstanceFactory(_registerType, _registerImplementation);

            public override ObjectFactoryBase GetFactoryForChildContainer(Type type, DependencyContainer parent, DependencyContainer child)
            {
                // We make sure that the singleton is constructed before the child container takes the factory.
                // Otherwise the results would vary depending on whether or not the parent container had resolved
                // the type before the child container does.
                GetObject(type, parent, DependencyContainerNamedParameterOverloads.Default, DependencyContainerResolveOptions.Default);
                return this;
            }

            public void Dispose()
            {
                var disposable = _current as IDisposable;

                disposable?.Dispose();
            }
        }

        /// <summary>
        /// A factory that offloads lifetime to an external lifetime provider
        /// </summary>
        private class CustomObjectLifetimeFactory : ObjectFactoryBase, IDisposable
        {
            private readonly object SingletonLock = new object();
            private readonly Type registerType;
            private readonly Type registerImplementation;
            private readonly ITinyIoCObjectLifetimeProvider _LifetimeProvider;

            public CustomObjectLifetimeFactory(Type registerType, Type registerImplementation, ITinyIoCObjectLifetimeProvider lifetimeProvider, string errorMessage)
            {
                if (lifetimeProvider == null)
                    throw new ArgumentNullException(nameof(lifetimeProvider));

                if (!IsValidAssignment(registerType, registerImplementation))
                    throw new DependencyContainerRegistrationTypeException(registerImplementation, "SingletonFactory");

                if (registerImplementation.IsAbstract() || registerImplementation.IsInterface())
                    throw new DependencyContainerRegistrationTypeException(registerImplementation, errorMessage);

                this.registerType = registerType;
                this.registerImplementation = registerImplementation;
                _LifetimeProvider = lifetimeProvider;
            }

            public override Type CreatesType => registerImplementation;

            public override object GetObject(Type requestedType, DependencyContainer container, DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options)
            {
                object current;

                lock (SingletonLock)
                {
                    current = _LifetimeProvider.GetObject();

                    if (current == null)
                    {
                        current = container.ConstructType(registerImplementation, Constructor, options);
                        _LifetimeProvider.SetObject(current);
                    }
                }

                return current;
            }

            public override ObjectFactoryBase SingletonVariant
            {
                get
                {
                    _LifetimeProvider.ReleaseObject();
                    return new SingletonFactory(registerType, registerImplementation);
                }
            }

            public override ObjectFactoryBase MultiInstanceVariant
            {
                get
                {
                    _LifetimeProvider.ReleaseObject();
                    return new MultiInstanceFactory(registerType, registerImplementation);
                }
            }

            public override ObjectFactoryBase GetCustomObjectLifetimeVariant(ITinyIoCObjectLifetimeProvider lifetimeProvider, string errorString)
            {
                _LifetimeProvider.ReleaseObject();
                return new CustomObjectLifetimeFactory(registerType, registerImplementation, lifetimeProvider, errorString);
            }

            public override ObjectFactoryBase GetFactoryForChildContainer(Type type, DependencyContainer parent, DependencyContainer child)
            {
                // We make sure that the singleton is constructed before the child container takes the factory.
                // Otherwise the results would vary depending on whether or not the parent container had resolved
                // the type before the child container does.
                GetObject(type, parent, DependencyContainerNamedParameterOverloads.Default, DependencyContainerResolveOptions.Default);
                return this;
            }

            public void Dispose()
            {
                _LifetimeProvider.ReleaseObject();
            }
        }
        #endregion

        #region Singleton Container

        static DependencyContainer()
        {
        }

        /// <summary>
        /// Lazy created Singleton instance of the container for simple scenarios
        /// </summary>
        public static DependencyContainer Current { get; } = new DependencyContainer();

        #endregion

        #region Type Registrations

        /// <summary>
        /// Represents a Type Registration within the IoC Container
        /// </summary>
        public sealed class TypeRegistration
        {
            private readonly int _hashCode;

            /// <summary>
            /// Gets the type.
            /// </summary>
            /// <value>
            /// The type.
            /// </value>
            public Type Type { get; }
            
            /// <summary>
            /// Gets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="TypeRegistration"/> class.
            /// </summary>
            /// <param name="type">The type.</param>
            /// <param name="name">The name.</param>
            public TypeRegistration(Type type, string name = null)
            {
                Type = type;
                Name = name ?? string.Empty;

                _hashCode = string.Concat(Type.FullName, "|", Name).GetHashCode();
            }

            /// <summary>
            /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
            /// </summary>
            /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
            /// <returns>
            ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
            /// </returns>
            public override bool Equals(object obj)
            {
                var typeRegistration = obj as TypeRegistration;

                if (typeRegistration == null || typeRegistration.Type != Type)
                    return false;

                return string.Compare(Name, typeRegistration.Name, StringComparison.Ordinal) == 0;
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
            /// </returns>
            public override int GetHashCode() =>_hashCode;
        }

        private readonly ConcurrentDictionary<TypeRegistration, ObjectFactoryBase> _registeredTypes;
        private delegate object ObjectConstructor(params object[] parameters);
#if USE_OBJECT_CONSTRUCTOR
        private static readonly ConcurrentDictionary<ConstructorInfo, ObjectConstructor> ObjectConstructorCache
            = new ConcurrentDictionary<ConstructorInfo, ObjectConstructor>();
#endif
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainer"/> class.
        /// </summary>
        public DependencyContainer()
        {
            _registeredTypes = new ConcurrentDictionary<TypeRegistration, ObjectFactoryBase>();

            RegisterDefaultTypes();
        }

        readonly DependencyContainer _parent;
        private DependencyContainer(DependencyContainer parent)
            : this()
        {
            _parent = parent;
        }
        #endregion

        #region Internal Methods
        private readonly object _autoRegisterLock = new object();

        private void AutoRegisterInternal(IEnumerable<Assembly> assemblies, DependencyContainerDuplicateImplementationActions duplicateAction, Func<Type, bool> registrationPredicate)
        {
            lock (_autoRegisterLock)
            {
                var types = assemblies.SelectMany(a => a.GetAllTypes()).Where(t => !IsIgnoredType(t, registrationPredicate)).ToList();

                var concreteTypes = types
                    .Where(type => type.IsClass() && (type.IsAbstract() == false) && (type != GetType() && (type.DeclaringType != GetType()) && (!type.IsGenericTypeDefinition())))
                    .ToList();

                foreach (var type in concreteTypes)
                {
                    try
                    {
                        RegisterInternal(type, string.Empty, GetDefaultObjectFactory(type, type));
                    }
                    catch (MethodAccessException)
                    {
                        // Ignore methods we can't access - added for Silverlight
                    }
                }

                var abstractInterfaceTypes = types.Where(
                        type =>
                            ((type.IsInterface() || type.IsAbstract()) && (type.DeclaringType != GetType()) &&
                             (!type.IsGenericTypeDefinition())));

                foreach (var type in abstractInterfaceTypes)
                {
                    var localType = type;
                    var implementations = concreteTypes.Where(implementationType => localType.IsAssignableFrom(implementationType)).ToList();

                    if (implementations.Skip(1).Any())
                    {
                        if (duplicateAction == DependencyContainerDuplicateImplementationActions.Fail)
                            throw new DependencyContainerAutoRegistrationException(type, implementations);

                        if (duplicateAction == DependencyContainerDuplicateImplementationActions.RegisterMultiple)
                        {
                            RegisterMultiple(type, implementations);
                        }
                    }

                    var firstImplementation = implementations.FirstOrDefault();

                    if (firstImplementation == null) continue;

                    try
                    {
                        RegisterInternal(type, string.Empty, GetDefaultObjectFactory(type, firstImplementation));
                    }
                    catch (MethodAccessException)
                    {
                        // Ignore methods we can't access - added for Silverlight
                    }
                }
            }
        }

        private static bool IsIgnoredAssembly(Assembly assembly)
        {
            // TODO - find a better way to remove "system" assemblies from the auto registration
            var ignoreChecks = new List<Func<Assembly, bool>>()
            {
                asm => asm.FullName.StartsWith("Microsoft.", StringComparison.Ordinal),
                asm => asm.FullName.StartsWith("System.", StringComparison.Ordinal),
                asm => asm.FullName.StartsWith("System,", StringComparison.Ordinal),
                asm => asm.FullName.StartsWith("CR_ExtUnitTest", StringComparison.Ordinal),
                asm => asm.FullName.StartsWith("mscorlib,", StringComparison.Ordinal),
                asm => asm.FullName.StartsWith("CR_VSTest", StringComparison.Ordinal),
                asm => asm.FullName.StartsWith("DevExpress.CodeRush", StringComparison.Ordinal),
                asm => asm.FullName.StartsWith("xunit.", StringComparison.Ordinal),
            };

            return ignoreChecks.Any(check => check(assembly));
        }

        private static bool IsIgnoredType(Type type, Func<Type, bool> registrationPredicate)
        {
            // TODO - find a better way to remove "system" types from the auto registration
            var ignoreChecks = new List<Func<Type, bool>>()
            {
                t => t.FullName.StartsWith("System.", StringComparison.Ordinal),
                t => t.FullName.StartsWith("Microsoft.", StringComparison.Ordinal),
                t => t.IsPrimitive(),
                t => t.IsGenericTypeDefinition(),
                t => (t.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Length == 0) && !(t.IsInterface() || t.IsAbstract())
            };

            if (registrationPredicate != null)
            {
                ignoreChecks.Add(t => !registrationPredicate(t));
            }

            return ignoreChecks.Any(check => check(type));
        }

        private void RegisterDefaultTypes()
        {
            Register(this);

            // Only register the TinyMessenger singleton if we are the root container
            if (_parent == null)
                Register<IMessageHub, MessageHub>();
        }

        private ObjectFactoryBase GetCurrentFactory(TypeRegistration registration)
        {
            ObjectFactoryBase current;

            _registeredTypes.TryGetValue(registration, out current);

            return current;
        }

        private RegisterOptions RegisterInternal(Type registerType, string name, ObjectFactoryBase factory)
        {
            var typeRegistration = new TypeRegistration(registerType, name);

            return AddUpdateRegistration(typeRegistration, factory);
        }

        private RegisterOptions AddUpdateRegistration(TypeRegistration typeRegistration, ObjectFactoryBase factory)
        {
            _registeredTypes[typeRegistration] = factory;

            return new RegisterOptions(this, typeRegistration);
        }

        private bool RemoveRegistration(TypeRegistration typeRegistration)
        {
            ObjectFactoryBase item;
            return _registeredTypes.TryRemove(typeRegistration, out item);
        }

        private static ObjectFactoryBase GetDefaultObjectFactory(Type registerType, Type registerImplementation)
        {
            if (registerType.IsInterface() || registerType.IsAbstract())
                return new SingletonFactory(registerType, registerImplementation);

            return new MultiInstanceFactory(registerType, registerImplementation);
        }

        private bool CanResolveInternal(TypeRegistration registration, DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var checkType = registration.Type;
            var name = registration.Name;

            ObjectFactoryBase factory;
            if (_registeredTypes.TryGetValue(new TypeRegistration(checkType, name), out factory))
            {
                if (factory.AssumeConstruction)
                    return true;

                if (factory.Constructor == null)
                    return GetBestConstructor(factory.CreatesType, parameters, options) != null;

                return CanConstruct(factory.Constructor, parameters, options);
            }
            
            // Fail if requesting named resolution and settings set to fail if unresolved
            // Or bubble up if we have a parent
            if (!string.IsNullOrEmpty(name) && options.NamedResolutionFailureAction == DependencyContainerNamedResolutionFailureActions.Fail)
                return _parent?.CanResolveInternal(registration, parameters, options) ?? false;

            // Attempted unnamed fallback container resolution if relevant and requested
            if (!string.IsNullOrEmpty(name) && options.NamedResolutionFailureAction == DependencyContainerNamedResolutionFailureActions.AttemptUnnamedResolution)
            {
                if (_registeredTypes.TryGetValue(new TypeRegistration(checkType), out factory))
                {
                    if (factory.AssumeConstruction)
                        return true;

                    return GetBestConstructor(factory.CreatesType, parameters, options) != null;
                }
            }

            // Check if type is an automatic lazy factory request
            if (IsAutomaticLazyFactoryRequest(checkType))
                return true;

            // Check if type is an IEnumerable<ResolveType>
            if (registration.Type.IsIEnumerable())
                return true;

            // Attempt unregistered construction if possible and requested
            // If we cant', bubble if we have a parent
            if ((options.UnregisteredResolutionAction == DependencyContainerUnregisteredResolutionActions.AttemptResolve) || (checkType.IsGenericType() && options.UnregisteredResolutionAction == DependencyContainerUnregisteredResolutionActions.GenericsOnly))
                return (GetBestConstructor(checkType, parameters, options) != null) || (_parent?.CanResolveInternal(registration, parameters, options) ?? false);

            // Bubble resolution up the container tree if we have a parent
            return _parent != null && _parent.CanResolveInternal(registration, parameters, options);
        }

        private static bool IsAutomaticLazyFactoryRequest(Type type)
        {
            if (!type.IsGenericType())
                return false;

            var genericType = type.GetGenericTypeDefinition();

            // Just a func
            if (genericType == typeof(Func<>))
                return true;

            // 2 parameter func with string as first parameter (name)
            if (genericType == typeof(Func<,>) && type.GetGenericArguments()[0] == typeof(string))
                return true;

            // 3 parameter func with string as first parameter (name) and IDictionary<string, object> as second (parameters)
            if (genericType == typeof(Func<,,>) && type.GetGenericArguments()[0] == typeof(string) &&
                type.GetGenericArguments()[1] == typeof(IDictionary<String, object>))
                return true;

            return false;
        }

        private ObjectFactoryBase GetParentObjectFactory(TypeRegistration registration)
        {
            if (_parent == null)
                return null;

            ObjectFactoryBase factory;
            if (_parent._registeredTypes.TryGetValue(registration, out factory))
            {
                return factory.GetFactoryForChildContainer(registration.Type, _parent, this);
            }

            return _parent.GetParentObjectFactory(registration);
        }

        private object ResolveInternal(TypeRegistration registration, DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options)
        {
            ObjectFactoryBase factory;

            // Attempt container resolution
            if (_registeredTypes.TryGetValue(registration, out factory))
            {
                try
                {
                    return factory.GetObject(registration.Type, this, parameters, options);
                }
                catch (DependencyContainerResolutionException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new DependencyContainerResolutionException(registration.Type, ex);
                }
            }

            // Attempt to get a factory from parent if we can
            var bubbledObjectFactory = GetParentObjectFactory(registration);
            if (bubbledObjectFactory != null)
            {
                try
                {
                    return bubbledObjectFactory.GetObject(registration.Type, this, parameters, options);
                }
                catch (DependencyContainerResolutionException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new DependencyContainerResolutionException(registration.Type, ex);
                }
            }

            // Fail if requesting named resolution and settings set to fail if unresolved
            if (!string.IsNullOrEmpty(registration.Name) && options.NamedResolutionFailureAction == DependencyContainerNamedResolutionFailureActions.Fail)
                throw new DependencyContainerResolutionException(registration.Type);

            // Attempted unnamed fallback container resolution if relevant and requested
            if (!string.IsNullOrEmpty(registration.Name) && options.NamedResolutionFailureAction == DependencyContainerNamedResolutionFailureActions.AttemptUnnamedResolution)
            {
                if (_registeredTypes.TryGetValue(new TypeRegistration(registration.Type, string.Empty), out factory))
                {
                    try
                    {
                        return factory.GetObject(registration.Type, this, parameters, options);
                    }
                    catch (DependencyContainerResolutionException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        throw new DependencyContainerResolutionException(registration.Type, ex);
                    }
                }
            }
            
            // Attempt unregistered construction if possible and requested
            if ((options.UnregisteredResolutionAction == DependencyContainerUnregisteredResolutionActions.AttemptResolve) || (registration.Type.IsGenericType() && options.UnregisteredResolutionAction == DependencyContainerUnregisteredResolutionActions.GenericsOnly))
            {
                if (!registration.Type.IsAbstract() && !registration.Type.IsInterface())
                    return ConstructType(registration.Type, parameters, options);
            }

            // Unable to resolve - throw
            throw new DependencyContainerResolutionException(registration.Type);
        }
        
        private bool CanConstruct(ConstructorInfo ctor, DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            foreach (var parameter in ctor.GetParameters())
            {
                if (string.IsNullOrEmpty(parameter.Name))
                    return false;

                var isParameterOverload = parameters.ContainsKey(parameter.Name);

                if (parameter.ParameterType.IsPrimitive() && !isParameterOverload)
                    return false;

                if (!isParameterOverload && !CanResolveInternal(new TypeRegistration(parameter.ParameterType), DependencyContainerNamedParameterOverloads.Default, options))
                    return false;
            }

            return true;
        }

        private ConstructorInfo GetBestConstructor(Type type, DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            if (type.IsValueType())
                return null;

            // Get constructors in reverse order based on the number of parameters
            // i.e. be as "greedy" as possible so we satisfy the most amount of dependencies possible
            var ctors = GetTypeConstructors(type);

            return ctors.FirstOrDefault(ctor => CanConstruct(ctor, parameters, options));
        }

        private static IEnumerable<ConstructorInfo> GetTypeConstructors(Type type)
        {
            return type.GetConstructors().OrderByDescending(ctor => ctor.GetParameters().Length);
        }

        private object ConstructType(Type implementationType, ConstructorInfo constructor, DependencyContainerResolveOptions options)
        {
            return ConstructType(implementationType, constructor, DependencyContainerNamedParameterOverloads.Default, options);
        }

        private object ConstructType(Type implementationType, DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options)
        {
            return ConstructType(implementationType, null, parameters, options);
        }

        private object ConstructType(Type implementationType, ConstructorInfo constructor, DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options)
        {
            var typeToConstruct = implementationType;

            if (constructor == null)
            {
                // Try and get the best constructor that we can construct
                // if we can't construct any then get the constructor
                // with the least number of parameters so we can throw a meaningful
                // resolve exception
                constructor = GetBestConstructor(typeToConstruct, parameters, options) ?? GetTypeConstructors(typeToConstruct).LastOrDefault();
            }

            if (constructor == null)
                throw new DependencyContainerResolutionException(typeToConstruct);

            var ctorParams = constructor.GetParameters();
            var args = new object[ctorParams.Length];

            for (var parameterIndex = 0; parameterIndex < ctorParams.Length; parameterIndex++)
            {
                var currentParam = ctorParams[parameterIndex];

                try
                {
                    args[parameterIndex] = parameters.ContainsKey(currentParam.Name) ?
                                            parameters[currentParam.Name] :
                                            ResolveInternal(
                                                new TypeRegistration(currentParam.ParameterType),
                                                DependencyContainerNamedParameterOverloads.Default,
                                                options);
                }
                catch (DependencyContainerResolutionException ex)
                {
                    // If a constructor parameter can't be resolved
                    // it will throw, so wrap it and throw that this can't
                    // be resolved.
                    throw new DependencyContainerResolutionException(typeToConstruct, ex);
                }
                catch (Exception ex)
                {
                    throw new DependencyContainerResolutionException(typeToConstruct, ex);
                }
            }

            try
            {
#if USE_OBJECT_CONSTRUCTOR
                var constructionDelegate = CreateObjectConstructionDelegateWithCache(constructor);
                return constructionDelegate.Invoke(args);
#else
                return constructor.Invoke(args);
#endif
            }
            catch (Exception ex)
            {
                throw new DependencyContainerResolutionException(typeToConstruct, ex);
            }
        }

#if USE_OBJECT_CONSTRUCTOR
        private static ObjectConstructor CreateObjectConstructionDelegateWithCache(ConstructorInfo constructor)
        {
            ObjectConstructor objectConstructor;
            if (ObjectConstructorCache.TryGetValue(constructor, out objectConstructor))
                return objectConstructor;

            // We could lock the cache here, but there's no real side
            // effect to two threads creating the same ObjectConstructor
            // at the same time, compared to the cost of a lock for 
            // every creation.
            var constructorParams = constructor.GetParameters();
            var lambdaParams = Expression.Parameter(typeof(object[]), "parameters");
            var newParams = new Expression[constructorParams.Length];

            for (var i = 0; i < constructorParams.Length; i++)
            {
                var paramsParameter = Expression.ArrayIndex(lambdaParams, Expression.Constant(i));

                newParams[i] = Expression.Convert(paramsParameter, constructorParams[i].ParameterType);
            }

            var newExpression = Expression.New(constructor, newParams);

            var constructionLambda = Expression.Lambda(typeof(ObjectConstructor), newExpression, lambdaParams);

            objectConstructor = (ObjectConstructor)constructionLambda.Compile();

            ObjectConstructorCache[constructor] = objectConstructor;
            return objectConstructor;
        }
#endif

        private void BuildUpInternal(object input, DependencyContainerResolveOptions resolveOptions)
        {
            var properties = from property in input.GetType().GetProperties()
                             where (property.GetGetMethod() != null) && (property.GetSetMethod() != null) && !property.PropertyType.IsValueType()
                             select property;

            foreach (var property in properties)
            {
                if (property.GetValue(input, null) != null) continue;

                try
                {
                    property.SetValue(input, ResolveInternal(new TypeRegistration(property.PropertyType), DependencyContainerNamedParameterOverloads.Default, resolveOptions), null);
                }
                catch (DependencyContainerResolutionException)
                {
                    // Catch any resolution errors and ignore them
                }
            }
        }

        private IEnumerable<TypeRegistration> GetParentRegistrationsForType(Type resolveType)
        {
            if (_parent == null)
                return new TypeRegistration[] { };

            var registrations = _parent._registeredTypes.Keys.Where(tr => tr.Type == resolveType);

            return registrations.Concat(_parent.GetParentRegistrationsForType(resolveType));
        }

        private IEnumerable<object> ResolveAllInternal(Type resolveType, bool includeUnnamed)
        {
            var registrations = _registeredTypes.Keys.Where(tr => tr.Type == resolveType).Concat(GetParentRegistrationsForType(resolveType)).Distinct();

            if (!includeUnnamed)
                registrations = registrations.Where(tr => tr.Name != string.Empty);

            return registrations.Select(registration => ResolveInternal(registration, DependencyContainerNamedParameterOverloads.Default, DependencyContainerResolveOptions.Default));
        }

        private static bool IsValidAssignment(Type registerType, Type registerImplementation)
        {
            if (!registerType.IsGenericTypeDefinition())
            {
                if (!registerType.IsAssignableFrom(registerImplementation))
                    return false;
            }
            else
            {
                if (registerType.IsInterface())
                {
                    if (registerImplementation.GetInterfaces().All(t => t.Name != registerType.Name))
                        return false;
                }
                else if (registerType.IsAbstract() && registerImplementation.BaseType() != registerType)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region IDisposable Members

        private bool _disposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            foreach (var item in _registeredTypes.Values)
            {
                var disposable = item as IDisposable;
                disposable?.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}