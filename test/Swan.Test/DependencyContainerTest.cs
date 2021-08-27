using NUnit.Framework;
using Swan.DI;
using Swan.Test.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Swan.Test.DependencyContainerTest
{
    [TestFixture]
    public class AutoRegister
    {
        [Test]
        public void WithFailDependencyContainer_ThrowsDependencyContainerRegistrationException()
        {
            var container = new DependencyContainer();

            Assert.Throws<DependencyContainerRegistrationException>(() =>
                container.AutoRegister(DependencyContainerDuplicateImplementationAction.Fail));
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

        [Test]
        public void WithAssemblies_ResolvesIAnimal()
        {
            var container = new DependencyContainer();
            container.AutoRegister(AppDomain.CurrentDomain.GetAssemblies());

            Assert.IsTrue(container.CanResolve<ICar>());
        }

        [Test]
        public void WithAssembliesAndFunc_ResolvesICar()
        {
            var container = new DependencyContainer();

            container.AutoRegister(
                AppDomain.CurrentDomain.GetAssemblies(),
                DependencyContainerDuplicateImplementationAction.RegisterSingle,
                param => true);

            Assert.IsTrue(container.CanResolve<ICar>());
        }

        [Test]
        public void WithAssembliesAndRegisterMultiple_ResolvesICar()
        {
            var container = new DependencyContainer();

            container.AutoRegister(
                AppDomain.CurrentDomain.GetAssemblies(),
                DependencyContainerDuplicateImplementationAction.RegisterMultiple);

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
                NamedResolutionFailureAction = DependencyContainerNamedResolutionFailureAction.AttemptUnnamedResolution,
            };

            Assert.IsNotNull(container.Resolve(
                typeof(Shark), new Shark().GetName(), resolveOptions));
        }

        [Test]
        public void WithInvalidTypeAndAttemptUnnamedResolution_ThrowsDependencyContainerResolutionException()
        {
            var container = new DependencyContainer();

            container.Register(typeof(Human));

            var resolveOptions = new DependencyContainerResolveOptions
            {
                NamedResolutionFailureAction = DependencyContainerNamedResolutionFailureAction.AttemptUnnamedResolution,
            };

            Assert.Throws<DependencyContainerResolutionException>(() =>
                container.Resolve(typeof(Human), "B. B. King", resolveOptions));
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
        public void WithName_ThrowsDependencyContainerResolutionException()
        {
            var container = new DependencyContainer();

            Assert.Throws<DependencyContainerResolutionException>(() =>
                container.Resolve<IAnimal>(nameof(IAnimal)));
        }
    }

    [TestFixture]
    public class RegisterMultiple
    {
        [Test]
        public void WithMultipleTypes_RegisterMultipleTypes()
        {
            var container = new DependencyContainer();

            Assert.IsNotNull(container.RegisterMultiple<IAnimal>(new[] { typeof(Monkey), typeof(Fish) }));
        }

        [Test]
        public void WithMultipleTypesAsMultiInstance_RegisterMultipleTypes()
        {
            var container = new DependencyContainer();

            Assert.IsNotNull(
                container.RegisterMultiple<IAnimal>(new[] { typeof(Monkey), typeof(Fish) }).AsMultiInstance());
        }

        [Test]
        public void WithMultipleTypesAsSingleton_RegisterMultipleTypes()
        {
            var container = new DependencyContainer();

            Assert.IsNotNull(container.RegisterMultiple<IAnimal>(new[] { typeof(Monkey), typeof(Fish) }).AsSingleton());
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
                container.RegisterMultiple<IAnimal>(new[] { typeof(TheOnlyCar), typeof(Fish) });
            });
        }

        [Test]
        public void WithDuplicatedTypes_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var container = new DependencyContainer();
                container.RegisterMultiple<IAnimal>(new[] { typeof(Monkey), typeof(Monkey) });
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

            Assert.AreEqual(expected, container.TryResolve(resolveType, out _));
        }

        [TestCase(typeof(Shark), "", true)]
        [TestCase(typeof(Shark), "Mario", false)]
        [TestCase(typeof(IAnimal), "", false)]
        [TestCase(typeof(IAnimal), "Mario", false)]
        public void WithTypeAndString_ResolveType(Type resolveType, string name, bool expected)
        {
            var container = new DependencyContainer();

            Assert.AreEqual(expected, container.TryResolve(resolveType, name, out _));
        }

        [TestCase(typeof(Shark), true)]
        [TestCase(typeof(IAnimal), false)]
        public void WithObject_ResolveType(Type resolveType, bool expected)
        {
            var container = new DependencyContainer();

            Assert.AreEqual(expected, container.TryResolve(resolveType, out _));
        }

        [TestCase(typeof(Human), typeof(Human), false)]
        [TestCase(typeof(Shark), typeof(Shark), true)]
        [TestCase(typeof(IAnimal), typeof(Shark), false)]
        [TestCase(typeof(ICar), typeof(Shark), false)]
        [TestCase(typeof(MyEnum), typeof(Shark), false)]
        public void WithTypeAndParent_ResolveType(Type resolveType, Type register, bool expected)
        {
            var containerParent = new DependencyContainer();
            containerParent.Register(register);

            var container = containerParent.GetChildContainer();

            Assert.AreEqual(expected, container.TryResolve(
                resolveType, out _));
        }

        [TestCase(typeof(Shark), true)]
        [TestCase(typeof(IAnimal), false)]
        public void WithObjectAndResolveOptions_ResolveType(Type resolveType, bool expected)
        {
            var container = new DependencyContainer();

            Assert.AreEqual(expected, container.TryResolve(
                resolveType, DependencyContainerResolveOptions.Default, out _));
        }

        [TestCase(typeof(IAnimal), "Mario", false)]
        [TestCase(typeof(Shark), "", true)]
        [TestCase(typeof(Shark), "Mario", false)]
        public void WithStringAndResolveOptions_ResolveType(Type resolveType, string name, bool expected)
        {
            var container = new DependencyContainer();

            Assert.AreEqual(expected, container.TryResolve(
                resolveType, name, DependencyContainerResolveOptions.Default, out _));
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
        [TestCase("Mario", false)]
        public void WithRegisterAndEmptyString_ResolveType(string name, bool expected)
        {
            var container = new DependencyContainer();
            container.Register<IAnimal, Fish>();

            Assert.AreEqual(expected, container.TryResolve(name, out IAnimal _));
        }

        [Test]
        public void WithoutRegisterAndWithString_FailResolveType()
        {
            var container = new DependencyContainer();

            Assert.IsFalse(container.TryResolve(
                new Shark().Name, out IAnimal _));
        }

        [Test]
        public void WithoutRegisterAndWithStringAndResolveOptions_FailResolveType()
        {
            var container = new DependencyContainer();

            Assert.IsFalse(container.TryResolve(
                new Shark().Name, DependencyContainerResolveOptions.Default, out IAnimal _));
        }

        [TestCase("", true)]
        [TestCase("Mario", false)]
        public void WithRegisterAndStringAndResolveOptions_FailResolveType(string name, bool expected)
        {
            var container = new DependencyContainer();
            container.Register<IAnimal, Fish>();

            Assert.AreEqual(expected, container.TryResolve(
                name, DependencyContainerResolveOptions.Default, out IAnimal _));
        }

        [Test]
        public void WithValidType_ResolveType()
        {
            var container = new DependencyContainer();

            Assert.IsTrue(container.TryResolve<Fish>(out _));
        }

        [Test]
        public void WithInvalidType_FailResolveType()
        {
            var container = new DependencyContainer();

            Assert.IsFalse(container.TryResolve<IAnimal>(out _));
        }

        [TestCase("", false)]
        [TestCase("Mario", false)]
        public void WithString_ResolveType(string name, bool expected)
        {
            var container = new DependencyContainer();

            Assert.AreEqual(expected, container.TryResolve(name, out Human _));
        }

        [TestCase("", false)]
        [TestCase("Mario", false)]
        public void WithRegisterAndString_ResolveType(string name, bool expected)
        {
            var container = new DependencyContainer();

            container.Register(typeof(Human), name);

            Assert.AreEqual(expected, container.TryResolve(name, out Human _));
        }

        [Test]
        public void WithInvalidTypeAndResolveOptions_FailResolveType()
        {
            var container = new DependencyContainer();

            Assert.IsFalse(container.TryResolve(DependencyContainerResolveOptions.Default, out IAnimal _));
        }

        [Test]
        public void WithValidTypeAndResolveOptions_ResolveType()
        {
            var container = new DependencyContainer();

            Assert.IsTrue(container.TryResolve(DependencyContainerResolveOptions.Default, out Fish _));
        }

        [TestCase("", true)]
        [TestCase("Mario", false)]
        public void WithResolveOptionsAndString_ResolveType(string name, bool expected)
        {
            var container = new DependencyContainer();

            Assert.AreEqual(expected, container.TryResolve(name, DependencyContainerResolveOptions.Default, out Shark _));
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

            var containerInstance = (Human)container.Resolve<IAnimal>();
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

            var containerInstance = (Human)container.Resolve<IAnimal>();
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

                container.Register((Func<DependencyContainer, Dictionary<string, object>, IAnimal>)null);
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
        [TestCase(typeof(IList<>))]
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
        [TestCase("Babu", "Babu")]
        [TestCase("", "Alleria")]
        public void WithInstanceAndRegister_ResolveContainer(string registerName, string resolveName)
        {
            var container = new DependencyContainer();

            var resolveOptions = new DependencyContainerResolveOptions
            {
                NamedResolutionFailureAction = DependencyContainerNamedResolutionFailureAction.AttemptUnnamedResolution,
            };

            container.Register<IAnimal>(new Human("George"), registerName);

            Assert.IsTrue(container.CanResolve<IAnimal>(resolveName, resolveOptions));
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
        public void WithoutRegister_FailResolveContainer()
        {
            var container = new DependencyContainer();

            Assert.IsFalse(container.CanResolve<Shark>(new Shark().Name));
        }

        [Test]
        public void WithRegister_ResolveContainer()
        {
            var container = new DependencyContainer();
            container.Register(typeof(Shark), new Shark().Name);

            Assert.IsTrue(container.CanResolve<Shark>(new Shark().Name));
        }

        [Test]
        public void WithRegisterAndAttemptUnnamedResolution_ResolveContainer()
        {
            var container = new DependencyContainer();

            container.Register(typeof(Shark));

            var resolveOptions = new DependencyContainerResolveOptions
            {
                NamedResolutionFailureAction = DependencyContainerNamedResolutionFailureAction.AttemptUnnamedResolution,
            };

            Assert.IsTrue(container.CanResolve(typeof(Shark), new Shark().Name, resolveOptions));
        }

        [TestCase(typeof(Func<>))]
        [TestCase(typeof(Func<string, int>))]
        [TestCase(typeof(Func<string, IDictionary<string, object>, int>))]
        public void WithType_ResolveContainer(Type resolveType)
        {
            var container = new DependencyContainer();

            Assert.IsTrue(container.CanResolve(resolveType));
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
            container.Register(typeof(Fish), typeof(Shark), "Geo");

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
            containerParent.Register(typeof(Fish), typeof(Shark), "Geo");
            containerParent.Register(typeof(Fish), typeof(Clown));

            var container = containerParent.GetChildContainer();

            var resolve = container.ResolveAll<Fish>(includeUnnamed);

            Assert.IsTrue(resolve.Any(x => x.GetType() == typeof(Shark)));
            Assert.AreEqual(includeUnnamed, resolve.Any(x => x.GetType() == typeof(Clown)));
        }
    }
}