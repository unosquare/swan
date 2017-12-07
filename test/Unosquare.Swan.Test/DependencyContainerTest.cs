namespace Unosquare.Swan.Test.DependencyContainerTest
{
    using System;
    using NUnit.Framework;
    using System.Collections.Generic;
    using Components;
    using Exceptions;
    using Mocks;
    using System.Linq;

    [TestFixture]
    public class AutoRegister
    {
#if !NETSTANDARD1_3 && !UWP
        [Test]
        public void WithFailDependencyContainer_ThrowsDependencyContainerRegistrationException()
        {
            var container = new DependencyContainer();

            Assert.Throws<DependencyContainerRegistrationException>(() =>
                container.AutoRegister(DependencyContainerDuplicateImplementationActions.Fail));
        }

        [Test]
        public void WithNoParams_ThrowsDependencyContainerResolutionException()
        {
            Assert.Throws<DependencyContainerResolutionException>(() =>
            {
                var container = new DependencyContainer();
                container.AutoRegister();

                Assert.IsTrue(container.CanResolve<ICar>());
                Assert.AreEqual(new TheOnlyCar().Name, DependencyContainer.Current.Resolve<ICar>().Name);
            });
        }
#endif

        [Test]
        public void WithAssemblies_ResolvesIAnimal()
        {
            var container = new DependencyContainer();
            container.AutoRegister(Runtime.GetAssemblies());

            Assert.IsTrue(container.CanResolve<ICar>());
        }
        
        [Test]
        public void WithAssembliesAndFunc_ResolvesICar()
        {
            var container = new DependencyContainer();

            container.AutoRegister(
                Runtime.GetAssemblies(),
                DependencyContainerDuplicateImplementationActions.RegisterSingle,
                (param) => true);

            Assert.IsTrue(container.CanResolve<ICar>());
        }

        [Test]
        public void WithAssembliesAndRegisterMultiple_ResolvesICar()
        {
            var container = new DependencyContainer();

            container.AutoRegister(
                Runtime.GetAssemblies(),
                DependencyContainerDuplicateImplementationActions.RegisterMultiple);

            Assert.IsTrue(container.CanResolve<ICar>());
        }
    }

    [TestFixture]
    public class BuildUp
    {
        [Test]
        public void WithController_ResolvePublicProperties()
        {
            var container = new DependencyContainer();

            container.Register<IAnimal, Fish>();
            container.Register<ICar, TheOnlyCar>();

            var instance = new Controller();

            container.BuildUp(instance);

            Assert.AreEqual(new Fish().Name, instance.Animal.Name);
            Assert.AreEqual(new TheOnlyCar().Name, instance.Car.Name);
        }
    }

    [TestFixture]
    public class Resolve
    {
        [Test]
        public void WithRegisterAndAttemptUnnamedResolution_ResolveContainer()
        {
            var container = new DependencyContainer();

            container.Register(typeof(Shark));

            var resolveOptions = new DependencyContainerResolveOptions
            {
                NamedResolutionFailureAction = DependencyContainerNamedResolutionFailureActions.AttemptUnnamedResolution
            };

            Assert.IsNotNull(container.Resolve(
                typeof(Shark), new Shark().GetName(), new Dictionary<string, object>(), resolveOptions));
        }

        [Test]
        public void WithInvalidTypeAndAttemptUnnamedResolution_ThrowsDependencyContainerResolutionException()
        {
            var container = new DependencyContainer();

            container.Register(typeof(Human));

            var resolveOptions = new DependencyContainerResolveOptions
            {
                NamedResolutionFailureAction = DependencyContainerNamedResolutionFailureActions.AttemptUnnamedResolution
            };

            Assert.Throws<DependencyContainerResolutionException>(() =>
                container.Resolve(typeof(Human), "B. B. King", new Dictionary<string, object>(), resolveOptions));
        }
        
        [Test]
        public void WithInterface_ThrowsDependencyContainerResolutionException()
        {
            var container = new DependencyContainer();

            Assert.Throws<DependencyContainerResolutionException>(() =>
                container.Resolve<IAnimal>());
        }

        [Test]
        public void WithStringAndInvalidType_ThrowsDependencyContainerResolutionException()
        {
            var container = new DependencyContainer();

            Assert.Throws<DependencyContainerResolutionException>(() =>
                container.Resolve<IAnimal>("Jim Morrison"));
        }

        [Test]
        public void WithStringAndInvalidTypeAsParam_ThrowsDependencyContainerResolutionException()
        {
            var container = new DependencyContainer();

            Assert.Throws<DependencyContainerResolutionException>(() =>
                container.Resolve(typeof(IAnimal)));
        }

        [Test]
        public void WithDictionaryAndString_ThrowsDependencyContainerResolutionException()
        {
            var container = new DependencyContainer();

            Assert.Throws<DependencyContainerResolutionException>(() =>
                container.Resolve<IAnimal>("Carlos Santana", new Dictionary<string, object>()));
        }
    }

    [TestFixture]
    public class RegisterMultiple
    {
        [Test]
        public void WithMultipleTypes_RegisterMultipleTypes()
        {
            var container = new DependencyContainer();

            Assert.IsNotNull(container.RegisterMultiple<IAnimal>(new[] {typeof(Monkey), typeof(Fish)}));
        }

        [Test]
        public void WithMultipleTypesAsMultiInstance_RegisterMultipleTypes()
        {
            var container = new DependencyContainer();

            Assert.IsNotNull(
                container.RegisterMultiple<IAnimal>(new[] {typeof(Monkey), typeof(Fish)}).AsMultiInstance());
        }

        [Test]
        public void WithMultipleTypesAsSingleton_RegisterMultipleTypes()
        {
            var container = new DependencyContainer();

            Assert.IsNotNull(container.RegisterMultiple<IAnimal>(new[] {typeof(Monkey), typeof(Fish)}).AsSingleton());
        }

        [Test]
        public void WithOnlyOneType_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var container = new DependencyContainer();
                container.RegisterMultiple(typeof(IAnimal), null);
            });
        }

        [Test]
        public void WithInvalidType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var container = new DependencyContainer();
                container.RegisterMultiple<IAnimal>(new[] {typeof(TheOnlyCar), typeof(Fish)});
            });
        }

        [Test]
        public void WithDuplicatedTypes_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var container = new DependencyContainer();
                container.RegisterMultiple<IAnimal>(new[] {typeof(Monkey), typeof(Monkey)});
            });
        }
    }

    [TestFixture]
    public class TryResolve
    {
        [TestCase(typeof(IAnimal), false)]
        [TestCase(typeof(Shark), true)]
        public void WithType_ResolveType(Type resolveType, bool expected)
        {
            var container = new DependencyContainer();

            Assert.AreEqual(expected, container.TryResolve(resolveType, out var obj));
        }

        [TestCase(typeof(Shark), true)]
        [TestCase(typeof(IAnimal), false)]
        public void WithTypeAndResolveOptions_ResolveType(Type resolveType, bool expected)
        {
            var container = new DependencyContainer();

            Assert.AreEqual(expected, container.TryResolve(
                resolveType, DependencyContainerResolveOptions.Default, out var obj));
        }

        [TestCase(typeof(Shark), "", true)]
        [TestCase(typeof(Shark), "Warsong", false)]
        [TestCase(typeof(IAnimal), "", false)]
        [TestCase(typeof(IAnimal), "Warsong", false)]
        public void WithTypeAndString_ResolveType(Type resolveType, string name, bool expected)
        {
            var container = new DependencyContainer();

            Assert.AreEqual(expected, container.TryResolve(
                resolveType, name, out var obj));
        }

        [TestCase(typeof(Shark), "", true)]
        [TestCase(typeof(Shark), "Warsong", false)]
        [TestCase(typeof(IAnimal), "", false)]
        [TestCase(typeof(IAnimal), "Warsong", false)]
        public void WithTypeAndStringAndResolveOptions_ResolveType(Type resolveType, string name, bool expected)
        {
            var container = new DependencyContainer();

            Assert.AreEqual(expected, container.TryResolve(
                resolveType, name, DependencyContainerResolveOptions.Default, out var obj));
        }

        [TestCase(typeof(Shark), true)]
        [TestCase(typeof(IAnimal), false)]
        public void WithObjectAndDictionary_ResolveType(Type resolveType, bool expected)
        {
            var container = new DependencyContainer();

            Assert.AreEqual(expected, container.TryResolve(
                resolveType, new Dictionary<string, object>(), out var obj));
        }

        [TestCase(typeof(Human), typeof(Human), false)]
        [TestCase(typeof(Shark), typeof(Shark), true)]
        [TestCase(typeof(IAnimal), typeof(Shark), false)]
        [TestCase(typeof(ICar), typeof(Shark), false)]
        [TestCase(typeof(MyEnum), typeof(Shark),false)]
        public void WithTypeAndParent_ResolveType(Type resolveType, Type register, bool expected)
        {
            var containerParent = new DependencyContainer();
            containerParent.Register(register);

            var container = containerParent.GetChildContainer();

            Assert.AreEqual(expected, container.TryResolve(
                resolveType, out var obj));
        }
        
        [TestCase(typeof(IAnimal), "Warsong", false)]
        [TestCase(typeof(Shark), "", true)]
        [TestCase(typeof(Shark), "Warsong", false)]
        public void WithTypeAndStringAndDictionary_ResolveType(Type resolveType, string name, bool expected)
        {
            var container = new DependencyContainer();

            Assert.AreEqual(expected, container.TryResolve(
                resolveType, name, new Dictionary<string, object>(), out var obj));
        }

        [TestCase(typeof(Shark), true)]
        [TestCase(typeof(IAnimal), false)]
        public void WithObjectAndDictionaryAndResolveOptions_ResolveType(Type resolveType, bool expected)
        {
            var container = new DependencyContainer();

            Assert.AreEqual(expected, container.TryResolve(
                resolveType, new Dictionary<string, object>(), DependencyContainerResolveOptions.Default, out var obj));
        }

        [TestCase(typeof(IAnimal), "Warsong", false)]
        [TestCase(typeof(Shark), "", true)]
        [TestCase(typeof(Shark), "Warsong", false)]
        public void WithStringAndDictionaryAndResolveOptions_ResolveType(Type resolveType, string name, bool expected)
        {
            var container = new DependencyContainer();

            Assert.AreEqual(expected, container.TryResolve(
                resolveType, name, new Dictionary<string, object>(), DependencyContainerResolveOptions.Default, out var obj));
        }

        [Test]
        public void WithRegister_ResolveType()
        {
            var container = new DependencyContainer();
            container.Register<IAnimal, Fish>();

            Assert.IsTrue(container.TryResolve(out IAnimal instance));
            Assert.AreEqual(new Fish().Name, instance.Name);
        }

        [Test]
        public void WithoutRegister_FailResolveType()
        {
            var container = new DependencyContainer();

            Assert.IsFalse(container.TryResolve(out IAnimal instance));
        }

        [Test]
        public void WithRegisterAndResolveOptions_ResolveType()
        {
            var container = new DependencyContainer();
            container.Register<IAnimal, Fish>();

            Assert.IsTrue(container.TryResolve(
                DependencyContainerResolveOptions.Default, out IAnimal instance));
            Assert.AreEqual(new Fish().Name, instance.Name);
        }

        [Test]
        public void WithoutRegisterAndWithResolveOptions_FailResolveType()
        {
            var container = new DependencyContainer();

            Assert.IsFalse(container.TryResolve(
                DependencyContainerResolveOptions.Default, out IAnimal instance));
        }

        [TestCase("", true)]
        [TestCase("Warsong", false)]
        public void WithRegisterAndEmptyString_ResolveType(string name, bool expected)
        {
            var container = new DependencyContainer();
            container.Register<IAnimal, Fish>();

            Assert.AreEqual(expected, container.TryResolve(name, out IAnimal instance));
        }

        [Test]
        public void WithoutRegisterAndWithString_FailResolveType()
        {
            var container = new DependencyContainer();

            Assert.IsFalse(container.TryResolve(
                new Shark().Name, out IAnimal instance));
        }

        [Test]
        public void WithoutRegisterAndWithStringAndResolveOptions_FailResolveType()
        {
            var container = new DependencyContainer();

            Assert.IsFalse(container.TryResolve(
                new Shark().Name, DependencyContainerResolveOptions.Default, out IAnimal instance));
        }

        [TestCase("", true)]
        [TestCase("Warsong", false)]
        public void WithRegisterAndStringAndResolveOptions_FailResolveType(string name, bool expected)
        {
            var container = new DependencyContainer();
            container.Register<IAnimal, Fish>();

            Assert.AreEqual(expected, container.TryResolve(
                name, DependencyContainerResolveOptions.Default, out IAnimal instance));
        }

        [Test]
        public void WithDictionaryAndValidType_ResolveType()
        {
            var container = new DependencyContainer();

            Assert.IsTrue(container.TryResolve<Fish>(
                new Dictionary<string, object>(), out var instance));
        }

        [Test]
        public void WithDictionaryAndInvalidType_FailResolveType()
        {
            var container = new DependencyContainer();

            Assert.IsFalse(container.TryResolve<IAnimal>(
                new Dictionary<string, object>(), out var instance));
        }

        [TestCase("", false)]
        [TestCase("Warsong", false)]
        public void WithStringAndWithDictionary_ResolveType(string name, bool expected)
        {
            var container = new DependencyContainer();

            Assert.AreEqual(expected, container.TryResolve(
                name, new Dictionary<string, object>(), out Human instance));
        }

        [TestCase("", false)]
        [TestCase("Warsong", false)]
        public void WithRegisterAndStringAndDictionary_ResolveType(string name, bool expected)
        {
            var container = new DependencyContainer();

            container.Register(typeof(Human), name);

            Assert.AreEqual(expected, container.TryResolve(
                name, new Dictionary<string, object>(), out Human instance));
        }

        [Test]
        public void WithInvalidTypeAndDictionaryAndResolveOptions_FailResolveType()
        {
            var container = new DependencyContainer();

            Assert.IsFalse(container.TryResolve(
                new Dictionary<string, object>(), DependencyContainerResolveOptions.Default, out IAnimal instance));
        }

        [Test]
        public void WithValidTypeAndDictionaryAndResolveOptions_ResolveType()
        {
            var container = new DependencyContainer();

            Assert.IsTrue(container.TryResolve(
                new Dictionary<string, object>(), DependencyContainerResolveOptions.Default, out Fish instance));
        }

        [TestCase("", true)]
        [TestCase("Warsong", false)]
        public void WithResolveOptionsAndStringAndDictionary_ResolveType(string name, bool expected)
        {
            var container = new DependencyContainer();

            Assert.AreEqual(expected, container.TryResolve(
                name, new Dictionary<string, object>(), DependencyContainerResolveOptions.Default, out Shark instance));
        }

        [TestCase("", true)]
        [TestCase("Warsong", false)]
        public void WithStringAndDictionary_ResolveType(string name, bool expected)
        {
            var container = new DependencyContainer();

            Assert.AreEqual(expected, container.TryResolve(
                name, new Dictionary<string, object>(), out Shark instance));
        }
    }

    [TestFixture]
    public class Register
    {
        [Test]
        public void WithInterface_RegisterInterface()
        {
            var container = new DependencyContainer();

            container.Register<IAnimal, Fish>();

            Assert.IsNotNull(container.Resolve<IAnimal>());
            Assert.AreEqual(new Fish().Name, container.Resolve<IAnimal>().Name);

            container.Unregister<IAnimal>();
            Assert.Throws<DependencyContainerResolutionException>(() => container.Resolve<IAnimal>());
        }

        [Test]
        public void WithController_RegisterController()
        {
            var container = new DependencyContainer();
            Assert.IsNotNull(container.Register<Controller>());
        }

        [Test]
        public void RegisterInterfaceWithInstance_CanResolve()
        {
            var container = new DependencyContainer();
            var instance = new Human("George");

            container.Register<IAnimal, Human>(instance);
            Assert.IsTrue(container.TryResolve(out IAnimal containerInstance));
            Assert.AreEqual(instance.Name, containerInstance.Name);
        }

        [Test]
        public void RegisterInterfaceWithInstanceAndWeakReference_CanDestroy()
        {
            var container = new DependencyContainer();
            using (var instance = new Human("George"))
            {
                container.Register<IAnimal, Human>(instance).WithWeakReference();
            }

            var containerInstance = (Human) container.Resolve<IAnimal>();
            Assert.IsTrue(containerInstance.IsDisposed);
        }

        [Test]
        public void RegisterInterfaceAndWeakReference_ThrowsDependencyContainerRegistrationException()
        {
            var container = new DependencyContainer();

            Assert.Throws<DependencyContainerRegistrationException>(() =>
                container.Register<IAnimal, Fish>().WithWeakReference());
        }
        
        [Test]
        public void RegisterInterfaceWithInstanceStrongReference_CanDestroy()
        {
            var container = new DependencyContainer();
            using (var instance = new Human("George"))
            {
                container.Register<IAnimal>(instance).WithStrongReference();
            }

            var containerInstance = (Human) container.Resolve<IAnimal>();
            Assert.IsTrue(containerInstance.IsDisposed);
        }

        [Test]
        public void WithoutInstanceAndWithStrongReference_ThrowsDependencyContainerRegistrationException()
        {
            var container = new DependencyContainer();

            Assert.Throws<DependencyContainerRegistrationException>(() =>
                container.Register<IAnimal, Fish>().WithStrongReference());
        }

        [TestCase(typeof(IAnimal))]
        [TestCase(typeof(Dictionary<string, string>))]
        public void WithInvalidRegisterImplementation_ThrowsDependencyContainerRegistrationException(
            Type registerImplementation)
        {
            var container = new DependencyContainer();
            Assert.Throws<DependencyContainerRegistrationException>(() =>
                container.Register(typeof(IAnimal), registerImplementation).AsSingleton());
        }

        [Test]
        public void WithTypeAsSingleton_ResolveThrowsDependencyContainerResolutionException()
        {
            var container = new DependencyContainer();

            container.Register(typeof(string)).AsSingleton();

            Assert.Throws<DependencyContainerResolutionException>(() => container.Resolve<IAnimal>());
        }

        [TestCase(typeof(IAnimal))]
        [TestCase(typeof(Fish))]
        public void WithRegisterTypeAndInvalidRegisterImplementation_ThrowsDependencyContainerRegistrationException(
            Type registerImplementation)
        {
            var container = new DependencyContainer();
            Assert.Throws<DependencyContainerRegistrationException>(() => container.Register(
                    typeof(Shark), registerImplementation).AsMultiInstance());
        }

        [Test]
        public void WithInvalidTypeAsMultiInstance_ThrowsDependencyContainerResolutionException()
        {
            var container = new DependencyContainer();

            container.Register(typeof(TheOnlyCar)).AsMultiInstance();

            Assert.Throws<DependencyContainerResolutionException>(() => container.Resolve<IAnimal>());
        }

        [Test]
        public void WithRegisterImplementationAsSingleton_ThrowsDependencyContainerRegistrationException()
        {
            var container = new DependencyContainer();

            Assert.Throws<DependencyContainerRegistrationException>(() =>
                container.Register<IAnimal, Shark>(new Shark()).AsSingleton());
        }

        [Test]
        public void WithRegisterImplementationAsMultiInstance_ThrowsDependencyContainerResolutionException()
        {
            var container = new DependencyContainer();

            Assert.Throws<DependencyContainerRegistrationException>(() =>
                container.Register((di, param) => new Human(param["Name"].ToString())).AsMultiInstance());
        }

        [Test]
        public void WithNullFactory_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var container = new DependencyContainer();

                container.Register((Func<DependencyContainer, Dictionary<string, object>, IAnimal>) null);
            });
        }

        [Test]
        public void WithFunc_ReturnsOptions()
        {
            var container = new DependencyContainer();

            container.Register<IAnimal>((di, param) => new Human(param["Name"].ToString()));

            Assert.IsNotNull(container);
        }

        [TestCase(typeof(IEnumerator<>))]
        [TestCase(typeof(DependencyContainerTest<>))]
        public void RegisterTypeWithInstance_ThrowsDependencyContainerRegistrationException(Type registerType)
        {
            var container = new DependencyContainer();
            var instance = new Human("George");

            Assert.Throws<DependencyContainerRegistrationException>(() => container.Register(
                    registerType.GetGenericTypeDefinition(), typeof(string), instance));
        }
    }
    
    [TestFixture]
    public class Dispose
    {
        [Test]
        public void WithRegisteredInterface_DisposeContainer()
        {
            var container = new DependencyContainer();
            var instance = new Human("George");
            container.Register<IAnimal>(instance);
            container.Dispose();
            Assert.IsTrue(instance.IsDisposed);
        }

        [Test]
        public void WithRegisteredInterfaceAndContainerParent_DisposeContainer()
        {
            var instance = new Human("George");

            var containerParent = new DependencyContainer();
            containerParent.Register<IAnimal>(instance);

            var container = containerParent.GetChildContainer();
            container.Register<IAnimal>(instance);

            containerParent.Dispose();

            Assert.IsTrue(instance.IsDisposed);
        }
    }

    [TestFixture]
    public class CanResolve
    {
        [TestCase("Turalyon", "Turalyon")]
        [TestCase("", "Alleria")]
        public void WithInstanceAndRegister_ResolveContainer(string registerName, string resolveName)
        {
            var container = new DependencyContainer();

            var resolveOptions = new DependencyContainerResolveOptions
            {
                NamedResolutionFailureAction = DependencyContainerNamedResolutionFailureActions.AttemptUnnamedResolution
            };

            container.Register<IAnimal>(new Human("George"), registerName);
            
            Assert.IsTrue(container.CanResolve<IAnimal>(resolveName, null, resolveOptions));
        }
        
        [Test]
        public void WithIEnumerable_ResolveContainer()
        {
            var container = new DependencyContainer();

            Assert.IsTrue(container.CanResolve<IEnumerable<string>>());
            container.Register<IEnumerable<string>, StringEnumerable>();

            Assert.AreEqual(typeof(StringEnumerable), container.Resolve<IEnumerable<string>>().GetType());
        }

        [Test]
        public void WithDictionary_ResolveContainer()
        {
            var container = new DependencyContainer();

            Assert.IsTrue(container.CanResolve<Shark>(new Dictionary<string, object>()));
        }

        [Test]
        public void WithFailUnregisteredResolutionActions_FailResolveContainer()
        {
            var container = new DependencyContainer();

            var resolveOptions = new DependencyContainerResolveOptions
            {
                UnregisteredResolutionAction = DependencyContainerUnregisteredResolutionActions.Fail
            };

            Assert.IsFalse(container.CanResolve<Shark>(new Dictionary<string, object>(), resolveOptions));
        }

        [Test]
        public void WithoutRegister_FailResolveContainer()
        {
            var container = new DependencyContainer();

            Assert.IsFalse(container.CanResolve<Shark>(new Shark().Name));
        }

        [Test]
        public void WithRegister_ResolveContainer()
        {
            var container = new DependencyContainer();
            container.Register(typeof(Shark),new Shark().Name);

            Assert.IsTrue(container.CanResolve<Shark>(new Shark().Name));
        }

        [Test]
        public void WithRegisterAndAttemptUnnamedResolution_ResolveContainer()
        {
            var container = new DependencyContainer();

            container.Register(typeof(Shark));

            var resolveOptions = new DependencyContainerResolveOptions
            {
                NamedResolutionFailureAction = DependencyContainerNamedResolutionFailureActions.AttemptUnnamedResolution
            };

            Assert.IsTrue(container.CanResolve(
                typeof(Shark), new Dictionary<string, object>(), new Shark().Name, resolveOptions));
        }

        [TestCase(typeof(Func<>))]
        [TestCase(typeof(Func<string, int>))]
        [TestCase(typeof(Func<string, IDictionary<string, object>, int>))]
        public void WithType_ResolveContainer(Type resolveType)
        {
            var container = new DependencyContainer();

            Assert.IsTrue(container.CanResolve(resolveType, new Dictionary<string, object>()));
        }
    }

    [TestFixture]
    public class ResolveAll
    {
        [TestCase(false)]
        [TestCase(true)]
        public void WithType_ResolveAll(bool includeUnnamed)
        {
            var container = new DependencyContainer();
            container.Register(typeof(Fish), typeof(Shark), new Shark().GetName());
            container.Register(typeof(Fish), typeof(Clown));
            container.Register(typeof(Fish), typeof(Shark), "Kisame");

            var resolve = container.ResolveAll<Fish>(includeUnnamed);

            Assert.IsTrue(resolve.Any(x => x.GetType() == typeof(Shark)));
            Assert.AreEqual(includeUnnamed, resolve.Any(x => x.GetType() == typeof(Clown)));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void WithTypeAsParamAndWithParent_ResolveAll(bool includeUnnamed)
        {
            var containerParent = new DependencyContainer();
            containerParent.Register(typeof(Fish), typeof(Shark), new Shark().GetName());
            containerParent.Register(typeof(Fish), typeof(Shark), "Kisame");
            containerParent.Register(typeof(Fish), typeof(Clown));

            var container = containerParent.GetChildContainer();

            var resolve = container.ResolveAll<Fish>(includeUnnamed);
            
            Assert.IsTrue(resolve.Any(x => x.GetType() == typeof(Shark)));
            Assert.AreEqual(includeUnnamed, resolve.Any(x => x.GetType() == typeof(Clown)));
        }
    }
}