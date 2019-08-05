namespace Swan.DependencyInjection
{
    using JetBrains.Annotations;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// The concrete implementation of a simple IoC container
    /// based largely on TinyIoC (https://github.com/grumpydev/TinyIoC).
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public partial class DependencyContainer : IDisposable
    {
        private readonly object _autoRegisterLock = new object();

        private bool _disposed;

        static DependencyContainer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainer"/> class.
        /// </summary>
        public DependencyContainer()
        {
            RegisteredTypes = new TypesConcurrentDictionary(this);
            Register(this);
        }

        private DependencyContainer(DependencyContainer parent)
            : this()
        {
            Parent = parent;
        }

        /// <summary>
        /// Lazy created Singleton instance of the container for simple scenarios.
        /// </summary>
        public static DependencyContainer Current { get; } = new DependencyContainer();

        internal DependencyContainer Parent { get; }

        internal TypesConcurrentDictionary RegisteredTypes { get; }
        
        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            foreach (var disposable in RegisteredTypes.Values.Select(item => item as IDisposable))
            {
                disposable?.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets the child container.
        /// </summary>
        /// <returns>A new instance of the <see cref="DependencyContainer"/> class.</returns>
        public DependencyContainer GetChildContainer() => new DependencyContainer(this);

        #region Registration

        /// <summary>
        /// Attempt to automatically register all non-generic classes and interfaces in the current app domain.
        /// Types will only be registered if they pass the supplied registration predicate.
        /// </summary>
        /// <param name="duplicateAction">What action to take when encountering duplicate implementations of an interface/base class.</param>
        /// <param name="registrationPredicate">Predicate to determine if a particular type should be registered.</param>
        public void AutoRegister(
            DependencyContainerDuplicateImplementationAction duplicateAction =
                DependencyContainerDuplicateImplementationAction.RegisterSingle,
            Func<Type, bool> registrationPredicate = null)
        {
            AutoRegister(
                SwanRuntime.GetAssemblies().Where(a => !IsIgnoredAssembly(a)),
                duplicateAction,
                registrationPredicate);
        }

        /// <summary>
        /// Attempt to automatically register all non-generic classes and interfaces in the specified assemblies
        /// Types will only be registered if they pass the supplied registration predicate.
        /// </summary>
        /// <param name="assemblies">Assemblies to process.</param>
        /// <param name="duplicateAction">What action to take when encountering duplicate implementations of an interface/base class.</param>
        /// <param name="registrationPredicate">Predicate to determine if a particular type should be registered.</param>
        public void AutoRegister(
            IEnumerable<Assembly> assemblies,
            DependencyContainerDuplicateImplementationAction duplicateAction =
                DependencyContainerDuplicateImplementationAction.RegisterSingle,
            Func<Type, bool> registrationPredicate = null)
        {
            lock (_autoRegisterLock)
            {
                var types = assemblies
                    .SelectMany(a => a.GetAllTypes())
                    .Where(t => !IsIgnoredType(t, registrationPredicate))
                    .ToList();

                var concreteTypes = types
                    .Where(type =>
                        type.IsClass && !type.IsAbstract &&
                        (type != GetType() && (type.DeclaringType != GetType()) && !type.IsGenericTypeDefinition))
                    .ToList();

                foreach (var type in concreteTypes)
                {
                    try
                    {
                        RegisteredTypes.Register(type, string.Empty, GetDefaultObjectFactory(type, type));
                    }
                    catch (MethodAccessException)
                    {
                        // Ignore methods we can't access - added for Silverlight
                    }
                }

                var abstractInterfaceTypes = types.Where(
                    type =>
                        ((type.IsInterface || type.IsAbstract) && (type.DeclaringType != GetType()) &&
                         (!type.IsGenericTypeDefinition)));

                foreach (var type in abstractInterfaceTypes)
                {
                    var localType = type;
                    var implementations = concreteTypes
                        .Where(implementationType => localType.IsAssignableFrom(implementationType)).ToList();

                    if (implementations.Skip(1).Any())
                    {
                        if (duplicateAction == DependencyContainerDuplicateImplementationAction.Fail)
                            throw new DependencyContainerRegistrationException(type, implementations);

                        if (duplicateAction == DependencyContainerDuplicateImplementationAction.RegisterMultiple)
                        {
                            RegisterMultiple(type, implementations);
                        }
                    }

                    var firstImplementation = implementations.FirstOrDefault();

                    if (firstImplementation == null) continue;

                    try
                    {
                        RegisteredTypes.Register(type, string.Empty, GetDefaultObjectFactory(type, firstImplementation));
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
        /// <param name="registerType">Type to register.</param>
        /// <param name="name">Name of registration.</param>
        /// <returns>RegisterOptions for fluent API.</returns>
        public RegisterOptions Register(Type registerType, string name = "")
            => RegisteredTypes.Register(
                registerType,
                name,
                GetDefaultObjectFactory(registerType, registerType));

        /// <summary>
        /// Creates/replaces a named container class registration with a given implementation and default options.
        /// </summary>
        /// <param name="registerType">Type to register.</param>
        /// <param name="registerImplementation">Type to instantiate that implements RegisterType.</param>
        /// <param name="name">Name of registration.</param>
        /// <returns>RegisterOptions for fluent API.</returns>
        public RegisterOptions Register(Type registerType, Type registerImplementation, string name = "") =>
            RegisteredTypes.Register(registerType, name, GetDefaultObjectFactory(registerType, registerImplementation));

        /// <summary>
        /// Creates/replaces a named container class registration with a specific, strong referenced, instance.
        /// </summary>
        /// <param name="registerType">Type to register.</param>
        /// <param name="instance">Instance of RegisterType to register.</param>
        /// <param name="name">Name of registration.</param>
        /// <returns>RegisterOptions for fluent API.</returns>
        public RegisterOptions Register(Type registerType, object instance, string name = "") =>
            RegisteredTypes.Register(registerType, name, new InstanceFactory(registerType, registerType, instance));

        /// <summary>
        /// Creates/replaces a named container class registration with a specific, strong referenced, instance.
        /// </summary>
        /// <param name="registerType">Type to register.</param>
        /// <param name="registerImplementation">Type of instance to register that implements RegisterType.</param>
        /// <param name="instance">Instance of RegisterImplementation to register.</param>
        /// <param name="name">Name of registration.</param>
        /// <returns>RegisterOptions for fluent API.</returns>
        public RegisterOptions Register(
            Type registerType,
            Type registerImplementation,
            object instance,
            string name = "")
            => RegisteredTypes.Register(registerType, name, new InstanceFactory(registerType, registerImplementation, instance));

        /// <summary>
        /// Creates/replaces a container class registration with a user specified factory.
        /// </summary>
        /// <param name="registerType">Type to register.</param>
        /// <param name="factory">Factory/lambda that returns an instance of RegisterType.</param>
        /// <param name="name">Name of registration.</param>
        /// <returns>RegisterOptions for fluent API.</returns>
        public RegisterOptions Register(
            Type registerType,
            Func<DependencyContainer, Dictionary<string, object>, object> factory,
            string name = "")
            => RegisteredTypes.Register(registerType, name, new DelegateFactory(registerType, factory));

        /// <summary>
        /// Creates/replaces a named container class registration with default options.
        /// </summary>
        /// <typeparam name="TRegister">Type to register.</typeparam>
        /// <param name="name">Name of registration.</param>
        /// <returns>RegisterOptions for fluent API.</returns>
        public RegisterOptions Register<TRegister>(string name = "")
            where TRegister : class
        {
            return Register(typeof(TRegister), name);
        }

        /// <summary>
        /// Creates/replaces a named container class registration with a given implementation and default options.
        /// </summary>
        /// <typeparam name="TRegister">Type to register.</typeparam>
        /// <typeparam name="TRegisterImplementation">Type to instantiate that implements RegisterType.</typeparam>
        /// <param name="name">Name of registration.</param>
        /// <returns>RegisterOptions for fluent API.</returns>
        public RegisterOptions Register<TRegister, TRegisterImplementation>(string name = "")
            where TRegister : class
            where TRegisterImplementation : class, TRegister
        {
            return Register(typeof(TRegister), typeof(TRegisterImplementation), name);
        }

        /// <summary>
        /// Creates/replaces a named container class registration with a specific, strong referenced, instance.
        /// </summary>
        /// <typeparam name="TRegister">Type to register.</typeparam>
        /// <param name="instance">Instance of RegisterType to register.</param>
        /// <param name="name">Name of registration.</param>
        /// <returns>RegisterOptions for fluent API.</returns>
        public RegisterOptions Register<TRegister>(TRegister instance, string name = "")
            where TRegister : class
        {
            return Register(typeof(TRegister), instance, name);
        }

        /// <summary>
        /// Creates/replaces a named container class registration with a specific, strong referenced, instance.
        /// </summary>
        /// <typeparam name="TRegister">Type to register.</typeparam>
        /// <typeparam name="TRegisterImplementation">Type of instance to register that implements RegisterType.</typeparam>
        /// <param name="instance">Instance of RegisterImplementation to register.</param>
        /// <param name="name">Name of registration.</param>
        /// <returns>RegisterOptions for fluent API.</returns>
        public RegisterOptions Register<TRegister, TRegisterImplementation>(TRegisterImplementation instance,
            string name = "")
            where TRegister : class
            where TRegisterImplementation : class, TRegister
        {
            return Register(typeof(TRegister), typeof(TRegisterImplementation), instance, name);
        }

        /// <summary>
        /// Creates/replaces a named container class registration with a user specified factory.
        /// </summary>
        /// <typeparam name="TRegister">Type to register.</typeparam>
        /// <param name="factory">Factory/lambda that returns an instance of RegisterType.</param>
        /// <param name="name">Name of registration.</param>
        /// <returns>RegisterOptions for fluent API.</returns>
        public RegisterOptions Register<TRegister>(
            [NotNull] Func<DependencyContainer, Dictionary<string, object>, TRegister> factory, string name = "")
            where TRegister : class
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            return Register(typeof(TRegister), factory, name);
        }

        /// <summary>
        /// Register multiple implementations of a type.
        /// 
        /// Internally this registers each implementation using the full name of the class as its registration name.
        /// </summary>
        /// <typeparam name="TRegister">Type that each implementation implements.</typeparam>
        /// <param name="implementationTypes">Types that implement RegisterType.</param>
        /// <returns>MultiRegisterOptions for the fluent API.</returns>
        public MultiRegisterOptions RegisterMultiple<TRegister>(IEnumerable<Type> implementationTypes) =>
            RegisterMultiple(typeof(TRegister), implementationTypes);

        /// <summary>
        /// Register multiple implementations of a type.
        /// 
        /// Internally this registers each implementation using the full name of the class as its registration name.
        /// </summary>
        /// <param name="registrationType">Type that each implementation implements.</param>
        /// <param name="implementationTypes">Types that implement RegisterType.</param>
        /// <returns>MultiRegisterOptions for the fluent API.</returns>
        public MultiRegisterOptions RegisterMultiple([NotNull] Type registrationType, [NotNull] IEnumerable<Type> implementationTypes)
        {
            if (implementationTypes == null)
                throw new ArgumentNullException(nameof(implementationTypes), "types is null.");

            foreach (var type in implementationTypes.Where(type => !registrationType.IsAssignableFrom(type)))
            {
                throw new ArgumentException(
                    $"types: The type {registrationType.FullName} is not assignable from {type.FullName}");
            }

            if (implementationTypes.Count() != implementationTypes.Distinct().Count())
            {
                var queryForDuplicatedTypes = implementationTypes
                    .GroupBy(i => i)
                    .Where(j => j.Count() > 1)
                    .Select(j => j.Key.FullName);

                var fullNamesOfDuplicatedTypes = string.Join(",\n", queryForDuplicatedTypes.ToArray());

                throw new ArgumentException(
                    $"types: The same implementation type cannot be specified multiple times for {registrationType.FullName}\n\n{fullNamesOfDuplicatedTypes}");
            }

            var registerOptions = implementationTypes
                .Select(type => Register(registrationType, type, type.FullName))
                .ToList();

            return new MultiRegisterOptions(registerOptions);
        }

        #endregion

        #region Unregistration

        /// <summary>
        /// Remove a named container class registration.
        /// </summary>
        /// <typeparam name="TRegister">Type to unregister.</typeparam>
        /// <param name="name">Name of registration.</param>
        /// <returns><c>true</c> if the registration is successfully found and removed; otherwise, <c>false</c>.</returns>
        public bool Unregister<TRegister>(string name = "") => Unregister(typeof(TRegister), name);

        /// <summary>
        /// Remove a named container class registration.
        /// </summary>
        /// <param name="registerType">Type to unregister.</param>
        /// <param name="name">Name of registration.</param>
        /// <returns><c>true</c> if the registration is successfully found and removed; otherwise, <c>false</c>.</returns>
        public bool Unregister(Type registerType, string name = "") =>
            RegisteredTypes.RemoveRegistration(new TypeRegistration(registerType, name));

        #endregion

        #region Resolution

        /// <summary>
        /// Attempts to resolve a named type using specified options and the supplied constructor parameters.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// </summary>
        /// <param name="resolveType">Type to resolve.</param>
        /// <param name="name">Name of registration.</param>
        /// <param name="options">Resolution options.</param>
        /// <returns>Instance of type.</returns>
        /// <exception cref="DependencyContainerResolutionException">Unable to resolve the type.</exception>
        public object Resolve(
            Type resolveType, 
            string name = null, 
            DependencyContainerResolveOptions options = null)
            => RegisteredTypes.ResolveInternal(new TypeRegistration(resolveType, name), options ?? DependencyContainerResolveOptions.Default);

        /// <summary>
        /// Attempts to resolve a named type using specified options and the supplied constructor parameters.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// </summary>
        /// <typeparam name="TResolveType">Type to resolve.</typeparam>
        /// <param name="name">Name of registration.</param>
        /// <param name="options">Resolution options.</param>
        /// <returns>Instance of type.</returns>
        /// <exception cref="DependencyContainerResolutionException">Unable to resolve the type.</exception>
        public TResolveType Resolve<TResolveType>(
            string name = null,
            DependencyContainerResolveOptions options = null)
            where TResolveType : class
        {
            return (TResolveType)Resolve(typeof(TResolveType), name, options);
        }

        /// <summary>
        /// Attempts to predict whether a given type can be resolved with the supplied constructor parameters options.
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// Note: Resolution may still fail if user defined factory registrations fail to construct objects when called.
        /// </summary>
        /// <param name="resolveType">Type to resolve.</param>
        /// <param name="name">The name.</param>
        /// <param name="options">Resolution options.</param>
        /// <returns>
        /// Bool indicating whether the type can be resolved.
        /// </returns>
        public bool CanResolve(
            Type resolveType,
            string name = null,
            DependencyContainerResolveOptions options = null) =>
            RegisteredTypes.CanResolve(new TypeRegistration(resolveType, name), options);

        /// <summary>
        /// Attempts to predict whether a given named type can be resolved with the supplied constructor parameters options.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// 
        /// Note: Resolution may still fail if user defined factory registrations fail to construct objects when called.
        /// </summary>
        /// <typeparam name="TResolveType">Type to resolve.</typeparam>
        /// <param name="name">Name of registration.</param>
        /// <param name="options">Resolution options.</param>
        /// <returns>Bool indicating whether the type can be resolved.</returns>
        public bool CanResolve<TResolveType>(
            string name = null, 
            DependencyContainerResolveOptions options = null)
            where TResolveType : class
        {
            return CanResolve(typeof(TResolveType), name, options);
        }

        /// <summary>
        /// Attempts to resolve a type using the default options.
        /// </summary>
        /// <param name="resolveType">Type to resolve.</param>
        /// <param name="resolvedType">Resolved type or default if resolve fails.</param>
        /// <returns><c>true</c> if resolved successfully, <c>false</c> otherwise.</returns>
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
        /// Attempts to resolve a type using the given options.
        /// </summary>
        /// <param name="resolveType">Type to resolve.</param>
        /// <param name="options">Resolution options.</param>
        /// <param name="resolvedType">Resolved type or default if resolve fails.</param>
        /// <returns><c>true</c> if resolved successfully, <c>false</c> otherwise.</returns>
        public bool TryResolve(Type resolveType, DependencyContainerResolveOptions options, out object resolvedType)
        {
            try
            {
                resolvedType = Resolve(resolveType, options: options);
                return true;
            }
            catch (DependencyContainerResolutionException)
            {
                resolvedType = null;
                return false;
            }
        }

        /// <summary>
        /// Attempts to resolve a type using the default options and given name.
        /// </summary>
        /// <param name="resolveType">Type to resolve.</param>
        /// <param name="name">Name of registration.</param>
        /// <param name="resolvedType">Resolved type or default if resolve fails.</param>
        /// <returns><c>true</c> if resolved successfully, <c>false</c> otherwise.</returns>
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
        /// Attempts to resolve a type using the given options and name.
        /// </summary>
        /// <param name="resolveType">Type to resolve.</param>
        /// <param name="name">Name of registration.</param>
        /// <param name="options">Resolution options.</param>
        /// <param name="resolvedType">Resolved type or default if resolve fails.</param>
        /// <returns><c>true</c> if resolved successfully, <c>false</c> otherwise.</returns>
        public bool TryResolve(
            Type resolveType, 
            string name, 
            DependencyContainerResolveOptions options,
            out object resolvedType)
        {
            try
            {
                resolvedType = Resolve(resolveType, name, options);
                return true;
            }
            catch (DependencyContainerResolutionException)
            {
                resolvedType = null;
                return false;
            }
        }
        
        /// <summary>
        /// Attempts to resolve a type using the default options.
        /// </summary>
        /// <typeparam name="TResolveType">Type to resolve.</typeparam>
        /// <param name="resolvedType">Resolved type or default if resolve fails.</param>
        /// <returns><c>true</c> if resolved successfully, <c>false</c> otherwise.</returns>
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
                resolvedType = default;
                return false;
            }
        }

        /// <summary>
        /// Attempts to resolve a type using the given options.
        /// </summary>
        /// <typeparam name="TResolveType">Type to resolve.</typeparam>
        /// <param name="options">Resolution options.</param>
        /// <param name="resolvedType">Resolved type or default if resolve fails.</param>
        /// <returns><c>true</c> if resolved successfully, <c>false</c> otherwise.</returns>
        public bool TryResolve<TResolveType>(DependencyContainerResolveOptions options, out TResolveType resolvedType)
            where TResolveType : class
        {
            try
            {
                resolvedType = Resolve<TResolveType>(options: options);
                return true;
            }
            catch (DependencyContainerResolutionException)
            {
                resolvedType = default;
                return false;
            }
        }

        /// <summary>
        /// Attempts to resolve a type using the default options and given name.
        /// </summary>
        /// <typeparam name="TResolveType">Type to resolve.</typeparam>
        /// <param name="name">Name of registration.</param>
        /// <param name="resolvedType">Resolved type or default if resolve fails.</param>
        /// <returns><c>true</c> if resolved successfully, <c>false</c> otherwise.</returns>
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
                resolvedType = default;
                return false;
            }
        }

        /// <summary>
        /// Attempts to resolve a type using the given options and name.
        /// </summary>
        /// <typeparam name="TResolveType">Type to resolve.</typeparam>
        /// <param name="name">Name of registration.</param>
        /// <param name="options">Resolution options.</param>
        /// <param name="resolvedType">Resolved type or default if resolve fails.</param>
        /// <returns><c>true</c> if resolved successfully, <c>false</c> otherwise.</returns>
        public bool TryResolve<TResolveType>(
            string name,
            DependencyContainerResolveOptions options,
            out TResolveType resolvedType)
            where TResolveType : class
        {
            try
            {
                resolvedType = Resolve<TResolveType>(name, options);
                return true;
            }
            catch (DependencyContainerResolutionException)
            {
                resolvedType = default;
                return false;
            }
        }
        
        /// <summary>
        /// Returns all registrations of a type.
        /// </summary>
        /// <param name="resolveType">Type to resolveAll.</param>
        /// <param name="includeUnnamed">Whether to include un-named (default) registrations.</param>
        /// <returns>IEnumerable.</returns>
        public IEnumerable<object> ResolveAll(Type resolveType, bool includeUnnamed = false) 
            => RegisteredTypes.Resolve(resolveType, includeUnnamed);

        /// <summary>
        /// Returns all registrations of a type.
        /// </summary>
        /// <typeparam name="TResolveType">Type to resolveAll.</typeparam>
        /// <param name="includeUnnamed">Whether to include un-named (default) registrations.</param>
        /// <returns>IEnumerable.</returns>
        public IEnumerable<TResolveType> ResolveAll<TResolveType>(bool includeUnnamed = true)
            where TResolveType : class
        {
            return ResolveAll(typeof(TResolveType), includeUnnamed).Cast<TResolveType>();
        }

        /// <summary>
        /// Attempts to resolve all public property dependencies on the given object using the given resolve options.
        /// </summary>
        /// <param name="input">Object to "build up".</param>
        /// <param name="resolveOptions">Resolve options to use.</param>
        public void BuildUp(object input, DependencyContainerResolveOptions resolveOptions = null)
        {
            if (resolveOptions == null)
                resolveOptions = DependencyContainerResolveOptions.Default;

            var properties = input.GetType()
                .GetProperties()
                .Where(property => property.GetCacheGetMethod() != null && property.GetCacheSetMethod() != null &&
                                   !property.PropertyType.IsValueType);

            foreach (var property in properties.Where(property => property.GetValue(input, null) == null))
            {
                try
                {
                    property.SetValue(
                        input,
                        RegisteredTypes.ResolveInternal(new TypeRegistration(property.PropertyType), resolveOptions),
                        null);
                }
                catch (DependencyContainerResolutionException)
                {
                    // Catch any resolution errors and ignore them
                }
            }
        }

        #endregion

        #region Internal Methods
        
        internal static bool IsValidAssignment(Type registerType, Type registerImplementation)
        {
            if (!registerType.IsGenericTypeDefinition)
            {
                if (!registerType.IsAssignableFrom(registerImplementation))
                    return false;
            }
            else
            {
                if (registerType.IsInterface && registerImplementation.GetInterfaces().All(t => t.Name != registerType.Name))
                    return false;

                if (registerType.IsAbstract && registerImplementation.BaseType != registerType)
                    return false;
            }

            return true;
        }

        private static bool IsIgnoredAssembly(Assembly assembly)
        {
            // TODO - find a better way to remove "system" assemblies from the auto registration
            var ignoreChecks = new List<Func<Assembly, bool>>
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
                t => t.FullName?.StartsWith("System.", StringComparison.Ordinal) ?? false,
                t => t.FullName?.StartsWith("Microsoft.", StringComparison.Ordinal) ?? false,
                t => t.IsPrimitive,
                t => t.IsGenericTypeDefinition,
                t => (t.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Length == 0) &&
                     !(t.IsInterface || t.IsAbstract),
            };

            if (registrationPredicate != null)
            {
                ignoreChecks.Add(t => !registrationPredicate(t));
            }

            return ignoreChecks.Any(check => check(type));
        }

        private static ObjectFactoryBase GetDefaultObjectFactory(Type registerType, Type registerImplementation) => registerType.IsInterface || registerType.IsAbstract
            ? (ObjectFactoryBase)new SingletonFactory(registerType, registerImplementation)
            : new MultiInstanceFactory(registerType, registerImplementation);

        #endregion
    }
}