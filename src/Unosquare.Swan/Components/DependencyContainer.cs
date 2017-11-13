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

namespace Unosquare.Swan.Components
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Exceptions;

    /// <summary>
    /// The concrete implementation of a simple IoC container
    /// based largely on TinyIoC
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public partial class DependencyContainer : IDisposable
    {
        private static readonly ConcurrentDictionary<ConstructorInfo, ObjectConstructor> ObjectConstructorCache = new ConcurrentDictionary<ConstructorInfo, ObjectConstructor>();

        private readonly DependencyContainer _parent;
        private readonly object _autoRegisterLock = new object();
        private readonly ConcurrentDictionary<TypeRegistration, ObjectFactoryBase> _registeredTypes;
        
        private bool _disposed;

        static DependencyContainer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainer"/> class.
        /// </summary>
        public DependencyContainer()
        {
            _registeredTypes = new ConcurrentDictionary<TypeRegistration, ObjectFactoryBase>();

            RegisterDefaultTypes();
        }

        private DependencyContainer(DependencyContainer parent)
            : this()
        {
            _parent = parent;
        }

        private delegate object ObjectConstructor(params object[] parameters);

        /// <summary>
        /// Lazy created Singleton instance of the container for simple scenarios
        /// </summary>
        public static DependencyContainer Current { get; } = new DependencyContainer();

        /// <summary>
        /// Gets the child container.
        /// </summary>
        /// <returns>A new instance of the <see cref="DependencyContainer"/> class</returns>
        public DependencyContainer GetChildContainer() => new DependencyContainer(this);

        #region Registration

#if !NETSTANDARD1_3 && !UWP
        /// <summary>
        /// Attempt to automatically register all non-generic classes and interfaces in the current app domain.
        /// Types will only be registered if they pass the supplied registration predicate.
        /// </summary>
        /// <param name="duplicateAction">What action to take when encountering duplicate implementations of an interface/base class.</param>
        /// <param name="registrationPredicate">Predicate to determine if a particular type should be registered</param>
        public void AutoRegister(
            DependencyContainerDuplicateImplementationActions duplicateAction =
                DependencyContainerDuplicateImplementationActions.RegisterSingle,
            Func<Type, bool> registrationPredicate = null)
        {
            AutoRegister(
                Runtime.GetAssemblies().Where(a => !IsIgnoredAssembly(a)), duplicateAction,
                registrationPredicate);
        }
#endif

        /// <summary>
        /// Attempt to automatically register all non-generic classes and interfaces in the specified assemblies
        /// Types will only be registered if they pass the supplied registration predicate.
        /// </summary>
        /// <param name="assemblies">Assemblies to process</param>
        /// <param name="duplicateAction">What action to take when encountering duplicate implementations of an interface/base class.</param>
        /// <param name="registrationPredicate">Predicate to determine if a particular type should be registered</param>
        public void AutoRegister(
            IEnumerable<Assembly> assemblies,
            DependencyContainerDuplicateImplementationActions duplicateAction = DependencyContainerDuplicateImplementationActions.RegisterSingle,
            Func<Type, bool> registrationPredicate = null)
        {
            lock (_autoRegisterLock)
            {
                var types = assemblies.SelectMany(a => a.GetAllTypes())
                    .Where(t => !IsIgnoredType(t, registrationPredicate)).ToList();

                var concreteTypes = types
                    .Where(type =>
                        type.IsClass() && (type.IsAbstract() == false) &&
                        (type != GetType() && (type.DeclaringType != GetType()) && (!type.IsGenericTypeDefinition())))
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
                    var implementations = concreteTypes
                        .Where(implementationType => localType.IsAssignableFrom(implementationType)).ToList();

                    if (implementations.Skip(1).Any())
                    {
                        if (duplicateAction == DependencyContainerDuplicateImplementationActions.Fail)
                            throw new DependencyContainerRegistrationException(type, implementations);

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
        public RegisterOptions Register(Type registerType, Func<DependencyContainer, Dictionary<string, object>, object> factory, string name = "")
        {
            return RegisterInternal(registerType, name, new DelegateFactory(registerType, factory));
        }

        /// <summary>
        /// Creates/replaces a named container class registration with default options.
        /// </summary>
        /// <typeparam name="TRegister">Type to register</typeparam>
        /// <param name="name">Name of registration</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register<TRegister>(string name = "")
            where TRegister : class
        {
            return Register(typeof(TRegister), name);
        }

        /// <summary>
        /// Creates/replaces a named container class registration with a given implementation and default options.
        /// </summary>
        /// <typeparam name="TRegister">Type to register</typeparam>
        /// <typeparam name="TRegisterImplementation">Type to instantiate that implements RegisterType</typeparam>
        /// <param name="name">Name of registration</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register<TRegister, TRegisterImplementation>(string name = "")
            where TRegister : class
            where TRegisterImplementation : class, TRegister
        {
            return Register(typeof(TRegister), typeof(TRegisterImplementation), name);
        }

        /// <summary>
        /// Creates/replaces a named container class registration with a specific, strong referenced, instance.
        /// </summary>
        /// <typeparam name="TRegister">Type to register</typeparam>
        /// <param name="instance">Instance of RegisterType to register</param>
        /// <param name="name">Name of registration</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register<TRegister>(TRegister instance, string name = "")
            where TRegister : class
        {
            return Register(typeof(TRegister), instance, name);
        }

        /// <summary>
        /// Creates/replaces a named container class registration with a specific, strong referenced, instance.
        /// </summary>
        /// <typeparam name="TRegister">Type to register</typeparam>
        /// <typeparam name="TRegisterImplementation">Type of instance to register that implements RegisterType</typeparam>
        /// <param name="instance">Instance of RegisterImplementation to register</param>
        /// <param name="name">Name of registration</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register<TRegister, TRegisterImplementation>(TRegisterImplementation instance, string name = "")
            where TRegister : class
            where TRegisterImplementation : class, TRegister
        {
            return Register(typeof(TRegister), typeof(TRegisterImplementation), instance, name);
        }

        /// <summary>
        /// Creates/replaces a named container class registration with a user specified factory
        /// </summary>
        /// <typeparam name="TRegister">Type to register</typeparam>
        /// <param name="factory">Factory/lambda that returns an instance of RegisterType</param>
        /// <param name="name">Name of registation</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register<TRegister>(Func<DependencyContainer, Dictionary<string, object>, TRegister> factory, string name = "")
            where TRegister : class
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            return Register(typeof(TRegister), factory, name);
        }

        /// <summary>
        /// Register multiple implementations of a type.
        /// 
        /// Internally this registers each implementation using the full name of the class as its registration name.
        /// </summary>
        /// <typeparam name="TRegister">Type that each implementation implements</typeparam>
        /// <param name="implementationTypes">Types that implement RegisterType</param>
        /// <returns>MultiRegisterOptions for the fluent API</returns>
        public MultiRegisterOptions RegisterMultiple<TRegister>(IEnumerable<Type> implementationTypes)
        {
            return RegisterMultiple(typeof(TRegister), implementationTypes);
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

            foreach (var type in implementationTypes.Where(type => !registrationType.IsAssignableFrom(type)))
            {
                throw new ArgumentException($"types: The type {registrationType.FullName} is not assignable from {type.FullName}");
            }

            if (implementationTypes.Count() != implementationTypes.Distinct().Count())
            {
                var queryForDuplicatedTypes = implementationTypes
                    .GroupBy(i => i)
                    .Where(j => j.Count() > 1)
                    .Select(j => j.Key.FullName);

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
        /// <typeparam name="TRegister">Type to unregister</typeparam>
        /// <param name="name">Name of registration</param>
        /// <returns><c>true</c> if the registration is successfully found and removed; otherwise, <c>false</c>.</returns>
        public bool Unregister<TRegister>(string name = "")
        {
            return Unregister(typeof(TRegister), name);
        }

        /// <summary>
        /// Remove a named container class registration.
        /// </summary>
        /// <param name="registerType">Type to unregister</param>
        /// <param name="name">Name of registration</param>
        /// <returns><c>true</c> if the registration is successfully found and removed; otherwise, <c>false</c>.</returns>
        public bool Unregister(Type registerType, string name = "")
        {
            var typeRegistration = new TypeRegistration(registerType, name);

            return RemoveRegistration(typeRegistration);
        }

        #endregion

        #region Resolution
        
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
        public object Resolve(Type resolveType, Dictionary<string, object> parameters = null, DependencyContainerResolveOptions options = null)
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
        public object Resolve(Type resolveType, string name, Dictionary<string, object> parameters = null, DependencyContainerResolveOptions options = null)
        {
            return ResolveInternal(new TypeRegistration(resolveType, name), parameters, options ?? DependencyContainerResolveOptions.Default);
        }

        /// <summary>
        /// Attempts to resolve a type using specified options.
        /// </summary>
        /// <typeparam name="TResolveType">Type to resolve</typeparam>
        /// <param name="options">Resolution options</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="DependencyContainerResolutionException">Unable to resolve the type.</exception>
        public TResolveType Resolve<TResolveType>(DependencyContainerResolveOptions options = null)
            where TResolveType : class
        {
            return (TResolveType)Resolve(typeof(TResolveType), null, options);
        }

        /// <summary>
        /// Attempts to resolve a type using supplied options and  name.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// </summary>
        /// <typeparam name="TResolveType">Type to resolve</typeparam>
        /// <param name="name">Name of registration</param>
        /// <param name="options">Resolution options</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="DependencyContainerResolutionException">Unable to resolve the type.</exception>
        public TResolveType Resolve<TResolveType>(string name, DependencyContainerResolveOptions options = null)
            where TResolveType : class
        {
            return (TResolveType)Resolve(typeof(TResolveType), name, null, options);
        }

        /// <summary>
        /// Attempts to resolve a type using specified options and the supplied constructor parameters.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// </summary>
        /// <typeparam name="TResolveType">Type to resolve</typeparam>
        /// <param name="parameters">User specified constructor parameters</param>
        /// <param name="options">Resolution options</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="DependencyContainerResolutionException">Unable to resolve the type.</exception>
        public TResolveType Resolve<TResolveType>(Dictionary<string, object> parameters, DependencyContainerResolveOptions options = null)
            where TResolveType : class
        {
            return (TResolveType)Resolve(typeof(TResolveType), parameters, options);
        }

        /// <summary>
        /// Attempts to resolve a named type using specified options and the supplied constructor parameters.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// </summary>
        /// <typeparam name="TResolveType">Type to resolve</typeparam>
        /// <param name="name">Name of registration</param>
        /// <param name="parameters">User specified constructor parameters</param>
        /// <param name="options">Resolution options</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="DependencyContainerResolutionException">Unable to resolve the type.</exception>
        public TResolveType Resolve<TResolveType>(string name, Dictionary<string, object> parameters, DependencyContainerResolveOptions options = null)
            where TResolveType : class
        {
            return (TResolveType)Resolve(typeof(TResolveType), name, parameters, options);
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
        public bool CanResolve(Type resolveType, Dictionary<string, object> parameters = null, string name = null, DependencyContainerResolveOptions options = null)
        {
            return CanResolveInternal(new TypeRegistration(resolveType, name), parameters, options ?? DependencyContainerResolveOptions.Default);
        }

        /// <summary>
        /// Attempts to predict whether a given type can be resolved with the specified options.
        ///
        /// Note: Resolution may still fail if user defined factory registrations fail to construct objects when called.
        /// </summary>
        /// <typeparam name="TResolveType">Type to resolve</typeparam>
        /// <param name="options">Resolution options</param>
        /// <returns>Bool indicating whether the type can be resolved</returns>
        public bool CanResolve<TResolveType>(DependencyContainerResolveOptions options = null)
            where TResolveType : class
        {
            return CanResolve(typeof(TResolveType), null, null, options);
        }
        
        /// <summary>
        /// Attempts to predict whether a given type can be resolved with the supplied constructor parameters options.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// 
        /// Note: Resolution may still fail if user defined factory registrations fail to construct objects when called.
        /// </summary>
        /// <typeparam name="TResolveType">Type to resolve</typeparam>
        /// <param name="parameters">User supplied named parameter overloads</param>
        /// <param name="options">Resolution options</param>
        /// <returns>Bool indicating whether the type can be resolved</returns>
        public bool CanResolve<TResolveType>(Dictionary<string, object> parameters, DependencyContainerResolveOptions options = null)
            where TResolveType : class
        {
            return CanResolve(typeof(TResolveType), parameters, string.Empty, options);
        }

        /// <summary>
        /// Attempts to predict whether a given named type can be resolved with the supplied constructor parameters options.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// 
        /// Note: Resolution may still fail if user defined factory registrations fail to construct objects when called.
        /// </summary>
        /// <typeparam name="TResolveType">Type to resolve</typeparam>
        /// <param name="name">Name of registration</param>
        /// <param name="parameters">User supplied named parameter overloads</param>
        /// <param name="options">Resolution options</param>
        /// <returns>Bool indicating whether the type can be resolved</returns>
        public bool CanResolve<TResolveType>(string name, Dictionary<string, object> parameters = null, DependencyContainerResolveOptions options = null)
            where TResolveType : class
        {
            return CanResolve(typeof(TResolveType), parameters, name, options);
        }

        /// <summary>
        /// Attempts to resolve a type using the default options
        /// </summary>
        /// <param name="resolveType">Type to resolve</param>
        /// <param name="resolvedType">Resolved type or default if resolve fails</param>
        /// <returns><c>true</c> if resolved successfully, false otherwise</returns>
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
        /// <returns><c>true</c> if resolved successfully, false otherwise</returns>
        public bool TryResolve(Type resolveType, DependencyContainerResolveOptions options, out object resolvedType)
        {
            try
            {
                resolvedType = Resolve(resolveType, null, options);
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
        /// <returns><c>true</c> if resolved successfully, false otherwise</returns>
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
        /// <returns><c>true</c> if resolved successfully, false otherwise</returns>
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
        /// <returns><c>true</c> if resolved successfully, false otherwise</returns>
        public bool TryResolve(Type resolveType, Dictionary<string, object> parameters, out object resolvedType)
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
        /// <returns><c>true</c> if resolved successfully, false otherwise</returns>
        public bool TryResolve(Type resolveType, string name, Dictionary<string, object> parameters, out object resolvedType)
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
        /// <returns><c>true</c> if resolved successfully, false otherwise</returns>
        public bool TryResolve(Type resolveType, Dictionary<string, object> parameters, DependencyContainerResolveOptions options, out object resolvedType)
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
        /// <returns><c>true</c> if resolved successfully, false otherwise</returns>
        public bool TryResolve(Type resolveType, string name, Dictionary<string, object> parameters, DependencyContainerResolveOptions options, out object resolvedType)
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
        /// <typeparam name="TResolveType">Type to resolve</typeparam>
        /// <param name="resolvedType">Resolved type or default if resolve fails</param>
        /// <returns><c>true</c> if resolved successfully, false otherwise</returns>
        public bool TryResolve<TResolveType>(out TResolveType resolvedType)
            where TResolveType : class
        {
            try
            {
                resolvedType = Resolve<TResolveType>();
                return true;
            }
            catch (DependencyContainerResolutionException)
            {
                resolvedType = default(TResolveType);
                return false;
            }
        }

        /// <summary>
        /// Attempts to resolve a type using the given options
        /// </summary>
        /// <typeparam name="TResolveType">Type to resolve</typeparam>
        /// <param name="options">Resolution options</param>
        /// <param name="resolvedType">Resolved type or default if resolve fails</param>
        /// <returns><c>true</c> if resolved successfully, false otherwise</returns>
        public bool TryResolve<TResolveType>(DependencyContainerResolveOptions options, out TResolveType resolvedType)
            where TResolveType : class
        {
            try
            {
                resolvedType = Resolve<TResolveType>(options);
                return true;
            }
            catch (DependencyContainerResolutionException)
            {
                resolvedType = default(TResolveType);
                return false;
            }
        }

        /// <summary>
        /// Attempts to resolve a type using the default options and given name
        /// </summary>
        /// <typeparam name="TResolveType">Type to resolve</typeparam>
        /// <param name="name">Name of registration</param>
        /// <param name="resolvedType">Resolved type or default if resolve fails</param>
        /// <returns><c>true</c> if resolved successfully, false otherwise</returns>
        public bool TryResolve<TResolveType>(string name, out TResolveType resolvedType)
            where TResolveType : class
        {
            try
            {
                resolvedType = Resolve<TResolveType>(name);
                return true;
            }
            catch (DependencyContainerResolutionException)
            {
                resolvedType = default(TResolveType);
                return false;
            }
        }

        /// <summary>
        /// Attempts to resolve a type using the given options and name
        /// </summary>
        /// <typeparam name="TResolveType">Type to resolve</typeparam>
        /// <param name="name">Name of registration</param>
        /// <param name="options">Resolution options</param>
        /// <param name="resolvedType">Resolved type or default if resolve fails</param>
        /// <returns><c>true</c> if resolved successfully, false otherwise</returns>
        public bool TryResolve<TResolveType>(string name, DependencyContainerResolveOptions options, out TResolveType resolvedType)
            where TResolveType : class
        {
            try
            {
                resolvedType = Resolve<TResolveType>(name, options);
                return true;
            }
            catch (DependencyContainerResolutionException)
            {
                resolvedType = default(TResolveType);
                return false;
            }
        }

        /// <summary>
        /// Attempts to resolve a type using the default options and supplied constructor parameters
        /// </summary>
        /// <typeparam name="TResolveType">Type to resolve</typeparam>
        /// <param name="parameters">User specified constructor parameters</param>
        /// <param name="resolvedType">Resolved type or default if resolve fails</param>
        /// <returns><c>true</c> if resolved successfully, false otherwise</returns>
        public bool TryResolve<TResolveType>(Dictionary<string, object> parameters, out TResolveType resolvedType)
            where TResolveType : class
        {
            try
            {
                resolvedType = Resolve<TResolveType>(parameters);
                return true;
            }
            catch (DependencyContainerResolutionException)
            {
                resolvedType = default(TResolveType);
                return false;
            }
        }

        /// <summary>
        /// Attempts to resolve a type using the default options and supplied name and constructor parameters
        /// </summary>
        /// <typeparam name="TResolveType">Type to resolve</typeparam>
        /// <param name="name">Name of registration</param>
        /// <param name="parameters">User specified constructor parameters</param>
        /// <param name="resolvedType">Resolved type or default if resolve fails</param>
        /// <returns><c>true</c> if resolved successfully, false otherwise</returns>
        public bool TryResolve<TResolveType>(string name, Dictionary<string, object> parameters, out TResolveType resolvedType)
            where TResolveType : class
        {
            try
            {
                resolvedType = Resolve<TResolveType>(name, parameters);
                return true;
            }
            catch (DependencyContainerResolutionException)
            {
                resolvedType = default(TResolveType);
                return false;
            }
        }

        /// <summary>
        /// Attempts to resolve a type using the supplied options and constructor parameters
        /// </summary>
        /// <typeparam name="TResolveType">Type to resolve</typeparam>
        /// <param name="parameters">User specified constructor parameters</param>
        /// <param name="options">Resolution options</param>
        /// <param name="resolvedType">Resolved type or default if resolve fails</param>
        /// <returns><c>true</c> if resolved successfully, false otherwise</returns>
        public bool TryResolve<TResolveType>(Dictionary<string, object> parameters, DependencyContainerResolveOptions options, out TResolveType resolvedType)
            where TResolveType : class
        {
            try
            {
                resolvedType = Resolve<TResolveType>(parameters, options);
                return true;
            }
            catch (DependencyContainerResolutionException)
            {
                resolvedType = default(TResolveType);
                return false;
            }
        }

        /// <summary>
        /// Attempts to resolve a type using the supplied name, options and constructor parameters
        /// </summary>
        /// <typeparam name="TResolveType">Type to resolve</typeparam>
        /// <param name="name">Name of registration</param>
        /// <param name="parameters">User specified constructor parameters</param>
        /// <param name="options">Resolution options</param>
        /// <param name="resolvedType">Resolved type or default if resolve fails</param>
        /// <returns><c>true</c> if resolved successfully, false otherwise</returns>
        public bool TryResolve<TResolveType>(string name, Dictionary<string, object> parameters, DependencyContainerResolveOptions options, out TResolveType resolvedType)
            where TResolveType : class
        {
            try
            {
                resolvedType = Resolve<TResolveType>(name, parameters, options);
                return true;
            }
            catch (DependencyContainerResolutionException)
            {
                resolvedType = default(TResolveType);
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
            var registrations = _registeredTypes.Keys.Where(tr => tr.Type == resolveType).Concat(GetParentRegistrationsForType(resolveType)).Distinct();

            if (!includeUnnamed)
                registrations = registrations.Where(tr => tr.Name != string.Empty);

            return registrations.Select(registration => ResolveInternal(registration, null, DependencyContainerResolveOptions.Default));
        }

        /// <summary>
        /// Returns all registrations of a type
        /// </summary>
        /// <typeparam name="TResolveType">Type to resolveAll</typeparam>
        /// <param name="includeUnnamed">Whether to include un-named (default) registrations</param>
        /// <returns>IEnumerable</returns>
        public IEnumerable<TResolveType> ResolveAll<TResolveType>(bool includeUnnamed = true)
            where TResolveType : class
        {
            return ResolveAll(typeof(TResolveType), includeUnnamed).Cast<TResolveType>();
        }

        /// <summary>
        /// Attempts to resolve all public property dependencies on the given object using the given resolve options.
        /// </summary>
        /// <param name="input">Object to "build up"</param>
        /// <param name="resolveOptions">Resolve options to use</param>
        public void BuildUp(object input, DependencyContainerResolveOptions resolveOptions = null)
        {
            if (resolveOptions == null)
                resolveOptions = DependencyContainerResolveOptions.Default;

            var properties = input.GetType()
                .GetProperties()
                .Where(property => (property.GetGetMethod() != null) && (property.GetSetMethod() != null) &&
                                   !property.PropertyType.IsValueType());

            foreach (var property in properties.Where(property => property.GetValue(input, null) == null))
            {
                try
                {
                    property.SetValue(input, ResolveInternal(new TypeRegistration(property.PropertyType), null, resolveOptions), null);
                }
                catch (DependencyContainerResolutionException)
                {
                    // Catch any resolution errors and ignore them
                }
            }
        }
        #endregion
        
        #region Internal Methods
        
#if !NETSTANDARD1_3 && !UWP
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
#endif

        private static bool IsIgnoredType(Type type, Func<Type, bool> registrationPredicate)
        {
            // TODO - find a better way to remove "system" types from the auto registration
            var ignoreChecks = new List<Func<Type, bool>>()
            {
                t => t.FullName?.StartsWith("System.", StringComparison.Ordinal) ?? false,
                t => t.FullName?.StartsWith("Microsoft.", StringComparison.Ordinal)?? false,
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

        private static ObjectFactoryBase GetDefaultObjectFactory(Type registerType, Type registerImplementation)
        {
            return registerType.IsInterface() || registerType.IsAbstract()
                ? (ObjectFactoryBase) new SingletonFactory(registerType, registerImplementation)
                : new MultiInstanceFactory(registerType, registerImplementation);
        }

        private static IEnumerable<ConstructorInfo> GetTypeConstructors(Type type)
            => type.GetConstructors().OrderByDescending(ctor => ctor.GetParameters().Length);

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
                type.GetGenericArguments()[1] == typeof(IDictionary<string, object>))
                return true;

            return false;
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
            _registeredTypes.TryGetValue(registration, out var current);

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
            => _registeredTypes.TryRemove(typeRegistration, out var item);

        private bool CanResolveInternal(TypeRegistration registration, Dictionary<string, object> parameters, DependencyContainerResolveOptions options)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();

            var checkType = registration.Type;
            var name = registration.Name;

            if (_registeredTypes.TryGetValue(new TypeRegistration(checkType, name), out var factory))
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

        private ObjectFactoryBase GetParentObjectFactory(TypeRegistration registration)
        {
            if (_parent == null)
                return null;

            if (_parent._registeredTypes.TryGetValue(registration, out var factory))
            {
                return factory.GetFactoryForChildContainer(registration.Type, _parent, this);
            }

            return _parent.GetParentObjectFactory(registration);
        }

        private object ResolveInternal(TypeRegistration registration, Dictionary<string, object> parameters, DependencyContainerResolveOptions options)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();

            // Attempt container resolution
            if (_registeredTypes.TryGetValue(registration, out var factory))
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
                    return ConstructType(registration.Type, null, parameters, options);
            }

            // Unable to resolve - throw
            throw new DependencyContainerResolutionException(registration.Type);
        }

        private bool CanConstruct(ConstructorInfo ctor, Dictionary<string, object> parameters, DependencyContainerResolveOptions options)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();

            foreach (var parameter in ctor.GetParameters())
            {
                if (string.IsNullOrEmpty(parameter.Name))
                    return false;

                var isParameterOverload = parameters.ContainsKey(parameter.Name);

                if (parameter.ParameterType.IsPrimitive() && !isParameterOverload)
                    return false;

                if (!isParameterOverload && !CanResolveInternal(new TypeRegistration(parameter.ParameterType), null, options))
                    return false;
            }

            return true;
        }

        private ConstructorInfo GetBestConstructor(Type type, Dictionary<string, object> parameters, DependencyContainerResolveOptions options)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();

            if (type.IsValueType())
                return null;

            // Get constructors in reverse order based on the number of parameters
            // i.e. be as "greedy" as possible so we satisfy the most amount of dependencies possible
            var ctors = GetTypeConstructors(type);

            return ctors.FirstOrDefault(ctor => CanConstruct(ctor, parameters, options));
        }

        private object ConstructType(
            Type implementationType, 
            ConstructorInfo constructor, 
            Dictionary<string, object> parameters, 
            DependencyContainerResolveOptions options = null)
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
                                            ResolveInternal(new TypeRegistration(currentParam.ParameterType), null, options);
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
                var constructionDelegate = CreateObjectConstructionDelegateWithCache(constructor);
                return constructionDelegate.Invoke(args);
            }
            catch (Exception ex)
            {
                throw new DependencyContainerResolutionException(typeToConstruct, ex);
            }
        }
        
        private static ObjectConstructor CreateObjectConstructionDelegateWithCache(ConstructorInfo constructor)
        {
            if (ObjectConstructorCache.TryGetValue(constructor, out var objectConstructor))
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
        
        private IEnumerable<TypeRegistration> GetParentRegistrationsForType(Type resolveType)
        {
            if (_parent == null)
                return new TypeRegistration[] { };

            var registrations = _parent._registeredTypes.Keys.Where(tr => tr.Type == resolveType);

            return registrations.Concat(_parent.GetParentRegistrationsForType(resolveType));
        }
        
        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            foreach (var disposable in _registeredTypes.Values.Select(item => item as IDisposable))
            {
                disposable?.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        #endregion

        #region Type Registrations

        /// <summary>
        /// Represents a Type Registration within the IoC Container
        /// </summary>
        public sealed class TypeRegistration
        {
            private readonly int _hashCode;

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
            /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
            /// </summary>
            /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
            /// <returns>
            ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
            /// </returns>
            public override bool Equals(object obj)
            {
                if (!(obj is TypeRegistration typeRegistration) || typeRegistration.Type != Type)
                    return false;

                return string.Compare(Name, typeRegistration.Name, StringComparison.Ordinal) == 0;
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
            /// </returns>
            public override int GetHashCode() => _hashCode;
        }

        #endregion
    }
}