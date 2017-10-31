using System;
using NUnit.Framework;
using System.Collections.Generic;
using Unosquare.Swan.Components;
using Unosquare.Swan.Exceptions;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class DependencyContainerTest
    {
        [Test]
        public void GetDependencyContainerTest()
        {
            Assert.IsNotNull(Runtime.Container);
        }

        [Test]
        public void RegisterInterfaceTest()
        {
            var container = new DependencyContainer();

            container.Register<IAnimal, Fish>();

            Assert.IsNotNull(container.Resolve<IAnimal>());
            Assert.AreEqual((new Fish()).Name, container.Resolve<IAnimal>().Name);

            container.Unregister<IAnimal>();
            Assert.Throws<DependencyContainerResolutionException>(() => container.Resolve<IAnimal>());
        }

#if !NETSTANDARD1_3 && !UWP
        [Test]
        public void AutoregisterTest_ThrowResolutionException()
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
        public void AutoregisterTest_ThrowAutoRegistrationException()
        {
            Assert.Throws<DependencyContainerRegistrationException>(() =>
            {
                var container = new DependencyContainer();
                container.AutoRegister(DependencyContainerDuplicateImplementationActions.Fail);
            });
        }
#endif

        [Test]
        public void Autoregister_ResolvesIAnimal()
        {
            var container = new DependencyContainer();
            container.AutoRegister(Runtime.GetAssemblies());
            Assert.IsTrue(container.CanResolve<ICar>());
        }


        [Test]
        public void BuildUpTest()
        {
            var container = new DependencyContainer();

            container.Register<IAnimal, Fish>();
            container.Register<ICar, TheOnlyCar>();

            var instance = new Controller();

            container.BuildUp(instance);

            Assert.AreEqual((new Fish()).Name, instance.Animal.Name);
            Assert.AreEqual((new TheOnlyCar()).Name, instance.Car.Name);
        }

        [Test]
        public void ThrowResolutionExceptionTest()
        {
            if (Runtime.Container.CanResolve<IAnimal>())
                Runtime.Container.Unregister<IAnimal>();

            Assert.Throws<DependencyContainerResolutionException>(() => Runtime.Container.Resolve<IAnimal>());
        }

        [Test]
        public void RegisterClass_ReturnsOptions()
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
        public void RegisterInterfaceWithInstanceWeakReference_CanDestroy()
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
        public void RegisterInterfaceWithInstanceStrongReference_CanDestroy()
        {
            var container = new DependencyContainer();
            using (var instance = new Human("George"))
            {
                // TODO: mmmmm
                container.Register<IAnimal>(instance).WithStrongReference();
            }

            var containerInstance = (Human) container.Resolve<IAnimal>();
            Assert.IsTrue(containerInstance.IsDisposed);
        }


        [Test]
        public void RegisterDisposable_IsDispose()
        {
            var container = new DependencyContainer();
            var instance = new Human("George");
            container.Register<IAnimal>(instance);
            container.Dispose();
            Assert.IsTrue(instance.IsDisposed);
        }

        [Test]
        public void RegisterMultipleTypes_ReturnsOptionse()
        {
            var container = new DependencyContainer();
            Assert.IsNotNull(container.RegisterMultiple<IAnimal>(new[] {typeof(Monkey), typeof(Fish)}));
        }

        [Test]
        public void RegisterMultipleTypesNullImplementations_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var container = new DependencyContainer();
                Assert.IsNotNull(container.RegisterMultiple(typeof(IAnimal), null));
            });
        }

        [Test]
        public void RegisterMultipleTypesInvalidImplementations_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var container = new DependencyContainer();
                Assert.IsNotNull(container.RegisterMultiple<IAnimal>(new[] { typeof(TheOnlyCar), typeof(Fish) }));
            });
        }

        [Test]
        public void RegisterMultipleTypesDuplicatedImplementations_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var container = new DependencyContainer();
                Assert.IsNotNull(container.RegisterMultiple<IAnimal>(new[] { typeof(Monkey), typeof(Monkey) }));
            });
        }

        [Test]
        public void TryResolve_CanResolve()
        {
            var container = new DependencyContainer();
            container.Register<IAnimal, Fish>();

            Assert.IsTrue(container.TryResolve(out IAnimal instance));
            Assert.AreEqual((new Fish()).Name, instance.Name);
        }

        [Test]
        public void TryResolve_Fail()
        {
            var container = new DependencyContainer();

            Assert.IsFalse(container.TryResolve(out IAnimal instance));
        }

        [Test]
        public void RegisterIEnumerableTest()
        {
            var container = new DependencyContainer();

            Assert.IsTrue(container.CanResolve<IEnumerable<string>>());
            container.Register<IEnumerable<string>, StringEnumerable>();

            Assert.IsTrue(container.CanResolve<IEnumerable<string>>());
            Assert.AreEqual(typeof(StringEnumerable), container.Resolve<IEnumerable<string>>().GetType());
        }

        [Test]
        public void RegisterWithConstructor_ReturnsOptions()
        {
            var container = new DependencyContainer();

            container.Register(typeof(IAnimal), (di, param) => new Human(param["Name"].ToString()));
        }


        [Test]
        public void RegisterWithConstructor_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var container = new DependencyContainer();

                container.Register((Func<DependencyContainer, Dictionary<string, object>, IAnimal>) null);
            });
        }
    }
}