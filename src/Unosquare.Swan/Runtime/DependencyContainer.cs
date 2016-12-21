//===============================================================================
// TinyIoC
//
// An easy to use, hassle free, Inversion of Control Container for small projects
// and beginners alike.
//
// https://github.com/grumpydev/TinyIoC
//===============================================================================
// Copyright © Steven Robbins.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

namespace Unosquare.Swan.Runtime
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

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

    #region Exception Types

    /// <summary>
    /// An exception for dependency resolutions
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class DependencyContainerResolutionException : Exception
    {
        private const string ErrorText = "Unable to resolve type: {0}";

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerResolutionException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public DependencyContainerResolutionException(Type type)
            : base(String.Format(ErrorText, type.FullName))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerResolutionException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="innerException">The inner exception.</param>
        public DependencyContainerResolutionException(Type type, Exception innerException)
            : base(String.Format(ErrorText, type.FullName), innerException)
        {
        }
    }

    /// <summary>
    /// Registration Type Exception
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class DependencyContainerRegistrationTypeException : Exception
    {
        private const string RegisterErrorText = "Cannot register type {0} - abstract classes or interfaces are not valid implementation types for {1}.";

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerRegistrationTypeException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="factory">The factory.</param>
        public DependencyContainerRegistrationTypeException(Type type, string factory)
            : base(String.Format(RegisterErrorText, type.FullName, factory))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerRegistrationTypeException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="factory">The factory.</param>
        /// <param name="innerException">The inner exception.</param>
        public DependencyContainerRegistrationTypeException(Type type, string factory, Exception innerException)
            : base(String.Format(RegisterErrorText, type.FullName, factory), innerException)
        {
        }
    }

    /// <summary>
    /// Generic Constraint Registration Exception
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class DependencyContainerRegistrationException : Exception
    {
        private const string ConvertErrorText = "Cannot convert current registration of {0} to {1}";
        private const string GenericConstraintErrorText = "Type {1} is not valid for a registration of type {0}";

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerRegistrationException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="method">The method.</param>
        public DependencyContainerRegistrationException(Type type, string method)
            : base(String.Format(ConvertErrorText, type.FullName, method))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerRegistrationException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="method">The method.</param>
        /// <param name="innerException">The inner exception.</param>
        public DependencyContainerRegistrationException(Type type, string method, Exception innerException)
            : base(String.Format(ConvertErrorText, type.FullName, method), innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerRegistrationException"/> class.
        /// </summary>
        /// <param name="registerType">Type of the register.</param>
        /// <param name="implementationType">Type of the implementation.</param>
        public DependencyContainerRegistrationException(Type registerType, Type implementationType)
            : base(String.Format(GenericConstraintErrorText, registerType.FullName, implementationType.FullName))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerRegistrationException"/> class.
        /// </summary>
        /// <param name="registerType">Type of the register.</param>
        /// <param name="implementationType">Type of the implementation.</param>
        /// <param name="innerException">The inner exception.</param>
        public DependencyContainerRegistrationException(Type registerType, Type implementationType, Exception innerException)
            : base(String.Format(GenericConstraintErrorText, registerType.FullName, implementationType.FullName), innerException)
        {
        }
    }

    /// <summary>
    /// Weak Reference Exception
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class DependencyContainerWeakReferenceException : Exception
    {
        private const string ErrorText = "Unable to instantiate {0} - referenced object has been reclaimed";

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerWeakReferenceException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public DependencyContainerWeakReferenceException(Type type)
            : base(String.Format(ErrorText, type.FullName))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerWeakReferenceException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="innerException">The inner exception.</param>
        public DependencyContainerWeakReferenceException(Type type, Exception innerException)
            : base(String.Format(ErrorText, type.FullName), innerException)
        {
        }
    }

    /// <summary>
    /// Constructor Resolution Exception
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class DependencyContainerConstructorResolutionException : Exception
    {
        private const string ErrorText = "Unable to resolve constructor for {0} using provided Expression.";

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerConstructorResolutionException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public DependencyContainerConstructorResolutionException(Type type)
            : base(String.Format(ErrorText, type.FullName))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerConstructorResolutionException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="innerException">The inner exception.</param>
        public DependencyContainerConstructorResolutionException(Type type, Exception innerException)
            : base(String.Format(ErrorText, type.FullName), innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerConstructorResolutionException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public DependencyContainerConstructorResolutionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerConstructorResolutionException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DependencyContainerConstructorResolutionException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Auto-registration Exception
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class DependencyContainerAutoRegistrationException : Exception
    {
        private const string ErrorText = "Duplicate implementation of type {0} found ({1}).";

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerAutoRegistrationException"/> class.
        /// </summary>
        /// <param name="registerType">Type of the register.</param>
        /// <param name="types">The types.</param>
        public DependencyContainerAutoRegistrationException(Type registerType, IEnumerable<Type> types)
            : base(String.Format(ErrorText, registerType, GetTypesString(types)))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerAutoRegistrationException"/> class.
        /// </summary>
        /// <param name="registerType">Type of the register.</param>
        /// <param name="types">The types.</param>
        /// <param name="innerException">The inner exception.</param>
        public DependencyContainerAutoRegistrationException(Type registerType, IEnumerable<Type> types, Exception innerException)
            : base(String.Format(ErrorText, registerType, GetTypesString(types)), innerException)
        {
        }

        /// <summary>
        /// Gets the types string.
        /// </summary>
        /// <param name="types">The types.</param>
        /// <returns></returns>
        private static string GetTypesString(IEnumerable<Type> types)
        {
            var typeNames = from type in types
                            select type.FullName;

            return string.Join(",", typeNames.ToArray());
        }
    }

    #endregion

    #region SupportClasses

    /// <summary>
    /// Define overload on named parameters
    /// </summary>
    public sealed class DependencyContainerNamedParameterOverloads : Dictionary<string, object>
    {
        /// <summary>
        /// Creates a new instance from a Dictionary
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
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
            private readonly DependencyContainer _Container;
            private readonly TypeRegistration _Registration;

            /// <summary>
            /// Initializes a new instance of the <see cref="RegisterOptions"/> class.
            /// </summary>
            /// <param name="container">The container.</param>
            /// <param name="registration">The registration.</param>
            public RegisterOptions(DependencyContainer container, TypeRegistration registration)
            {
                _Container = container;
                _Registration = registration;
            }

            /// <summary>
            /// Make registration a singleton (single instance) if possible
            /// </summary>
            /// <returns>RegisterOptions</returns>
            /// <exception cref="DependencyContainerRegistrationException"></exception>
            public RegisterOptions AsSingleton()
            {
                var currentFactory = _Container.GetCurrentFactory(_Registration);

                if (currentFactory == null)
                    throw new DependencyContainerRegistrationException(_Registration.Type, "singleton");

                return _Container.AddUpdateRegistration(_Registration, currentFactory.SingletonVariant);
            }

            /// <summary>
            /// Make registration multi-instance if possible
            /// </summary>
            /// <returns>RegisterOptions</returns>
            /// <exception cref="DependencyContainerRegistrationException"></exception>
            public RegisterOptions AsMultiInstance()
            {
                var currentFactory = _Container.GetCurrentFactory(_Registration);

                if (currentFactory == null)
                    throw new DependencyContainerRegistrationException(_Registration.Type, "multi-instance");

                return _Container.AddUpdateRegistration(_Registration, currentFactory.MultiInstanceVariant);
            }

            /// <summary>
            /// Make registration hold a weak reference if possible
            /// </summary>
            /// <returns>RegisterOptions</returns>
            /// <exception cref="DependencyContainerRegistrationException"></exception>
            public RegisterOptions WithWeakReference()
            {
                var currentFactory = _Container.GetCurrentFactory(_Registration);

                if (currentFactory == null)
                    throw new DependencyContainerRegistrationException(_Registration.Type, "weak reference");

                return _Container.AddUpdateRegistration(_Registration, currentFactory.WeakReferenceVariant);
            }

            /// <summary>
            /// Make registration hold a strong reference if possible
            /// </summary>
            /// <returns>RegisterOptions</returns>
            /// <exception cref="DependencyContainerRegistrationException"></exception>
            public RegisterOptions WithStrongReference()
            {
                var currentFactory = _Container.GetCurrentFactory(_Registration);

                if (currentFactory == null)
                    throw new DependencyContainerRegistrationException(_Registration.Type, "strong reference");

                return _Container.AddUpdateRegistration(_Registration, currentFactory.StrongReferenceVariant);
            }


            /// <summary>
            /// Sets the constructor to use
            /// </summary>
            /// <typeparam name="RegisterType">The type of the egister type.</typeparam>
            /// <param name="constructor">The constructor.</param>
            /// <returns></returns>
            /// <exception cref="Unosquare.Swan.Runtime.DependencyContainerConstructorResolutionException">
            /// </exception>
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

                var currentFactory = _Container.GetCurrentFactory(_Registration);
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

                var currentFactory = instance._Container.GetCurrentFactory(instance._Registration);

                if (currentFactory == null)
                    throw new DependencyContainerRegistrationException(instance._Registration.Type, errorString);

                return instance._Container.AddUpdateRegistration(instance._Registration, currentFactory.GetCustomObjectLifetimeVariant(lifetimeProvider, errorString));
            }
        }

        /// <summary>
        /// Registration options for "fluent" API when registering multiple implementations
        /// </summary>
        public sealed class MultiRegisterOptions
        {
            private IEnumerable<RegisterOptions> _RegisterOptions;

            /// <summary>
            /// Initializes a new instance of the MultiRegisterOptions class.
            /// </summary>
            /// <param name="registerOptions">Registration options</param>
            public MultiRegisterOptions(IEnumerable<RegisterOptions> registerOptions)
            {
                _RegisterOptions = registerOptions;
            }

            /// <summary>
            /// Make registration a singleton (single instance) if possible
            /// </summary>
            /// <returns>RegisterOptions</returns>
            /// <exception cref="DependencyContainerRegistrationException"></exception>
            public MultiRegisterOptions AsSingleton()
            {
                _RegisterOptions = ExecuteOnAllRegisterOptions(ro => ro.AsSingleton());
                return this;
            }

            /// <summary>
            /// Make registration multi-instance if possible
            /// </summary>
            /// <returns>MultiRegisterOptions</returns>
            /// <exception cref="DependencyContainerRegistrationException"></exception>
            public MultiRegisterOptions AsMultiInstance()
            {
                _RegisterOptions = ExecuteOnAllRegisterOptions(ro => ro.AsMultiInstance());
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

                instance._RegisterOptions = instance.ExecuteOnAllRegisterOptions(ro => RegisterOptions.ToCustomLifetimeManager(ro, lifetimeProvider, errorString));

                return instance;
            }

            private IEnumerable<RegisterOptions> ExecuteOnAllRegisterOptions(Func<RegisterOptions, RegisterOptions> action)
            {
                var newRegisterOptions = new List<RegisterOptions>();

                foreach (var registerOption in _RegisterOptions)
                {
                    newRegisterOptions.Add(action(registerOption));
                }

                return newRegisterOptions;
            }
        }
        #endregion

        #region Public API

        #region Child Containers

        /// <summary>
        /// Gets the child container.
        /// </summary>
        /// <returns></returns>
        public DependencyContainer GetChildContainer()
        {
            return new DependencyContainer(this);
        }

        #endregion

        #region Registration
        /// <summary>
        /// Attempt to automatically register all non-generic classes and interfaces in the current app domain.
        /// 
        /// If more than one class implements an interface then only one implementation will be registered
        /// although no error will be thrown.
        /// </summary>
        public void AutoRegister()
        {
            AutoRegisterInternal(AppDomain.CurrentDomain.GetAssemblies().Where(a => !IsIgnoredAssembly(a)), DependencyContainerDuplicateImplementationActions.RegisterSingle, null);
        }

        /// <summary>
        /// Attempt to automatically register all non-generic classes and interfaces in the current app domain.
        /// Types will only be registered if they pass the supplied registration predicate.
        /// 
        /// If more than one class implements an interface then only one implementation will be registered
        /// although no error will be thrown.
        /// </summary>
        /// <param name="registrationPredicate">Predicate to determine if a particular type should be registered</param>
        public void AutoRegister(Func<Type, bool> registrationPredicate)
        {
            AutoRegisterInternal(AppDomain.CurrentDomain.GetAssemblies().Where(a => !IsIgnoredAssembly(a)), DependencyContainerDuplicateImplementationActions.RegisterSingle, registrationPredicate);
        }

        /// <summary>
        /// Attempt to automatically register all non-generic classes and interfaces in the current app domain.
        /// </summary>
        /// <param name="duplicateAction">What action to take when encountering duplicate implementations of an interface/base class.</param>
        /// <exception cref="DependencyContainerAutoRegistrationException"/>
        public void AutoRegister(DependencyContainerDuplicateImplementationActions duplicateAction)
        {
            AutoRegisterInternal(AppDomain.CurrentDomain.GetAssemblies().Where(a => !IsIgnoredAssembly(a)), duplicateAction, null);
        }

        /// <summary>
        /// Attempt to automatically register all non-generic classes and interfaces in the current app domain.
        /// Types will only be registered if they pass the supplied registration predicate.
        /// </summary>
        /// <param name="duplicateAction">What action to take when encountering duplicate implementations of an interface/base class.</param>
        /// <param name="registrationPredicate">Predicate to determine if a particular type should be registered</param>
        /// <exception cref="DependencyContainerAutoRegistrationException"/>
        public void AutoRegister(DependencyContainerDuplicateImplementationActions duplicateAction, Func<Type, bool> registrationPredicate)
        {
            AutoRegisterInternal(AppDomain.CurrentDomain.GetAssemblies().Where(a => !IsIgnoredAssembly(a)), duplicateAction, registrationPredicate);
        }

        /// <summary>
        /// Attempt to automatically register all non-generic classes and interfaces in the specified assemblies
        /// 
        /// If more than one class implements an interface then only one implementation will be registered
        /// although no error will be thrown.
        /// </summary>
        /// <param name="assemblies">Assemblies to process</param>
        public void AutoRegister(IEnumerable<Assembly> assemblies)
        {
            AutoRegisterInternal(assemblies, DependencyContainerDuplicateImplementationActions.RegisterSingle, null);
        }

        /// <summary>
        /// Attempt to automatically register all non-generic classes and interfaces in the specified assemblies
        /// Types will only be registered if they pass the supplied registration predicate.
        /// 
        /// If more than one class implements an interface then only one implementation will be registered
        /// although no error will be thrown.
        /// </summary>
        /// <param name="assemblies">Assemblies to process</param>
        /// <param name="registrationPredicate">Predicate to determine if a particular type should be registered</param>
        public void AutoRegister(IEnumerable<Assembly> assemblies, Func<Type, bool> registrationPredicate)
        {
            AutoRegisterInternal(assemblies, DependencyContainerDuplicateImplementationActions.RegisterSingle, registrationPredicate);
        }

        /// <summary>
        /// Attempt to automatically register all non-generic classes and interfaces in the specified assemblies
        /// </summary>
        /// <param name="assemblies">Assemblies to process</param>
        /// <param name="duplicateAction">What action to take when encountering duplicate implementations of an interface/base class.</param>
        /// <exception cref="DependencyContainerAutoRegistrationException"/>
        public void AutoRegister(IEnumerable<Assembly> assemblies, DependencyContainerDuplicateImplementationActions duplicateAction)
        {
            AutoRegisterInternal(assemblies, duplicateAction, null);
        }

        /// <summary>
        /// Attempt to automatically register all non-generic classes and interfaces in the specified assemblies
        /// Types will only be registered if they pass the supplied registration predicate.
        /// </summary>
        /// <param name="assemblies">Assemblies to process</param>
        /// <param name="duplicateAction">What action to take when encountering duplicate implementations of an interface/base class.</param>
        /// <param name="registrationPredicate">Predicate to determine if a particular type should be registered</param>
        /// <exception cref="DependencyContainerAutoRegistrationException"/>
        public void AutoRegister(IEnumerable<Assembly> assemblies, DependencyContainerDuplicateImplementationActions duplicateAction, Func<Type, bool> registrationPredicate)
        {
            AutoRegisterInternal(assemblies, duplicateAction, registrationPredicate);
        }

        /// <summary>
        /// Creates/replaces a container class registration with default options.
        /// </summary>
        /// <param name="registerType">Type to register</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register(Type registerType)
        {
            return RegisterInternal(registerType, string.Empty, GetDefaultObjectFactory(registerType, registerType));
        }

        /// <summary>
        /// Creates/replaces a named container class registration with default options.
        /// </summary>
        /// <param name="registerType">Type to register</param>
        /// <param name="name">Name of registration</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register(Type registerType, string name)
        {
            return RegisterInternal(registerType, name, GetDefaultObjectFactory(registerType, registerType));

        }

        /// <summary>
        /// Creates/replaces a container class registration with a given implementation and default options.
        /// </summary>
        /// <param name="registerType">Type to register</param>
        /// <param name="registerImplementation">Type to instantiate that implements RegisterType</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register(Type registerType, Type registerImplementation)
        {
            return RegisterInternal(registerType, string.Empty, GetDefaultObjectFactory(registerType, registerImplementation));
        }

        /// <summary>
        /// Creates/replaces a named container class registration with a given implementation and default options.
        /// </summary>
        /// <param name="registerType">Type to register</param>
        /// <param name="registerImplementation">Type to instantiate that implements RegisterType</param>
        /// <param name="name">Name of registration</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register(Type registerType, Type registerImplementation, string name)
        {
            return RegisterInternal(registerType, name, GetDefaultObjectFactory(registerType, registerImplementation));
        }

        /// <summary>
        /// Creates/replaces a container class registration with a specific, strong referenced, instance.
        /// </summary>
        /// <param name="registerType">Type to register</param>
        /// <param name="instance">Instance of RegisterType to register</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register(Type registerType, object instance)
        {
            return RegisterInternal(registerType, string.Empty, new InstanceFactory(registerType, registerType, instance));
        }

        /// <summary>
        /// Creates/replaces a named container class registration with a specific, strong referenced, instance.
        /// </summary>
        /// <param name="registerType">Type to register</param>
        /// <param name="instance">Instance of RegisterType to register</param>
        /// <param name="name">Name of registration</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register(Type registerType, object instance, string name)
        {
            return RegisterInternal(registerType, name, new InstanceFactory(registerType, registerType, instance));
        }

        /// <summary>
        /// Creates/replaces a container class registration with a specific, strong referenced, instance.
        /// </summary>
        /// <param name="registerType">Type to register</param>
        /// <param name="registerImplementation">Type of instance to register that implements RegisterType</param>
        /// <param name="instance">Instance of RegisterImplementation to register</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register(Type registerType, Type registerImplementation, object instance)
        {
            return RegisterInternal(registerType, string.Empty, new InstanceFactory(registerType, registerImplementation, instance));
        }

        /// <summary>
        /// Creates/replaces a named container class registration with a specific, strong referenced, instance.
        /// </summary>
        /// <param name="registerType">Type to register</param>
        /// <param name="registerImplementation">Type of instance to register that implements RegisterType</param>
        /// <param name="instance">Instance of RegisterImplementation to register</param>
        /// <param name="name">Name of registration</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register(Type registerType, Type registerImplementation, object instance, string name)
        {
            return RegisterInternal(registerType, name, new InstanceFactory(registerType, registerImplementation, instance));
        }

        /// <summary>
        /// Creates/replaces a container class registration with a user specified factory
        /// </summary>
        /// <param name="registerType">Type to register</param>
        /// <param name="factory">Factory/lambda that returns an instance of RegisterType</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register(Type registerType, Func<DependencyContainer, DependencyContainerNamedParameterOverloads, object> factory)
        {
            return RegisterInternal(registerType, string.Empty, new DelegateFactory(registerType, factory));
        }

        /// <summary>
        /// Creates/replaces a container class registration with a user specified factory
        /// </summary>
        /// <param name="registerType">Type to register</param>
        /// <param name="factory">Factory/lambda that returns an instance of RegisterType</param>
        /// <param name="name">Name of registation</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register(Type registerType, Func<DependencyContainer, DependencyContainerNamedParameterOverloads, object> factory, string name)
        {
            return RegisterInternal(registerType, name, new DelegateFactory(registerType, factory));
        }

        /// <summary>
        /// Creates/replaces a container class registration with default options.
        /// </summary>
        /// <typeparam name="RegisterType">Type to register</typeparam>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register<RegisterType>()
            where RegisterType : class
        {
            return Register(typeof(RegisterType));
        }

        /// <summary>
        /// Creates/replaces a named container class registration with default options.
        /// </summary>
        /// <typeparam name="RegisterType">Type to register</typeparam>
        /// <param name="name">Name of registration</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register<RegisterType>(string name)
            where RegisterType : class
        {
            return Register(typeof(RegisterType), name);
        }

        /// <summary>
        /// Creates/replaces a container class registration with a given implementation and default options.
        /// </summary>
        /// <typeparam name="RegisterType">Type to register</typeparam>
        /// <typeparam name="RegisterImplementation">Type to instantiate that implements RegisterType</typeparam>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register<RegisterType, RegisterImplementation>()
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            return Register(typeof(RegisterType), typeof(RegisterImplementation));
        }

        /// <summary>
        /// Creates/replaces a named container class registration with a given implementation and default options.
        /// </summary>
        /// <typeparam name="RegisterType">Type to register</typeparam>
        /// <typeparam name="RegisterImplementation">Type to instantiate that implements RegisterType</typeparam>
        /// <param name="name">Name of registration</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register<RegisterType, RegisterImplementation>(string name)
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            return Register(typeof(RegisterType), typeof(RegisterImplementation), name);
        }

        /// <summary>
        /// Creates/replaces a container class registration with a specific, strong referenced, instance.
        /// </summary>
        /// <typeparam name="RegisterType">Type to register</typeparam>
        /// <param name="instance">Instance of RegisterType to register</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register<RegisterType>(RegisterType instance)
           where RegisterType : class
        {
            return Register(typeof(RegisterType), instance);
        }

        /// <summary>
        /// Creates/replaces a named container class registration with a specific, strong referenced, instance.
        /// </summary>
        /// <typeparam name="RegisterType">Type to register</typeparam>
        /// <param name="instance">Instance of RegisterType to register</param>
        /// <param name="name">Name of registration</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register<RegisterType>(RegisterType instance, string name)
            where RegisterType : class
        {
            return Register(typeof(RegisterType), instance, name);
        }

        /// <summary>
        /// Creates/replaces a container class registration with a specific, strong referenced, instance.
        /// </summary>
        /// <typeparam name="RegisterType">Type to register</typeparam>
        /// <typeparam name="RegisterImplementation">Type of instance to register that implements RegisterType</typeparam>
        /// <param name="instance">Instance of RegisterImplementation to register</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register<RegisterType, RegisterImplementation>(RegisterImplementation instance)
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            return Register(typeof(RegisterType), typeof(RegisterImplementation), instance);
        }

        /// <summary>
        /// Creates/replaces a named container class registration with a specific, strong referenced, instance.
        /// </summary>
        /// <typeparam name="RegisterType">Type to register</typeparam>
        /// <typeparam name="RegisterImplementation">Type of instance to register that implements RegisterType</typeparam>
        /// <param name="instance">Instance of RegisterImplementation to register</param>
        /// <param name="name">Name of registration</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register<RegisterType, RegisterImplementation>(RegisterImplementation instance, string name)
            where RegisterType : class
            where RegisterImplementation : class, RegisterType
        {
            return Register(typeof(RegisterType), typeof(RegisterImplementation), instance, name);
        }

        /// <summary>
        /// Creates/replaces a container class registration with a user specified factory
        /// </summary>
        /// <typeparam name="RegisterType">Type to register</typeparam>
        /// <param name="factory">Factory/lambda that returns an instance of RegisterType</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register<RegisterType>(Func<DependencyContainer, DependencyContainerNamedParameterOverloads, RegisterType> factory)
            where RegisterType : class
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            return Register(typeof(RegisterType), factory);
        }

        /// <summary>
        /// Creates/replaces a named container class registration with a user specified factory
        /// </summary>
        /// <typeparam name="RegisterType">Type to register</typeparam>
        /// <param name="factory">Factory/lambda that returns an instance of RegisterType</param>
        /// <param name="name">Name of registation</param>
        /// <returns>RegisterOptions for fluent API</returns>
        public RegisterOptions Register<RegisterType>(Func<DependencyContainer, DependencyContainerNamedParameterOverloads, RegisterType> factory, string name)
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
                if (!registrationType.GetTypeInfo().IsAssignableFrom(type))
                    throw new ArgumentException(String.Format("types: The type {0} is not assignable from {1}", registrationType.FullName, type.FullName));

            if (implementationTypes.Count() != implementationTypes.Distinct().Count())
            {
                var queryForDuplicatedTypes = from i in implementationTypes
                                              group i by i
                                                  into j
                                              where j.Count() > 1
                                              select j.Key.FullName;

                var fullNamesOfDuplicatedTypes = string.Join(",\n", queryForDuplicatedTypes.ToArray());
                var multipleRegMessage = string.Format("types: The same implementation type cannot be specified multiple times for {0}\n\n{1}", registrationType.FullName, fullNamesOfDuplicatedTypes);
                throw new ArgumentException(multipleRegMessage);
            }

            var registerOptions = new List<RegisterOptions>();

            foreach (var type in implementationTypes)
            {
                registerOptions.Add(Register(registrationType, type, type.FullName));
            }

            return new MultiRegisterOptions(registerOptions);
        }
        #endregion

        #region Unregistration

        /// <summary>
        /// Remove a container class registration.
        /// </summary>
        /// <typeparam name="RegisterType">Type to unregister</typeparam>
        /// <returns>true if the registration is successfully found and removed; otherwise, false.</returns>
        public bool Unregister<RegisterType>()
        {
            return Unregister(typeof(RegisterType), string.Empty);
        }

        /// <summary>
        /// Remove a named container class registration.
        /// </summary>
        /// <typeparam name="RegisterType">Type to unregister</typeparam>
        /// <param name="name">Name of registration</param>
        /// <returns>true if the registration is successfully found and removed; otherwise, false.</returns>
        public bool Unregister<RegisterType>(string name)
        {
            return Unregister(typeof(RegisterType), name);
        }

        /// <summary>
        /// Remove a container class registration.
        /// </summary>
        /// <param name="registerType">Type to unregister</param>
        /// <returns>true if the registration is successfully found and removed; otherwise, false.</returns>
        public bool Unregister(Type registerType)
        {
            return Unregister(registerType, string.Empty);
        }

        /// <summary>
        /// Remove a named container class registration.
        /// </summary>
        /// <param name="registerType">Type to unregister</param>
        /// <param name="name">Name of registration</param>
        /// <returns>true if the registration is successfully found and removed; otherwise, false.</returns>
        public bool Unregister(Type registerType, string name)
        {
            var typeRegistration = new TypeRegistration(registerType, name);

            return RemoveRegistration(typeRegistration);
        }

        #endregion

        #region Resolution
        /// <summary>
        /// Attempts to resolve a type using default options.
        /// </summary>
        /// <param name="resolveType">Type to resolve</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="DependencyContainerResolutionException">Unable to resolve the type.</exception>
        public object Resolve(Type resolveType)
        {
            return ResolveInternal(new TypeRegistration(resolveType), DependencyContainerNamedParameterOverloads.Default, DependencyContainerResolveOptions.Default);
        }

        /// <summary>
        /// Attempts to resolve a type using specified options.
        /// </summary>
        /// <param name="resolveType">Type to resolve</param>
        /// <param name="options">Resolution options</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="DependencyContainerResolutionException">Unable to resolve the type.</exception>
        public object Resolve(Type resolveType, DependencyContainerResolveOptions options)
        {
            return ResolveInternal(new TypeRegistration(resolveType), DependencyContainerNamedParameterOverloads.Default, options);
        }

        /// <summary>
        /// Attempts to resolve a type using default options and the supplied name.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// </summary>
        /// <param name="resolveType">Type to resolve</param>
        /// <param name="name">Name of registration</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="DependencyContainerResolutionException">Unable to resolve the type.</exception>
        public object Resolve(Type resolveType, string name)
        {
            return ResolveInternal(new TypeRegistration(resolveType, name), DependencyContainerNamedParameterOverloads.Default, DependencyContainerResolveOptions.Default);
        }

        /// <summary>
        /// Attempts to resolve a type using supplied options and  name.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// </summary>
        /// <param name="resolveType">Type to resolve</param>
        /// <param name="name">Name of registration</param>
        /// <param name="options">Resolution options</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="DependencyContainerResolutionException">Unable to resolve the type.</exception>
        public object Resolve(Type resolveType, string name, DependencyContainerResolveOptions options)
        {
            return ResolveInternal(new TypeRegistration(resolveType, name), DependencyContainerNamedParameterOverloads.Default, options);
        }

        /// <summary>
        /// Attempts to resolve a type using default options and the supplied constructor parameters.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// </summary>
        /// <param name="resolveType">Type to resolve</param>
        /// <param name="parameters">User specified constructor parameters</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="DependencyContainerResolutionException">Unable to resolve the type.</exception>
        public object Resolve(Type resolveType, DependencyContainerNamedParameterOverloads parameters)
        {
            return ResolveInternal(new TypeRegistration(resolveType), parameters, DependencyContainerResolveOptions.Default);
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
        public object Resolve(Type resolveType, DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options)
        {
            return ResolveInternal(new TypeRegistration(resolveType), parameters, options);
        }

        /// <summary>
        /// Attempts to resolve a type using default options and the supplied constructor parameters and name.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// </summary>
        /// <param name="resolveType">Type to resolve</param>
        /// <param name="parameters">User specified constructor parameters</param>
        /// <param name="name">Name of registration</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="DependencyContainerResolutionException">Unable to resolve the type.</exception>
        public object Resolve(Type resolveType, string name, DependencyContainerNamedParameterOverloads parameters)
        {
            return ResolveInternal(new TypeRegistration(resolveType, name), parameters, DependencyContainerResolveOptions.Default);
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
        public object Resolve(Type resolveType, string name, DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options)
        {
            return ResolveInternal(new TypeRegistration(resolveType, name), parameters, options);
        }

        /// <summary>
        /// Attempts to resolve a type using default options.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <returns>Instance of type</returns>
        /// <exception cref="DependencyContainerResolutionException">Unable to resolve the type.</exception>
        public ResolveType Resolve<ResolveType>()
            where ResolveType : class
        {
            return (ResolveType)Resolve(typeof(ResolveType));
        }

        /// <summary>
        /// Attempts to resolve a type using specified options.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="options">Resolution options</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="DependencyContainerResolutionException">Unable to resolve the type.</exception>
        public ResolveType Resolve<ResolveType>(DependencyContainerResolveOptions options)
            where ResolveType : class
        {
            return (ResolveType)Resolve(typeof(ResolveType), options);
        }

        /// <summary>
        /// Attempts to resolve a type using default options and the supplied name.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="name">Name of registration</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="DependencyContainerResolutionException">Unable to resolve the type.</exception>
        public ResolveType Resolve<ResolveType>(string name)
            where ResolveType : class
        {
            return (ResolveType)Resolve(typeof(ResolveType), name);
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
        public ResolveType Resolve<ResolveType>(string name, DependencyContainerResolveOptions options)
            where ResolveType : class
        {
            return (ResolveType)Resolve(typeof(ResolveType), name, options);
        }

        /// <summary>
        /// Attempts to resolve a type using default options and the supplied constructor parameters.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="parameters">User specified constructor parameters</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="DependencyContainerResolutionException">Unable to resolve the type.</exception>
        public ResolveType Resolve<ResolveType>(DependencyContainerNamedParameterOverloads parameters)
            where ResolveType : class
        {
            return (ResolveType)Resolve(typeof(ResolveType), parameters);
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
        public ResolveType Resolve<ResolveType>(DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options)
            where ResolveType : class
        {
            return (ResolveType)Resolve(typeof(ResolveType), parameters, options);
        }

        /// <summary>
        /// Attempts to resolve a type using default options and the supplied constructor parameters and name.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="parameters">User specified constructor parameters</param>
        /// <param name="name">Name of registration</param>
        /// <returns>Instance of type</returns>
        /// <exception cref="DependencyContainerResolutionException">Unable to resolve the type.</exception>
        public ResolveType Resolve<ResolveType>(string name, DependencyContainerNamedParameterOverloads parameters)
            where ResolveType : class
        {
            return (ResolveType)Resolve(typeof(ResolveType), name, parameters);
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
        public ResolveType Resolve<ResolveType>(string name, DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options)
            where ResolveType : class
        {
            return (ResolveType)Resolve(typeof(ResolveType), name, parameters, options);
        }

        /// <summary>
        /// Attempts to predict whether a given type can be resolved with default options.
        ///
        /// Note: Resolution may still fail if user defined factory registrations fail to construct objects when called.
        /// </summary>
        /// <param name="resolveType">Type to resolve</param>
        /// <returns>Bool indicating whether the type can be resolved</returns>
        public bool CanResolve(Type resolveType)
        {
            return CanResolveInternal(new TypeRegistration(resolveType), DependencyContainerNamedParameterOverloads.Default, DependencyContainerResolveOptions.Default);
        }

        /// <summary>
        /// Attempts to predict whether a given named type can be resolved with default options.
        ///
        /// Note: Resolution may still fail if user defined factory registrations fail to construct objects when called.
        /// </summary>
        /// <param name="resolveType">Type to resolve</param>
        /// <param name="name">Name of registration</param>
        /// <returns>Bool indicating whether the type can be resolved</returns>
        private bool CanResolve(Type resolveType, string name)
        {
            return CanResolveInternal(new TypeRegistration(resolveType, name), DependencyContainerNamedParameterOverloads.Default, DependencyContainerResolveOptions.Default);
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
        /// Attempts to predict whether a given named type can be resolved with the specified options.
        ///
        /// Note: Resolution may still fail if user defined factory registrations fail to construct objects when called.
        /// </summary>
        /// <param name="resolveType">Type to resolve</param>
        /// <param name="name">Name of registration</param>
        /// <param name="options">Resolution options</param>
        /// <returns>Bool indicating whether the type can be resolved</returns>
        public bool CanResolve(Type resolveType, string name, DependencyContainerResolveOptions options)
        {
            return CanResolveInternal(new TypeRegistration(resolveType, name), DependencyContainerNamedParameterOverloads.Default, options);
        }

        /// <summary>
        /// Attempts to predict whether a given type can be resolved with the supplied constructor parameters and default options.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// 
        /// Note: Resolution may still fail if user defined factory registrations fail to construct objects when called.
        /// </summary>
        /// <param name="resolveType">Type to resolve</param>
        /// <param name="parameters">User supplied named parameter overloads</param>
        /// <returns>Bool indicating whether the type can be resolved</returns>
        public bool CanResolve(Type resolveType, DependencyContainerNamedParameterOverloads parameters)
        {
            return CanResolveInternal(new TypeRegistration(resolveType), parameters, DependencyContainerResolveOptions.Default);
        }

        /// <summary>
        /// Attempts to predict whether a given named type can be resolved with the supplied constructor parameters and default options.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// 
        /// Note: Resolution may still fail if user defined factory registrations fail to construct objects when called.
        /// </summary>
        /// <param name="resolveType">Type to resolve</param>
        /// <param name="name">Name of registration</param>
        /// <param name="parameters">User supplied named parameter overloads</param>
        /// <returns>Bool indicating whether the type can be resolved</returns>
        public bool CanResolve(Type resolveType, string name, DependencyContainerNamedParameterOverloads parameters)
        {
            return CanResolveInternal(new TypeRegistration(resolveType, name), parameters, DependencyContainerResolveOptions.Default);
        }

        /// <summary>
        /// Attempts to predict whether a given type can be resolved with the supplied constructor parameters options.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// 
        /// Note: Resolution may still fail if user defined factory registrations fail to construct objects when called.
        /// </summary>
        /// <param name="resolveType">Type to resolve</param>
        /// <param name="parameters">User supplied named parameter overloads</param>
        /// <param name="options">Resolution options</param>
        /// <returns>Bool indicating whether the type can be resolved</returns>
        public bool CanResolve(Type resolveType, DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options)
        {
            return CanResolveInternal(new TypeRegistration(resolveType), parameters, options);
        }

        /// <summary>
        /// Attempts to predict whether a given named type can be resolved with the supplied constructor parameters options.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// 
        /// Note: Resolution may still fail if user defined factory registrations fail to construct objects when called.
        /// </summary>
        /// <param name="resolveType">Type to resolve</param>
        /// <param name="name">Name of registration</param>
        /// <param name="parameters">User supplied named parameter overloads</param>
        /// <param name="options">Resolution options</param>
        /// <returns>Bool indicating whether the type can be resolved</returns>
        public bool CanResolve(Type resolveType, string name, DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options)
        {
            return CanResolveInternal(new TypeRegistration(resolveType, name), parameters, options);
        }

        /// <summary>
        /// Attempts to predict whether a given type can be resolved with default options.
        ///
        /// Note: Resolution may still fail if user defined factory registrations fail to construct objects when called.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <returns>Bool indicating whether the type can be resolved</returns>
        public bool CanResolve<ResolveType>()
            where ResolveType : class
        {
            return CanResolve(typeof(ResolveType));
        }

        /// <summary>
        /// Attempts to predict whether a given named type can be resolved with default options.
        ///
        /// Note: Resolution may still fail if user defined factory registrations fail to construct objects when called.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <returns>Bool indicating whether the type can be resolved</returns>
        public bool CanResolve<ResolveType>(string name)
            where ResolveType : class
        {
            return CanResolve(typeof(ResolveType), name);
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
        public bool CanResolve<ResolveType>(string name, DependencyContainerResolveOptions options)
            where ResolveType : class
        {
            return CanResolve(typeof(ResolveType), name, options);
        }

        /// <summary>
        /// Attempts to predict whether a given type can be resolved with the supplied constructor parameters and default options.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// 
        /// Note: Resolution may still fail if user defined factory registrations fail to construct objects when called.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="parameters">User supplied named parameter overloads</param>
        /// <returns>Bool indicating whether the type can be resolved</returns>
        public bool CanResolve<ResolveType>(DependencyContainerNamedParameterOverloads parameters)
            where ResolveType : class
        {
            return CanResolve(typeof(ResolveType), parameters);
        }

        /// <summary>
        /// Attempts to predict whether a given named type can be resolved with the supplied constructor parameters and default options.
        ///
        /// Parameters are used in conjunction with normal container resolution to find the most suitable constructor (if one exists).
        /// All user supplied parameters must exist in at least one resolvable constructor of RegisterType or resolution will fail.
        /// 
        /// Note: Resolution may still fail if user defined factory registrations fail to construct objects when called.
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolve</typeparam>
        /// <param name="name">Name of registration</param>
        /// <param name="parameters">User supplied named parameter overloads</param>
        /// <returns>Bool indicating whether the type can be resolved</returns>
        public bool CanResolve<ResolveType>(string name, DependencyContainerNamedParameterOverloads parameters)
            where ResolveType : class
        {
            return CanResolve(typeof(ResolveType), name, parameters);
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
        public bool CanResolve<ResolveType>(DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options)
            where ResolveType : class
        {
            return CanResolve(typeof(ResolveType), parameters, options);
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
        public bool CanResolve<ResolveType>(string name, DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options)
            where ResolveType : class
        {
            return CanResolve(typeof(ResolveType), name, parameters, options);
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
        public IEnumerable<object> ResolveAll(Type resolveType, bool includeUnnamed)
        {
            return ResolveAllInternal(resolveType, includeUnnamed);
        }

        /// <summary>
        /// Returns all registrations of a type, both named and unnamed
        /// </summary>
        /// <param name="resolveType">Type to resolveAll</param>
        /// <returns>IEnumerable</returns>
        public IEnumerable<object> ResolveAll(Type resolveType)
        {
            return ResolveAll(resolveType, false);
        }

        /// <summary>
        /// Returns all registrations of a type
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolveAll</typeparam>
        /// <param name="includeUnnamed">Whether to include un-named (default) registrations</param>
        /// <returns>IEnumerable</returns>
        public IEnumerable<ResolveType> ResolveAll<ResolveType>(bool includeUnnamed)
            where ResolveType : class
        {
            return ResolveAll(typeof(ResolveType), includeUnnamed).Cast<ResolveType>();
        }

        /// <summary>
        /// Returns all registrations of a type, both named and unnamed
        /// </summary>
        /// <typeparam name="ResolveType">Type to resolveAll</typeparam>
        /// <returns>IEnumerable</returns>
        public IEnumerable<ResolveType> ResolveAll<ResolveType>()
            where ResolveType : class
        {
            return ResolveAll<ResolveType>(true);
        }

        /// <summary>
        /// Attempts to resolve all public property dependencies on the given object.
        /// </summary>
        /// <param name="input">Object to "build up"</param>
        public void BuildUp(object input)
        {
            BuildUpInternal(input, DependencyContainerResolveOptions.Default);
        }

        /// <summary>
        /// Attempts to resolve all public property dependencies on the given object using the given resolve options.
        /// </summary>
        /// <param name="input">Object to "build up"</param>
        /// <param name="resolveOptions">Resolve options to use</param>
        public void BuildUp(object input, DependencyContainerResolveOptions resolveOptions)
        {
            BuildUpInternal(input, resolveOptions);
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
            public ConstructorInfo Constructor { get; protected set; }

            /// <summary>
            /// Create the type
            /// </summary>
            /// <param name="requestedType">Type user requested to be resolved</param>
            /// <param name="container">Container that requested the creation</param>
            /// <param name="parameters">Any user parameters passed</param>
            /// <param name="options"></param>
            /// <returns></returns>
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
                    return container.ConstructType(requestedType, registerImplementation, Constructor, parameters, options);
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
            private readonly Type registerType;
            private readonly Type registerImplementation;
            private readonly object SingletonLock = new object();
            private object _Current;

            public SingletonFactory(Type registerType, Type registerImplementation)
            {
                //#if NETFX_CORE
                //				if (registerImplementation.GetTypeInfo().IsAbstract() || registerImplementation.GetTypeInfo().IsInterface())
                //#else
                if (registerImplementation.IsAbstract() || registerImplementation.IsInterface())
                    //#endif
                    throw new DependencyContainerRegistrationTypeException(registerImplementation, "SingletonFactory");

                if (!IsValidAssignment(registerType, registerImplementation))
                    throw new DependencyContainerRegistrationTypeException(registerImplementation, "SingletonFactory");

                this.registerType = registerType;
                this.registerImplementation = registerImplementation;
            }

            public override Type CreatesType => registerImplementation;

            public override object GetObject(Type requestedType, DependencyContainer container, DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options)
            {
                if (parameters.Count != 0)
                    throw new ArgumentException("Cannot specify parameters for singleton types");

                lock (SingletonLock)
                    if (_Current == null)
                        _Current = container.ConstructType(requestedType, registerImplementation, Constructor, options);

                return _Current;
            }

            public override ObjectFactoryBase SingletonVariant => this;

            public override ObjectFactoryBase GetCustomObjectLifetimeVariant(ITinyIoCObjectLifetimeProvider lifetimeProvider, string errorString)
            {
                return new CustomObjectLifetimeFactory(registerType, registerImplementation, lifetimeProvider, errorString);
            }

            public override ObjectFactoryBase MultiInstanceVariant => new MultiInstanceFactory(registerType, registerImplementation);

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
                var disposable = _Current as IDisposable;

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
                    throw new ArgumentNullException(nameof(lifetimeProvider), "lifetimeProvider is null.");

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
                        current = container.ConstructType(requestedType, registerImplementation, Constructor, options);
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
            public TypeRegistration(Type type)
                : this(type, string.Empty)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="TypeRegistration"/> class.
            /// </summary>
            /// <param name="type">The type.</param>
            /// <param name="name">The name.</param>
            public TypeRegistration(Type type, string name)
            {
                Type = type;
                Name = name;

                _hashCode = String.Concat(Type.FullName, "|", Name).GetHashCode();
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

                if (typeRegistration == null)
                    return false;

                if (Type != typeRegistration.Type)
                    return false;

                if (String.Compare(Name, typeRegistration.Name, StringComparison.Ordinal) != 0)
                    return false;

                return true;
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
            /// </returns>
            public override int GetHashCode()
            {
                return _hashCode;
            }
        }
        private readonly ConcurrentDictionary<TypeRegistration, ObjectFactoryBase> _RegisteredTypes;
        private delegate object ObjectConstructor(params object[] parameters);
        private static readonly ConcurrentDictionary<ConstructorInfo, ObjectConstructor> _ObjectConstructorCache 
            = new ConcurrentDictionary<ConstructorInfo, ObjectConstructor>();
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainer"/> class.
        /// </summary>
        public DependencyContainer()
        {
            _RegisteredTypes = new ConcurrentDictionary<TypeRegistration, ObjectFactoryBase>();

            RegisterDefaultTypes();
        }

        readonly DependencyContainer _Parent;
        private DependencyContainer(DependencyContainer parent)
            : this()
        {
            _Parent = parent;
        }
        #endregion

        #region Internal Methods
        private readonly object _AutoRegisterLock = new object();

        private void AutoRegisterInternal(IEnumerable<Assembly> assemblies, DependencyContainerDuplicateImplementationActions duplicateAction, Func<Type, bool> registrationPredicate)
        {
            lock (_AutoRegisterLock)
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
                    var implementations = concreteTypes.Where(implementationType => localType.GetTypeInfo().IsAssignableFrom(implementationType)).ToList();

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

                    if (firstImplementation != null)
                    {
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
        }

        private bool IsIgnoredAssembly(Assembly assembly)
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

            foreach (var check in ignoreChecks)
            {
                if (check(assembly))
                    return true;
            }

            return false;
        }

        private bool IsIgnoredType(Type type, Func<Type, bool> registrationPredicate)
        {
            // TODO - find a better way to remove "system" types from the auto registration
            var ignoreChecks = new List<Func<Type, bool>>()
            {
                t => t.FullName.StartsWith("System.", StringComparison.Ordinal),
                t => t.FullName.StartsWith("Microsoft.", StringComparison.Ordinal),
                t => t.IsPrimitive(),
                t => t.IsGenericTypeDefinition(),
                t => (t.GetTypeInfo().GetConstructors(BindingFlags.Instance | BindingFlags.Public).Length == 0) && !(t.IsInterface() || t.IsAbstract()),
            };

            if (registrationPredicate != null)
            {
                ignoreChecks.Add(t => !registrationPredicate(t));
            }

            foreach (var check in ignoreChecks)
            {
                if (check(type))
                    return true;
            }

            return false;
        }

        private void RegisterDefaultTypes()
        {
            Register(this);

            // Only register the TinyMessenger singleton if we are the root container
            if (_Parent == null)
                Register<IMessageHub, MessageHub>();
        }

        private ObjectFactoryBase GetCurrentFactory(TypeRegistration registration)
        {
            ObjectFactoryBase current = null;

            _RegisteredTypes.TryGetValue(registration, out current);

            return current;
        }

        private RegisterOptions RegisterInternal(Type registerType, string name, ObjectFactoryBase factory)
        {
            var typeRegistration = new TypeRegistration(registerType, name);

            return AddUpdateRegistration(typeRegistration, factory);
        }

        private RegisterOptions AddUpdateRegistration(TypeRegistration typeRegistration, ObjectFactoryBase factory)
        {
            _RegisteredTypes[typeRegistration] = factory;

            return new RegisterOptions(this, typeRegistration);
        }

        private bool RemoveRegistration(TypeRegistration typeRegistration)
        {
            ObjectFactoryBase item;
            return _RegisteredTypes.TryRemove(typeRegistration, out item);
        }

        private ObjectFactoryBase GetDefaultObjectFactory(Type registerType, Type registerImplementation)
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
            if (_RegisteredTypes.TryGetValue(new TypeRegistration(checkType, name), out factory))
            {
                if (factory.AssumeConstruction)
                    return true;

                if (factory.Constructor == null)
                    return (GetBestConstructor(factory.CreatesType, parameters, options) != null);

                return CanConstruct(factory.Constructor, parameters, options);
            }

#if RESOLVE_OPEN_GENERICS
            if (checkType.IsInterface() && checkType.IsGenericType())
            {
                // if the type is registered as an open generic, then see if the open generic is registered
                if (_RegisteredTypes.TryGetValue(new TypeRegistration(checkType.GetGenericTypeDefinition(), name), out factory))
                {
                    if (factory.AssumeConstruction)
                        return true;

                    if (factory.Constructor == null)
                        return (GetBestConstructor(factory.CreatesType, parameters, options) != null) ? true : false;
                    else
                        return CanConstruct(factory.Constructor, parameters, options);
                }
            }
#endif

            // Fail if requesting named resolution and settings set to fail if unresolved
            // Or bubble up if we have a parent
            if (!string.IsNullOrEmpty(name) && options.NamedResolutionFailureAction == DependencyContainerNamedResolutionFailureActions.Fail)
                return _Parent?.CanResolveInternal(registration, parameters, options) ?? false;

            // Attempted unnamed fallback container resolution if relevant and requested
            if (!string.IsNullOrEmpty(name) && options.NamedResolutionFailureAction == DependencyContainerNamedResolutionFailureActions.AttemptUnnamedResolution)
            {
                if (_RegisteredTypes.TryGetValue(new TypeRegistration(checkType), out factory))
                {
                    if (factory.AssumeConstruction)
                        return true;

                    return (GetBestConstructor(factory.CreatesType, parameters, options) != null) ? true : false;
                }
            }

            // Check if type is an automatic lazy factory request
            if (IsAutomaticLazyFactoryRequest(checkType))
                return true;

            // Check if type is an IEnumerable<ResolveType>
            if (IsIEnumerableRequest(registration.Type))
                return true;

            // Attempt unregistered construction if possible and requested
            // If we cant', bubble if we have a parent
            if ((options.UnregisteredResolutionAction == DependencyContainerUnregisteredResolutionActions.AttemptResolve) || (checkType.IsGenericType() && options.UnregisteredResolutionAction == DependencyContainerUnregisteredResolutionActions.GenericsOnly))
                return (GetBestConstructor(checkType, parameters, options) != null) || (_Parent?.CanResolveInternal(registration, parameters, options) ?? false);

            // Bubble resolution up the container tree if we have a parent
            return _Parent != null && _Parent.CanResolveInternal(registration, parameters, options);
        }

        private bool IsIEnumerableRequest(Type type)
        {
            if (!type.IsGenericType())
                return false;

            var genericType = type.GetGenericTypeDefinition();

            return genericType == typeof(IEnumerable<>);
        }

        private bool IsAutomaticLazyFactoryRequest(Type type)
        {
            if (!type.IsGenericType())
                return false;

            Type genericType = type.GetGenericTypeDefinition();

            // Just a func
            if (genericType == typeof(Func<>))
                return true;

            // 2 parameter func with string as first parameter (name)
            //#if NETFX_CORE
            //			if ((genericType == typeof(Func<,>) && type.GetTypeInfo().GenericTypeArguments[0] == typeof(string)))
            //#else
            if ((genericType == typeof(Func<,>) && type.GetTypeInfo().GetGenericArguments()[0] == typeof(string)))
                //#endif
                return true;

            // 3 parameter func with string as first parameter (name) and IDictionary<string, object> as second (parameters)
            //#if NETFX_CORE
            //			if ((genericType == typeof(Func<,,>) && type.GetTypeInfo().GenericTypeArguments[0] == typeof(string) && type.GetTypeInfo().GenericTypeArguments[1] == typeof(IDictionary<String, object>)))
            //#else
            if ((genericType == typeof(Func<,,>) && type.GetTypeInfo().GetGenericArguments()[0] == typeof(string) && type.GetTypeInfo().GetGenericArguments()[1] == typeof(IDictionary<String, object>)))
                //#endif
                return true;

            return false;
        }

        private ObjectFactoryBase GetParentObjectFactory(TypeRegistration registration)
        {
            if (_Parent == null)
                return null;

            ObjectFactoryBase factory;
            if (_Parent._RegisteredTypes.TryGetValue(registration, out factory))
            {
                return factory.GetFactoryForChildContainer(registration.Type, _Parent, this);
            }

            return _Parent.GetParentObjectFactory(registration);
        }

        private object ResolveInternal(TypeRegistration registration, DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options)
        {
            ObjectFactoryBase factory;

            // Attempt container resolution
            if (_RegisteredTypes.TryGetValue(registration, out factory))
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

#if RESOLVE_OPEN_GENERICS
            // Attempt container resolution of open generic
            if (registration.Type.IsGenericType())
            {
                var openTypeRegistration = new TypeRegistration(registration.Type.GetGenericTypeDefinition(),
                                                                registration.Name);

                if (_RegisteredTypes.TryGetValue(openTypeRegistration, out factory))
                {
                    try
                    {
                        return factory.GetObject(registration.Type, this, parameters, options);
                    }
                    catch (TinyIoCResolutionException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        throw new TinyIoCResolutionException(registration.Type, ex);
                    }
                }
            }
#endif

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

            // Attemped unnamed fallback container resolution if relevant and requested
            if (!string.IsNullOrEmpty(registration.Name) && options.NamedResolutionFailureAction == DependencyContainerNamedResolutionFailureActions.AttemptUnnamedResolution)
            {
                if (_RegisteredTypes.TryGetValue(new TypeRegistration(registration.Type, string.Empty), out factory))
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

#if EXPRESSIONS
            // Attempt to construct an automatic lazy factory if possible
            if (IsAutomaticLazyFactoryRequest(registration.Type))
                return GetLazyAutomaticFactoryRequest(registration.Type);
#endif
            if (IsIEnumerableRequest(registration.Type))
                return GetIEnumerableRequest(registration.Type);

            // Attempt unregistered construction if possible and requested
            if ((options.UnregisteredResolutionAction == DependencyContainerUnregisteredResolutionActions.AttemptResolve) || (registration.Type.IsGenericType() && options.UnregisteredResolutionAction == DependencyContainerUnregisteredResolutionActions.GenericsOnly))
            {
                if (!registration.Type.IsAbstract() && !registration.Type.IsInterface())
                    return ConstructType(null, registration.Type, parameters, options);
            }

            // Unable to resolve - throw
            throw new DependencyContainerResolutionException(registration.Type);
        }

#if EXPRESSIONS
        private object GetLazyAutomaticFactoryRequest(Type type)
        {
            if (!type.IsGenericType())
                return null;

            Type genericType = type.GetGenericTypeDefinition();
            //#if NETFX_CORE
            //			Type[] genericArguments = type.GetTypeInfo().GenericTypeArguments.ToArray();
            //#else
            Type[] genericArguments = type.GetGenericArguments();
            //#endif

            // Just a func
            if (genericType == typeof(Func<>))
            {
                Type returnType = genericArguments[0];

                //#if NETFX_CORE
                //				MethodInfo resolveMethod = typeof(TinyIoCContainer).GetTypeInfo().GetDeclaredMethods("Resolve").First(mi => !mi.GetParameters().Any());
                //#else
                MethodInfo resolveMethod = typeof(TinyIoCContainer).GetMethod("Resolve", new Type[] { });
                //#endif
                resolveMethod = resolveMethod.MakeGenericMethod(returnType);

                var resolveCall = Expression.Call(Expression.Constant(this), resolveMethod);

                var resolveLambda = Expression.Lambda(resolveCall).Compile();

                return resolveLambda;
            }

            // 2 parameter func with string as first parameter (name)
            if ((genericType == typeof(Func<,>)) && (genericArguments[0] == typeof(string)))
            {
                Type returnType = genericArguments[1];

                //#if NETFX_CORE
                //				MethodInfo resolveMethod = typeof(TinyIoCContainer).GetTypeInfo().GetDeclaredMethods("Resolve").First(mi => mi.GetParameters().Length == 1 && mi.GetParameters()[0].GetType() == typeof(String));
                //#else
                MethodInfo resolveMethod = typeof(TinyIoCContainer).GetMethod("Resolve", new Type[] { typeof(String) });
                //#endif
                resolveMethod = resolveMethod.MakeGenericMethod(returnType);

                ParameterExpression[] resolveParameters = new ParameterExpression[] { Expression.Parameter(typeof(String), "name") };
                var resolveCall = Expression.Call(Expression.Constant(this), resolveMethod, resolveParameters);

                var resolveLambda = Expression.Lambda(resolveCall, resolveParameters).Compile();

                return resolveLambda;
            }

            // 3 parameter func with string as first parameter (name) and IDictionary<string, object> as second (parameters)
            //#if NETFX_CORE
            //			if ((genericType == typeof(Func<,,>) && type.GenericTypeArguments[0] == typeof(string) && type.GenericTypeArguments[1] == typeof(IDictionary<string, object>)))
            //#else
            if ((genericType == typeof(Func<,,>) && type.GetGenericArguments()[0] == typeof(string) && type.GetGenericArguments()[1] == typeof(IDictionary<string, object>)))
            //#endif
            {
                Type returnType = genericArguments[2];

                var name = Expression.Parameter(typeof(string), "name");
                var parameters = Expression.Parameter(typeof(IDictionary<string, object>), "parameters");

                //#if NETFX_CORE
                //				MethodInfo resolveMethod = typeof(TinyIoCContainer).GetTypeInfo().GetDeclaredMethods("Resolve").First(mi => mi.GetParameters().Length == 2 && mi.GetParameters()[0].GetType() == typeof(String) && mi.GetParameters()[1].GetType() == typeof(NamedParameterOverloads));
                //#else
                MethodInfo resolveMethod = typeof(TinyIoCContainer).GetMethod("Resolve", new Type[] { typeof(String), typeof(NamedParameterOverloads) });
                //#endif
                resolveMethod = resolveMethod.MakeGenericMethod(returnType);

                var resolveCall = Expression.Call(Expression.Constant(this), resolveMethod, name, Expression.Call(typeof(NamedParameterOverloads), "FromIDictionary", null, parameters));

                var resolveLambda = Expression.Lambda(resolveCall, name, parameters).Compile();

                return resolveLambda;
            }

            throw new TinyIoCResolutionException(type);
        }
#endif
        private object GetIEnumerableRequest(Type type)
        {
            //#if NETFX_CORE
            //			var genericResolveAllMethod = this.GetType().GetGenericMethod("ResolveAll", type.GenericTypeArguments, new[] { typeof(bool) });
            //#else
            var genericResolveAllMethod = GetType().GetGenericMethod(BindingFlags.Public | BindingFlags.Instance, "ResolveAll", type.GetTypeInfo().GetGenericArguments(), new[] { typeof(bool) });
            //#endif

            return genericResolveAllMethod.Invoke(this, new object[] { false });
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

                //#if NETFX_CORE                
                //				if (parameter.ParameterType.GetTypeInfo().IsPrimitive && !isParameterOverload)
                //#else
                if (parameter.ParameterType.IsPrimitive() && !isParameterOverload)
                    //#endif
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

            //#if NETFX_CORE
            //			if (type.GetTypeInfo().IsValueType)
            //#else
            if (type.IsValueType())
                //#endif
                return null;

            // Get constructors in reverse order based on the number of parameters
            // i.e. be as "greedy" as possible so we satify the most amount of dependencies possible
            var ctors = GetTypeConstructors(type);

            foreach (var ctor in ctors)
            {
                if (CanConstruct(ctor, parameters, options))
                    return ctor;
            }

            return null;
        }

        private static IEnumerable<ConstructorInfo> GetTypeConstructors(Type type)
        {
            //#if NETFX_CORE
            //			return type.GetTypeInfo().DeclaredConstructors.OrderByDescending(ctor => ctor.GetParameters().Count());
            //#else
            return type.GetTypeInfo().GetConstructors().OrderByDescending(ctor => ctor.GetParameters().Count());
            //#endif
        }

        private object ConstructType(Type requestedType, Type implementationType, DependencyContainerResolveOptions options)
        {
            return ConstructType(requestedType, implementationType, null, DependencyContainerNamedParameterOverloads.Default, options);
        }

        private object ConstructType(Type requestedType, Type implementationType, ConstructorInfo constructor, DependencyContainerResolveOptions options)
        {
            return ConstructType(requestedType, implementationType, constructor, DependencyContainerNamedParameterOverloads.Default, options);
        }

        private object ConstructType(Type requestedType, Type implementationType, DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options)
        {
            return ConstructType(requestedType, implementationType, null, parameters, options);
        }

        private object ConstructType(Type requestedType, Type implementationType, ConstructorInfo constructor, DependencyContainerNamedParameterOverloads parameters, DependencyContainerResolveOptions options)
        {
            var typeToConstruct = implementationType;

#if RESOLVE_OPEN_GENERICS
            if (implementationType.IsGenericTypeDefinition())
            {
                if (requestedType == null || !requestedType.IsGenericType() || !requestedType.GetGenericArguments().Any())
                    throw new TinyIoCResolutionException(typeToConstruct);

                typeToConstruct = typeToConstruct.MakeGenericType(requestedType.GetGenericArguments());
            }
#endif
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
            var args = new object[ctorParams.Count()];

            for (int parameterIndex = 0; parameterIndex < ctorParams.Count(); parameterIndex++)
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
            if (_ObjectConstructorCache.TryGetValue(constructor, out objectConstructor))
                return objectConstructor;

            // We could lock the cache here, but there's no real side
            // effect to two threads creating the same ObjectConstructor
            // at the same time, compared to the cost of a lock for 
            // every creation.
            var constructorParams = constructor.GetParameters();
            var lambdaParams = Expression.Parameter(typeof(object[]), "parameters");
            var newParams = new Expression[constructorParams.Length];

            for (int i = 0; i < constructorParams.Length; i++)
            {
                var paramsParameter = Expression.ArrayIndex(lambdaParams, Expression.Constant(i));

                newParams[i] = Expression.Convert(paramsParameter, constructorParams[i].ParameterType);
            }

            var newExpression = Expression.New(constructor, newParams);

            var constructionLambda = Expression.Lambda(typeof(ObjectConstructor), newExpression, lambdaParams);

            objectConstructor = (ObjectConstructor)constructionLambda.Compile();

            _ObjectConstructorCache[constructor] = objectConstructor;
            return objectConstructor;
        }
#endif

        private void BuildUpInternal(object input, DependencyContainerResolveOptions resolveOptions)
        {
            //#if NETFX_CORE
            //			var properties = from property in input.GetType().GetTypeInfo().DeclaredProperties
            //							 where (property.GetMethod != null) && (property.SetMethod != null) && !property.PropertyType.GetTypeInfo().IsValueType
            //							 select property;
            //#else
            var properties = from property in input.GetType().GetTypeInfo().GetProperties()
                             where (property.GetGetMethod() != null) && (property.GetSetMethod() != null) && !property.PropertyType.IsValueType()
                             select property;
            //#endif

            foreach (var property in properties)
            {
                if (property.GetValue(input, null) == null)
                {
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
        }

        private IEnumerable<TypeRegistration> GetParentRegistrationsForType(Type resolveType)
        {
            if (_Parent == null)
                return new TypeRegistration[] { };

            var registrations = _Parent._RegisteredTypes.Keys.Where(tr => tr.Type == resolveType);

            return registrations.Concat(_Parent.GetParentRegistrationsForType(resolveType));
        }

        private IEnumerable<object> ResolveAllInternal(Type resolveType, bool includeUnnamed)
        {
            var registrations = _RegisteredTypes.Keys.Where(tr => tr.Type == resolveType).Concat(GetParentRegistrationsForType(resolveType)).Distinct();

            if (!includeUnnamed)
                registrations = registrations.Where(tr => tr.Name != string.Empty);

            return registrations.Select(registration => ResolveInternal(registration, DependencyContainerNamedParameterOverloads.Default, DependencyContainerResolveOptions.Default));
        }

        private static bool IsValidAssignment(Type registerType, Type registerImplementation)
        {
            if (!registerType.IsGenericTypeDefinition())
            {
                if (!registerType.GetTypeInfo().IsAssignableFrom(registerImplementation))
                    return false;
            }
            else
            {
                if (registerType.IsInterface())
                {
                    if (registerImplementation.GetTypeInfo().GetInterfaces().All(t => t.Name != registerType.Name))
                        return false;
                }
                else if (registerType.IsAbstract() && registerImplementation.BaseType() != registerType)
                {
                    return false;
                }
            }
            //#endif
            return true;
        }

        #endregion

        #region IDisposable Members

        bool disposed = false;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                foreach (var item in _RegisteredTypes.Values)
                {
                    var disposable = item as IDisposable;
                    disposable?.Dispose();
                }

                GC.SuppressFinalize(this);
            }
        }

        #endregion
    }

}