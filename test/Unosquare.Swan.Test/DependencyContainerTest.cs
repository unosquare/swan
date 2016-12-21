using NUnit.Framework;
using Unosquare.Swan.Runtime;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class DependencyContainerTest
    {
        [Test]
        public void GetDependencyContainerTest()
        {
            Assert.IsNotNull(CurrentApp.Container);
        }

        [Test]
        public void RegisterInterfaceTest()
        {
            if (CurrentApp.Container.CanResolve<IAnimal>() == false)
                CurrentApp.Container.Register<IAnimal, Fish>();

            Assert.IsNotNull(CurrentApp.Container.Resolve<IAnimal>());
            Assert.AreEqual((new Fish()).Name, CurrentApp.Container.Resolve<IAnimal>().Name);

            CurrentApp.Container.Unregister<IAnimal>();
            Assert.Throws<DependencyContainerResolutionException>(() => CurrentApp.Container.Resolve<IAnimal>());
        }

        // Autoregister is not working when you run NUNit at NETCORE, because the deps are not loaded. Probably an issue.
#if NET452
        [Test]
        public void AutoregisterTest()
        {
            CurrentApp.Container.AutoRegister();
            Assert.IsTrue(CurrentApp.Container.CanResolve<ICar>());
            Assert.AreEqual((new TheOnlyCar()).Name, CurrentApp.Container.Resolve<ICar>().Name);
        }
#endif

        [Test]
        public void BuildUpTest()
        {
            CurrentApp.Container.Register<IAnimal, Fish>();
            CurrentApp.Container.Register<ICar, TheOnlyCar>();

            var instance = new Controller();

            CurrentApp.Container.BuildUp(instance);

            Assert.AreEqual((new Fish()).Name, instance.Animal.Name);
            Assert.AreEqual((new TheOnlyCar()).Name, instance.Car.Name);
        }
        
        [Test]
        public void ThrowResolutionExceptionTest()
        {
            if (CurrentApp.Container.CanResolve<IAnimal>())
                CurrentApp.Container.Unregister<IAnimal>();

            Assert.Throws<DependencyContainerResolutionException>(() => CurrentApp.Container.Resolve<IAnimal>());
        }
    }
}