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
        public void TryResolveTest()
        {
            var container = new DependencyContainer();

            container.Register<IAnimal, Fish>();

            Assert.IsTrue(container.TryResolve(out IAnimal instance));
            Assert.AreEqual((new Fish()).Name, instance.Name);
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
    }
}