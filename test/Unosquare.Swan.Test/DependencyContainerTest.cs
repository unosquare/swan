using System;
using NUnit.Framework;
using System.Collections.Generic;
using Unosquare.Swan.Components;
using Unosquare.Swan.Exceptions;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test.DependencyContainerTest
{
    public abstract class DependencyContainerTest
    {
        protected Dictionary<string, object> NameDictionary = new Dictionary<string, object> {
            { "Name", "Thrall" }, { "Race", "Orc" }, { "Affiliation", "Horde" } };
    }
    
    [TestFixture]
    public class AutoRegister
    {

#if !NETSTANDARD1_3 && !UWP
        [Test]
        public void WithNoParams_ThrowsDependencyContainerResolutionException()
        {
            Assert.Throws<DependencyContainerResolutionException>(() =>
            {
                var container = new DependencyContainer();
                container.AutoRegister();
                Assert.IsTrue(container.CanResolve<ICar>());
                Assert.AreEqual((new TheOnlyCar()).Name, Runtime.Container.Resolve<ICar>().Name);
            });
        }

        [Test]
        public void WithFailDependencyContainer_ThrowsDependencyContainerRegistrationException()
        {
            Assert.Throws<DependencyContainerRegistrationException>(() =>
            {
                var container = new DependencyContainer();
                container.AutoRegister(DependencyContainerDuplicateImplementationActions.Fail);
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

            Assert.AreEqual((new Fish()).Name, instance.Animal.Name);
            Assert.AreEqual((new TheOnlyCar()).Name, instance.Car.Name);
        }
    }

    [TestFixture]
    public class Resolve : DependencyContainerTest
    {
        [Test]
        public void WithInterface_ThrowsDependencyContainerResolutionException()
        {
            if(Runtime.Container.CanResolve<IAnimal>())
                Runtime.Container.Unregister<IAnimal>();

            Assert.Throws<DependencyContainerResolutionException>(() => 
                Runtime.Container.Resolve<IAnimal>()
            );
        }
        
        [Test]
        public void WithStringAndInvalidType_ThrowsDependencyContainerResolutionException()
        {
            var container = new DependencyContainer();
            
            Assert.Throws<DependencyContainerResolutionException>(() =>
                container.Resolve<Dictionary<string, object>>((new Shark()).Name)
            );
        }

        [Test]
        public void WithStringAndInvalidTypeAsParam_ThrowsDependencyContainerResolutionException()
        {
            var container = new DependencyContainer();
            
            Assert.Throws<DependencyContainerResolutionException>(() =>
                container.Resolve(typeof(string), (new Shark()).Name)
            );
        }

        [Test]
        public void WithDictionaryAndString_ThrowsDependencyContainerResolutionException()
        {
            var container = new DependencyContainer();
            
            Assert.Throws<DependencyContainerResolutionException>(() =>
                container.Resolve<string>((new Shark()).Name, NameDictionary)
            );
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
            
            Assert.IsNotNull(container.RegisterMultiple<IAnimal>(new[] { typeof(Monkey), typeof(Fish) }).AsMultiInstance());
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
                Assert.IsNotNull(container.RegisterMultiple(typeof(IAnimal), null));
            });
        }

        [Test]
        public void WithInvalidType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var container = new DependencyContainer();
                Assert.IsNotNull(container.RegisterMultiple<IAnimal>(new[] { typeof(TheOnlyCar), typeof(Fish) }));
            });
        }

        [Test]
        public void WithDuplicatedTypes_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var container = new DependencyContainer();
                Assert.IsNotNull(container.RegisterMultiple<IAnimal>(new[] { typeof(Monkey), typeof(Monkey) }));
            });
        }
    }

    [TestFixture]
    public class TryResolve : DependencyContainerTest
    {
        [TestCase(typeof(string), false)]
        [TestCase(typeof(Dictionary<string, string>), true)]
        public void WithType_ResolveType(Type resolveType, bool expected)
        {
            var container = new DependencyContainer();
            
            Assert.AreEqual(expected, container.TryResolve(resolveType, out object obj));
        }
        
        [TestCase(typeof(Dictionary<String, Object>), true)]
        [TestCase(typeof(string), false)]
        public void WithObjectAndResolveOptions_ResolveType(Type resolveType, bool expected)
        {
            var container = new DependencyContainer();
            
            Assert.AreEqual(expected, container.TryResolve(
                resolveType, DependencyContainerResolveOptions.Default, out object obj));
        }
        
        [TestCase(typeof(string), "Warsong", false)]
        [TestCase(typeof(Dictionary<string, string>), "", true)]
        [TestCase(typeof(Dictionary<string, string>), "Warsong", false)]
        public void WithTypeAndString_ResolveType(Type resolveType, string name, bool expected)
        {
            var container = new DependencyContainer();
            
            Assert.AreEqual(expected, container.TryResolve(
                resolveType, name, out object obj));
        }
        
        [TestCase(typeof(string), "Warsong", false)]
        [TestCase(typeof(Dictionary<string, string>), "", true)]
        [TestCase(typeof(Dictionary<string, string>), "Warsong", false)]
        public void WithTypeAndStringAndResolveOptions_ResolveType(Type resolveType, string name, bool expected)
        {
            var container = new DependencyContainer();
            
            Assert.AreEqual(expected, container.TryResolve(
                resolveType, name, DependencyContainerResolveOptions.Default, out object obj));
        }

        [TestCase(typeof(Dictionary<String, Object>), true)]
        [TestCase(typeof(string), false)]
        public void WithObjectAndDictionary_ResolveType(Type resolveType, bool expected)
        {
            var container = new DependencyContainer();

            Assert.AreEqual(expected, container.TryResolve(
                resolveType, NameDictionary, out object obj));
        }

        [TestCase(typeof(Dictionary<String, Object>), true)]
        [TestCase(typeof(string), false)]
        [TestCase(typeof(int), false)]
        [TestCase(typeof(IEnumerable<>), false)]
        public void WithTypeAndParent_ResolveType(Type resolveType, bool expected)
        {
            var containerParent = new DependencyContainer();
            containerParent.Register(typeof(string));
            
            var container = containerParent.GetChildContainer();

            Assert.AreEqual(expected, container.TryResolve(
                resolveType, out object obj));
        }

        [TestCase(typeof(string), "Warsong", false)]
        [TestCase(typeof(Dictionary<string, string>), "", true)]
        [TestCase(typeof(Dictionary<string, string>), "Warsong", false)]
        public void WithTypeAndStringAndDictionary_ResolveType(Type resolveType, string name, bool expected)
        {
            var container = new DependencyContainer();

            Assert.AreEqual(expected, container.TryResolve(
                resolveType, name, NameDictionary, out object obj));
        }

        [TestCase(typeof(Dictionary<String, Object>), true)]
        [TestCase(typeof(string), false)]
        public void WithObjectAndDictionaryAndResolveOptions_ResolveType(Type resolveType, bool expected)
        {
            var container = new DependencyContainer();

            Assert.AreEqual(expected, container.TryResolve(
                resolveType, NameDictionary, DependencyContainerResolveOptions.Default, out object obj));
        }
        
        [TestCase(typeof(string), "Warsong", false)]
        [TestCase(typeof(Dictionary<string, string>), "", true)]
        [TestCase(typeof(Dictionary<string, string>), "Warsong", false)]
        public void WithStringAndDictionaryAndResolveOptions_ResolveType(Type resolveType, string name, bool expected)
        {
            var container = new DependencyContainer();

            Assert.AreEqual(expected, container.TryResolve(
                resolveType, name, NameDictionary, 
                DependencyContainerResolveOptions.Default, out object obj));
        }

        [Test]
        public void WithRegister_ResolveType()
        {
            var container = new DependencyContainer();
            container.Register<IAnimal, Fish>();

            Assert.IsTrue(container.TryResolve(out IAnimal instance));
            Assert.AreEqual((new Fish()).Name, instance.Name);
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
            Assert.AreEqual((new Fish()).Name, instance.Name);
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
                (new Shark()).Name, out IAnimal instance));
        }

        [Test]
        public void WithoutRegisterAndWithStringAndResolveOptions_FailResolveType()
        {
            var container = new DependencyContainer();

            Assert.IsFalse(container.TryResolve(
                (new Shark()).Name, DependencyContainerResolveOptions.Default, out IAnimal instance));
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

            Assert.IsTrue(container.TryResolve<Dictionary<string, object>>(
                NameDictionary, out Dictionary<string,object> instance));
        }

        [Test]
        public void WithDictionaryAndInvalidType_FailResolveType()
        {
            var container = new DependencyContainer();

            Assert.IsFalse(container.TryResolve<string>(
                NameDictionary, out string instance));
        }

        [TestCase("", true)]
        [TestCase("Warsong", false)]
        public void WithStringAndsadWithDictionary_FailResolveType(string name, bool expected)
        {
            var container = new DependencyContainer();

            Assert.AreEqual(expected, container.TryResolve(
                name, NameDictionary, out Dictionary<string, object> instance));
        }

        [Test]
        public void WithInvalidTypeAndDictionaryAndResolveOptions_FailResolveType()
        {
            var container = new DependencyContainer();

            Assert.IsFalse(container.TryResolve(
                NameDictionary, DependencyContainerResolveOptions.Default, 
                out string instance));
        }

        [Test]
        public void WithValidTypeAndDictionaryAndResolveOptions_ResolveType()
        {
            var container = new DependencyContainer();

            Assert.IsTrue(container.TryResolve(
                NameDictionary, DependencyContainerResolveOptions.Default,
                out Dictionary<string, object> instance));
        }

        [TestCase("", true)]
        [TestCase("Warsong", false)]
        public void WithStringAndDictionaryAndResolveOptions_FailResolveType(string name, bool expected)
        {
            var container = new DependencyContainer();

            Assert.AreEqual(expected, container.TryResolve(
                name, NameDictionary, DependencyContainerResolveOptions.Default, 
                out Dictionary<string, object> instance));
        }
        
    }

    [TestFixture]
    public class Register : DependencyContainerTest
    {
        [Test]
        public void WithInterface_RegisterInterface()
        {
            var container = new DependencyContainer();

            container.Register<IAnimal, Fish>();

            Assert.IsNotNull(container.Resolve<IAnimal>());
            Assert.AreEqual((new Fish()).Name, container.Resolve<IAnimal>().Name);

            container.Unregister<IAnimal>();
            Assert.Throws<DependencyContainerResolutionException>(() =>
                container.Resolve<IAnimal>()
            );
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
            using(var instance = new Human("George"))
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
                container.Register<IAnimal, Fish>().WithWeakReference()
            );
        }

        [Test]
        public void RegisterInterfaceAndStrongReference_ThrowsDependencyContainerRegistrationException()
        {
            var container = new DependencyContainer();

            Assert.Throws<DependencyContainerRegistrationException>(() =>
                container.Register<IAnimal, Fish>().WithStrongReference()
            );
        }

        [Test]
        public void RegisterInterfaceWithInstanceStrongReference_CanDestroy()
        {
            var container = new DependencyContainer();
            using(var instance = new Human("George"))
            {
                container.Register<IAnimal>(instance).WithStrongReference();
            }

            var containerInstance = (Human)container.Resolve<IAnimal>();
            Assert.IsTrue(containerInstance.IsDisposed);
        }

        [Test]
        public void RegisterInterfaceAnsdStrongReference_ThrowsDependencyContainerRegistrationException()
        {
            var container = new DependencyContainer();

            Assert.Throws<DependencyContainerRegistrationException>(() =>
                container.Register<IAnimal, Fish>().WithStrongReference()
            );
        }
        
        [TestCase(typeof(IAnimal))]
        [TestCase(typeof(Dictionary<string, string>))]
        public void WithInvalidRegisterImplementation_ThrowsDependencyContainerRegistrationException(Type registerImplementation)
        {
            var container = new DependencyContainer();
            Assert.Throws<DependencyContainerRegistrationException>(() =>
                container.Register(typeof(IAnimal), registerImplementation).AsSingleton()
            );
        }

        [Test]
        public void WithTypeAsSingleton_ResolveThrowsDependencyContainerResolutionException()
        {
            var container = new DependencyContainer();

            container.Register(typeof(string)).AsSingleton();
            
            Assert.Throws<DependencyContainerResolutionException>(() =>
                container.Resolve<IAnimal>()
            );
        }
        
        [TestCase(typeof(IAnimal))]
        [TestCase(typeof(List))]
        public void WithInvalidRegisterImplementation_ThrowsDependencyContainerResolutionException(
            Type registerImplementation)
        {
            var container = new DependencyContainer();
            Assert.Throws<DependencyContainerRegistrationException>(() =>
                container.Register(
                    typeof(Dictionary<string, string>), registerImplementation).AsMultiInstance()
            );
        }
        
        [Test]
        public void WithTypeAsMultiInstance_ThrowsDependencyContainerResolutionException()
        {
            var container = new DependencyContainer();

            container.Register(typeof(string)).AsMultiInstance();

            Assert.Throws<DependencyContainerResolutionException>(() =>
                container.Resolve<IAnimal>()
            );
        }

        [Test]
        public void WithRegisterImplementationAsSingleton_ThrowsDependencyContainerRegistrationException()
        {
            var container = new DependencyContainer();

            Assert.Throws<DependencyContainerRegistrationException>(() =>
                container.Register<IAnimal, Shark>(new Shark()).AsSingleton()
            );
        }

        [Test]
        public void WithRegisterImplementationAsMultiInstance_ThrowsDependencyContainerResolutionException()
        {
            var container = new DependencyContainer();

            Assert.Throws<DependencyContainerRegistrationException>(() =>
                container.Register( (di, param) => new Human(param["Name"].ToString()) ).AsMultiInstance()
            );
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

        [Test]
        public void RegisterInterfaceWithInstance_ThrowsDependencyContainerRegistrationException()
        {
            var container = new DependencyContainer();
            var instance = new Human("George");

            Assert.Throws<DependencyContainerRegistrationException>(() =>
                container.Register(typeof(IDictionary<string, string>).GetGenericTypeDefinition(), typeof(string), instance)
            );
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
    public class CanResolve : DependencyContainerTest
    {
        [Test]
        public void WithIEnumerable_RegisterIEnumerable()
        {
            var container = new DependencyContainer();

            Assert.IsTrue(container.CanResolve<IEnumerable<string>>());
            container.Register<IEnumerable<string>, StringEnumerable>();

            Assert.IsTrue(container.CanResolve<IEnumerable<string>>());
            Assert.AreEqual(typeof(StringEnumerable), container.Resolve<IEnumerable<string>>().GetType());
        }

        [Test]
        public void WithDictionary_RegisterDictionary()
        {
            var container = new DependencyContainer();

            Assert.IsTrue(container.CanResolve<Dictionary<string,object>>(NameDictionary));
        }

        [Test]
        public void WithString_RegisterDictionary()
        {
            var container = new DependencyContainer();

            Assert.IsFalse(container.CanResolve<string>((new Shark()).Name));
        }

        [TestCase(typeof(Func<>))]
        [TestCase(typeof(Func<string, int>))]
        [TestCase(typeof(Func<string, IDictionary<string, object>, int>))]
        public void WithType_RegisterDictionary(Type resolveType)
        {
            var container = new DependencyContainer();

            Assert.IsTrue(container.CanResolve(resolveType, NameDictionary));
        }
        
    }

    [TestFixture]
    public class ResolveAll
    {
        [Test]
        public void WithTypeAsParam_ResolveAll()
        {
            var container = new DependencyContainer();
            container.Register(typeof(Shark));
            Assert.IsNotNull(container.ResolveAll(typeof(Shark)));
        }

        [Test]
        public void WithType_ResolveAll()
        {
            var container = new DependencyContainer();
            container.Register(typeof(Shark));
            Assert.IsNotNull(container.ResolveAll<Shark>());
        }
        
        [TestCase(typeof(Shark), false)]
        [TestCase(typeof(Shark), true)]
        public void WithTypeAsParamAndWithParent_ResolveAll(Type resolveType, bool includeUnnamed)
        {
            var containerParent = new DependencyContainer();
            containerParent.Register(typeof(Shark));

            var container = containerParent.GetChildContainer();
            
            Assert.IsNotNull(container.ResolveAll(resolveType, includeUnnamed));
        }
    }

}