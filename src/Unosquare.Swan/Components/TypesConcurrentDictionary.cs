namespace Unosquare.Swan.Components
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;    
    using System.Collections.Generic;
    using System.Linq;
    using Exceptions;
    using System.Collections.Concurrent;

    /// <summary>
    /// Represents a Concurrent Dictionary for TypeRegistration.
    /// </summary>
    public class TypesConcurrentDictionary : ConcurrentDictionary<DependencyContainer.TypeRegistration, ObjectFactoryBase>
    {
        private static readonly ConcurrentDictionary<ConstructorInfo, ObjectConstructor> ObjectConstructorCache =
            new ConcurrentDictionary<ConstructorInfo, ObjectConstructor>();

        private readonly DependencyContainer _dependencyContainer;

        internal TypesConcurrentDictionary(DependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        /// <summary>
        /// Represents a delegate to build an object with the parameters.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The built object.</returns>
        public delegate object ObjectConstructor(params object[] parameters);

        internal IEnumerable<object> Resolve(Type resolveType, bool includeUnnamed)
        {
            var registrations = Keys.Where(tr => tr.Type == resolveType)
                .Concat(GetParentRegistrationsForType(resolveType)).Distinct();

            if (!includeUnnamed)
                registrations = registrations.Where(tr => !string.IsNullOrEmpty(tr.Name));

            return registrations.Select(registration =>
                ResolveInternal(registration, DependencyContainerResolveOptions.Default));
        }
        
        internal ObjectFactoryBase GetCurrentFactory(DependencyContainer.TypeRegistration registration)
        {
            TryGetValue(registration, out var current);

            return current;
        }

        internal RegisterOptions Register(Type registerType, string name, ObjectFactoryBase factory) 
            => AddUpdateRegistration(new DependencyContainer.TypeRegistration(registerType, name), factory);

        internal RegisterOptions AddUpdateRegistration(DependencyContainer.TypeRegistration typeRegistration, ObjectFactoryBase factory)
        {
            this[typeRegistration] = factory;

            return new RegisterOptions(this, typeRegistration);
        }

        internal bool RemoveRegistration(DependencyContainer.TypeRegistration typeRegistration)
            => TryRemove(typeRegistration, out _);
        
        internal object ResolveInternal(
            DependencyContainer.TypeRegistration registration,
            DependencyContainerResolveOptions options = null)
        {
            if (options == null)
                options = DependencyContainerResolveOptions.Default;

            // Attempt container resolution
            if (TryGetValue(registration, out var factory))
            {
                try
                {
                    return factory.GetObject(registration.Type, _dependencyContainer, options);
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
                    return bubbledObjectFactory.GetObject(registration.Type, _dependencyContainer, options);
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
            if (!string.IsNullOrEmpty(registration.Name) && options.NamedResolutionFailureAction ==
                DependencyContainerNamedResolutionFailureActions.Fail)
                throw new DependencyContainerResolutionException(registration.Type);

            // Attempted unnamed fallback container resolution if relevant and requested
            if (!string.IsNullOrEmpty(registration.Name) && options.NamedResolutionFailureAction ==
                DependencyContainerNamedResolutionFailureActions.AttemptUnnamedResolution)
            {
                if (TryGetValue(new DependencyContainer.TypeRegistration(registration.Type, string.Empty), out factory))
                {
                    try
                    {
                        return factory.GetObject(registration.Type, _dependencyContainer, options);
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
            var isValid = (options.UnregisteredResolutionAction ==
                           DependencyContainerUnregisteredResolutionActions.AttemptResolve) ||
                          (registration.Type.IsGenericType() && options.UnregisteredResolutionAction ==
                           DependencyContainerUnregisteredResolutionActions.GenericsOnly);

            return isValid && !registration.Type.IsAbstract() && !registration.Type.IsInterface()
                ? ConstructType(registration.Type, null, options)
                : throw new DependencyContainerResolutionException(registration.Type);
        }
        
        internal bool CanResolve(
            DependencyContainer.TypeRegistration registration,
            DependencyContainerResolveOptions options = null)
        {
            if (options == null)
                options = DependencyContainerResolveOptions.Default;

            var checkType = registration.Type;
            var name = registration.Name;

            if (TryGetValue(new DependencyContainer.TypeRegistration(checkType, name), out var factory))
            {
                if (factory.AssumeConstruction)
                    return true;

                if (factory.Constructor == null)
                    return GetBestConstructor(factory.CreatesType, options) != null;

                return CanConstruct(factory.Constructor, options);
            }

            // Fail if requesting named resolution and settings set to fail if unresolved
            // Or bubble up if we have a parent
            if (!string.IsNullOrEmpty(name) && options.NamedResolutionFailureAction ==
                DependencyContainerNamedResolutionFailureActions.Fail)
                return _dependencyContainer.Parent?.RegisteredTypes.CanResolve(registration, options.Clone()) ?? false;

            // Attempted unnamed fallback container resolution if relevant and requested
            if (!string.IsNullOrEmpty(name) && options.NamedResolutionFailureAction ==
                DependencyContainerNamedResolutionFailureActions.AttemptUnnamedResolution)
            {
                if (TryGetValue(new DependencyContainer.TypeRegistration(checkType), out factory))
                {
                    if (factory.AssumeConstruction)
                        return true;

                    return GetBestConstructor(factory.CreatesType, options) != null;
                }
            }

            // Check if type is an automatic lazy factory request or an IEnumerable<ResolveType>
            if (IsAutomaticLazyFactoryRequest(checkType) || registration.Type.IsIEnumerable())
                return true;

            // Attempt unregistered construction if possible and requested
            // If we cant', bubble if we have a parent
            if ((options.UnregisteredResolutionAction ==
                 DependencyContainerUnregisteredResolutionActions.AttemptResolve) ||
                (checkType.IsGenericType() && options.UnregisteredResolutionAction ==
                 DependencyContainerUnregisteredResolutionActions.GenericsOnly))
            {
                return (GetBestConstructor(checkType, options) != null) ||
                       (_dependencyContainer.Parent?.RegisteredTypes.CanResolve(registration, options.Clone()) ?? false);
            }

            // Bubble resolution up the container tree if we have a parent
            return _dependencyContainer.Parent != null && _dependencyContainer.Parent.RegisteredTypes.CanResolve(registration, options.Clone());
        }
        
        internal object ConstructType(
            Type implementationType,
            ConstructorInfo constructor,
            DependencyContainerResolveOptions options = null)
        {
            var typeToConstruct = implementationType;

            if (constructor == null)
            {
                // Try and get the best constructor that we can construct
                // if we can't construct any then get the constructor
                // with the least number of parameters so we can throw a meaningful
                // resolve exception
                constructor = GetBestConstructor(typeToConstruct, options) ??
                              GetTypeConstructors(typeToConstruct).LastOrDefault();
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
                    args[parameterIndex] = options?.ConstructorParameters.GetValueOrDefault(currentParam.Name, ResolveInternal(new DependencyContainer.TypeRegistration(currentParam.ParameterType), options.Clone()));
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
                return CreateObjectConstructionDelegateWithCache(constructor).Invoke(args);
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
            return genericType == typeof(Func<,,>) && type.GetGenericArguments()[0] == typeof(string) &&
                   type.GetGenericArguments()[1] == typeof(IDictionary<string, object>);
        }
        
        private ObjectFactoryBase GetParentObjectFactory(DependencyContainer.TypeRegistration registration)
        {
            if (_dependencyContainer.Parent == null)
                return null;

            return _dependencyContainer.Parent.RegisteredTypes.TryGetValue(registration, out var factory)
                ? factory.GetFactoryForChildContainer(registration.Type, _dependencyContainer.Parent, _dependencyContainer)
                : _dependencyContainer.Parent.RegisteredTypes.GetParentObjectFactory(registration);
        }

        private ConstructorInfo GetBestConstructor(
            Type type,
            DependencyContainerResolveOptions options)
            => type.IsValueType() ? null : GetTypeConstructors(type).FirstOrDefault(ctor => CanConstruct(ctor, options));
        
        private bool CanConstruct(
            ConstructorInfo ctor,
            DependencyContainerResolveOptions options)
        {
            foreach (var parameter in ctor.GetParameters())
            {
                if (string.IsNullOrEmpty(parameter.Name))
                    return false;

                var isParameterOverload = options.ConstructorParameters.ContainsKey(parameter.Name);

                if (parameter.ParameterType.IsPrimitive() && !isParameterOverload)
                    return false;

                if (!isParameterOverload &&
                    !CanResolve(new DependencyContainer.TypeRegistration(parameter.ParameterType), options.Clone()))
                    return false;
            }

            return true;
        }

        private IEnumerable<DependencyContainer.TypeRegistration> GetParentRegistrationsForType(Type resolveType)
            => _dependencyContainer.Parent == null 
                ? Array.Empty<DependencyContainer.TypeRegistration>()
                : _dependencyContainer.Parent.RegisteredTypes.Keys.Where(tr => tr.Type == resolveType).Concat(_dependencyContainer.Parent.RegisteredTypes.GetParentRegistrationsForType(resolveType));
    }
}
