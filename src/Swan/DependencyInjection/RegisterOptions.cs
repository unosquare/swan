namespace Swan.DependencyInjection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Registration options for "fluent" API.
    /// </summary>
    public sealed class RegisterOptions
    {
        private readonly TypesConcurrentDictionary _registeredTypes;
        private readonly DependencyContainer.TypeRegistration _registration;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterOptions" /> class.
        /// </summary>
        /// <param name="registeredTypes">The registered types.</param>
        /// <param name="registration">The registration.</param>
        public RegisterOptions(TypesConcurrentDictionary registeredTypes, DependencyContainer.TypeRegistration registration)
        {
            _registeredTypes = registeredTypes;
            _registration = registration;
        }

        /// <summary>
        /// Make registration a singleton (single instance) if possible.
        /// </summary>
        /// <returns>A registration options  for fluent API.</returns>
        /// <exception cref="DependencyContainerRegistrationException">Generic constraint registration exception.</exception>
        public RegisterOptions AsSingleton()
        {
            var currentFactory = _registeredTypes.GetCurrentFactory(_registration);

            if (currentFactory == null)
                throw new DependencyContainerRegistrationException(_registration.Type, "singleton");

            return _registeredTypes.AddUpdateRegistration(_registration, currentFactory.SingletonVariant);
        }

        /// <summary>
        /// Make registration multi-instance if possible.
        /// </summary>
        /// <returns>A registration options  for fluent API.</returns>
        /// <exception cref="DependencyContainerRegistrationException">Generic constraint registration exception.</exception>
        public RegisterOptions AsMultiInstance()
        {
            var currentFactory = _registeredTypes.GetCurrentFactory(_registration);

            if (currentFactory == null)
                throw new DependencyContainerRegistrationException(_registration.Type, "multi-instance");

            return _registeredTypes.AddUpdateRegistration(_registration, currentFactory.MultiInstanceVariant);
        }

        /// <summary>
        /// Make registration hold a weak reference if possible.
        /// </summary>
        /// <returns>A registration options  for fluent API.</returns>
        /// <exception cref="DependencyContainerRegistrationException">Generic constraint registration exception.</exception>
        public RegisterOptions WithWeakReference()
        {
            var currentFactory = _registeredTypes.GetCurrentFactory(_registration);

            if (currentFactory == null)
                throw new DependencyContainerRegistrationException(_registration.Type, "weak reference");

            return _registeredTypes.AddUpdateRegistration(_registration, currentFactory.WeakReferenceVariant);
        }

        /// <summary>
        /// Make registration hold a strong reference if possible.
        /// </summary>
        /// <returns>A registration options  for fluent API.</returns>
        /// <exception cref="DependencyContainerRegistrationException">Generic constraint registration exception.</exception>
        public RegisterOptions WithStrongReference()
        {
            var currentFactory = _registeredTypes.GetCurrentFactory(_registration);

            if (currentFactory == null)
                throw new DependencyContainerRegistrationException(_registration.Type, "strong reference");

            return _registeredTypes.AddUpdateRegistration(_registration, currentFactory.StrongReferenceVariant);
        }
    }

    /// <summary>
    /// Registration options for "fluent" API when registering multiple implementations.
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
        /// Make registration a singleton (single instance) if possible.
        /// </summary>
        /// <returns>A registration multi-instance for fluent API.</returns>
        /// <exception cref="DependencyContainerRegistrationException">Generic Constraint Registration Exception.</exception>
        public MultiRegisterOptions AsSingleton()
        {
            _registerOptions = ExecuteOnAllRegisterOptions(ro => ro.AsSingleton());
            return this;
        }

        /// <summary>
        /// Make registration multi-instance if possible.
        /// </summary>
        /// <returns>A registration multi-instance for fluent API.</returns>
        /// <exception cref="DependencyContainerRegistrationException">Generic Constraint Registration Exception.</exception>
        public MultiRegisterOptions AsMultiInstance()
        {
            _registerOptions = ExecuteOnAllRegisterOptions(ro => ro.AsMultiInstance());
            return this;
        }

        private IEnumerable<RegisterOptions> ExecuteOnAllRegisterOptions(
            Func<RegisterOptions, RegisterOptions> action)
        {
            return _registerOptions.Select(action).ToList();
        }
    }
}