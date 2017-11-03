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
        protected string Name = "BlizzCon!";

        protected Dictionary<string, object> Dictionary = new Dictionary<string, object> {
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
        public void WithDictionary_ResolvesTheType()
        {
            var container = new DependencyContainer();
            
            container.Resolve<Dictionary<string, object>>(Dictionary);
            
            Assert.AreEqual(container.ToString(),"Unosquare.Swan.Components.DependencyContainer");
        }
        
        [Test]
        public void WithStringAndInvalidType_ThrowsDependencyContainerResolutionException()
        {
            var container = new DependencyContainer();
            
            Assert.Throws<DependencyContainerResolutionException>(() =>
                container.Resolve<Dictionary<string, object>>(Name)
            );
        }

        [Test]
        public void WithStringAndInvalidTypeAsParam_ThrowsDependencyContainerResolutionException()
        {
            var container = new DependencyContainer();
            
            Assert.Throws<DependencyContainerResolutionException>(() =>
                container.Resolve(typeof(string), Name)
            );
        }

        [Test]
        public void WithDictionaryAndString_ThrowsDependencyContainerResolutionException()
        {
            var container = new DependencyContainer();
            
            Assert.Throws<DependencyContainerResolutionException>(() =>
                container.Resolve<string>(Name, Dictionary)
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
        [Test]
        public void WithObject_FailResolveType()
        {
            var container = new DependencyContainer();
            
            Assert.IsFalse(container.TryResolve(typeof(string) , out object obj));
        }

        [TestCase(typeof(Dictionary<String, Object>), true)]
        [TestCase(typeof(string), false)]
        public void WithObjectAndResolveOptions_ResolveType(Type resolveType, bool expected)
        {
            var container = new DependencyContainer();
            
            Assert.AreEqual(expected, container.TryResolve(
                resolveType, DependencyContainerResolveOptions.Default, out object obj));
        }

        [Test]
        public void WithObjectAndString_FailResolveType()
        {
            var container = new DependencyContainer();
            
            Assert.IsFalse(container.TryResolve(
                typeof(string), Name, out object obj));
        }

        [Test]
        public void WithObjectAndStringAndResolveOptions_FailResolveType()
        {
            var container = new DependencyContainer();
            
            Assert.IsFalse(container.TryResolve(
                typeof(string), Name, DependencyContainerResolveOptions.Default, out object obj));
        }

        [TestCase(typeof(Dictionary<String, Object>), true)]
        [TestCase(typeof(string), false)]
        public void WithObjectAndDictionary_ResolveType(Type resolveType, bool expected)
        {
            var container = new DependencyContainer();

            Assert.AreEqual(expected, container.TryResolve(
                resolveType, Dictionary, out object obj));
        }

        [Test]
        public void WithObjectAndStringAndDictionary_FailResolveType()
        {
            var container = new DependencyContainer();

            Assert.IsFalse(container.TryResolve(
                typeof(string), Name, Dictionary, out object obj));
        }

        [TestCase(typeof(Dictionary<String, Object>), true)]
        [TestCase(typeof(string), false)]
        public void WithObjectAndDictionaryAndResolveOptions_ResolveType(Type resolveType, bool expected)
        {
            var container = new DependencyContainer();

            Assert.AreEqual(expected, container.TryResolve(
                resolveType, Dictionary, DependencyContainerResolveOptions.Default, out object obj));
        }


        [TestCase(typeof(Dictionary<String, Object>), false)]
        [TestCase(typeof(string), false)]
        public void WithStringAndDictionaryAndResolveOptions_FailResolveType(Type resolveType, bool expected)
        {
            var container = new DependencyContainer();

            Assert.AreEqual(expected, container.TryResolve(
                resolveType, Name, Dictionary, DependencyContainerResolveOptions.Default, out object obj));
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

        [Test]
        public void WithRegisterAndString_FailResolveType()
        {
            var container = new DependencyContainer();
            container.Register<IAnimal, Fish>();

            Assert.IsFalse(container.TryResolve(
                Name, out IAnimal instance));
        }

        [Test]
        public void WithoutRegisterAndWithString_FailResolveType()
        {
            var container = new DependencyContainer();

            Assert.IsFalse(container.TryResolve(
                Name, out IAnimal instance));
        }

        [Test]
        public void WithStringAndResolveOptions_FailResolveType()
        {
            var container = new DependencyContainer();

            Assert.IsFalse(container.TryResolve(
                Name, DependencyContainerResolveOptions.Default, out IAnimal instance));
        }

        [Test]
        public void WithRegisterAndDictionary_FailResolveType()
        {
            var container = new DependencyContainer();
            container.Register<IDictionary<string, object>, Dictionary<string, object>>();

            Assert.IsFalse(container.TryResolve(
                Dictionary, out IDictionary<string,object> instance));
        }

        [Test]
        public void WithoutRegisterAndWithDictionary_FailResolveType()
        {
            var container = new DependencyContainer();

            Assert.IsFalse(container.TryResolve(
                Dictionary, out IDictionary<string, object> instance));
        }

        [Test]
        public void WithStringAndWithDictionary_FailResolveType()
        {
            var container = new DependencyContainer();

            Assert.IsFalse(container.TryResolve(
                Name, Dictionary, out IDictionary<string, object> instance));
        }

        [Test]
        public void WithDictionaryAndResolveOptions_FailResolveType()
        {
            var container = new DependencyContainer();

            Assert.IsFalse(container.TryResolve(
                Dictionary, DependencyContainerResolveOptions.Default, 
                out IDictionary<string, object> instance));
        }

        [Test]
        public void WithStringAndDictionaryAndResolveOptions_FailResolveType()
        {
            var container = new DependencyContainer();

            Assert.IsFalse(container.TryResolve(
                Name, Dictionary, DependencyContainerResolveOptions.Default, 
                out IDictionary<string, object> instance));
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
        public void WithConstructor_ReturnsOptions()
        {
            var container = new DependencyContainer();

            container.Register(typeof(IAnimal), (di, param) => new Human(param["Name"].ToString()));
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
                // TODO: mmmmm
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

        [Test]
        public void WithTypeAsSingleton_ThrowsDependencyContainerResolutionException()
        {
            var container = new DependencyContainer();

            container.Register(typeof(string)).AsSingleton();
            
            Assert.Throws<DependencyContainerResolutionException>(() =>
                container.Resolve<IAnimal>()
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
        public void WithNullFactory_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var container = new DependencyContainer();

                container.Register((Func<DependencyContainer, Dictionary<string, object>, IAnimal>)null);
            });
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

            Assert.IsTrue(container.CanResolve<Dictionary<string,object>>(Dictionary));
        }

        [Test]
        public void WithString_RegisterDictionary()
        {
            var container = new DependencyContainer();

            Assert.IsFalse(container.CanResolve<string>(Name));
        }
    }

    [TestFixture]
    public class ResolveAll
    {
        [Test]
        public void WithType_ResolveAll()
        {
            var container = new DependencyContainer();

            var resolved = container.ResolveAll<Dictionary<string, object>>();
            
            Assert.AreEqual(
                "System.Linq.Enumerable+<CastIterator>d__94`1[System.Collections" +
                ".Generic.Dictionary`2[System.String,System.Object]]", 
                resolved.ToString());
        }

        [Test]
        public void WithTypeAsParam_ResolveAll()
        {
            var container = new DependencyContainer();

            var resolved = container.ResolveAll(typeof(Dictionary<string, object>));

            Assert.AreEqual(
                "System.Linq.Enumerable+WhereSelectEnumerableIterator`2" +
                "[Unosquare.Swan.Components.DependencyContainer+TypeRegistration,System.Object]",
                resolved.ToString());
        }
    }

}