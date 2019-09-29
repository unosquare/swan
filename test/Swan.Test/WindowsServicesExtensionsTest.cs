using Moq;
using Moq.Protected;
using NUnit.Framework;
using System;
using AutoFixture;
using System.Linq;
using AutoFixture.AutoMoq;
using System.Collections.Generic;

namespace Swan.Test
{
    [TestFixture]
    public class WindowsServicesExtensionsTest
    {
        [Test]
        public void RunInConsoleMode_NullService()
        {
            Services.ServiceBase nullObject = null;

            Assert.Throws<ArgumentNullException>(() =>
                nullObject.RunInConsoleMode());
        }

        [Test]
        public void RunInConsoleMode()
        {
            var service = new Mock<Services.ServiceBase>();
            service.Object.RunInConsoleMode();
            Assert.That(true);
        }

        [Test]
        public void RunInConsoleMode_NullServicesArray()
        {
            Services.ServiceBase[] nulls= null;

            Assert.Throws<ArgumentNullException>(() =>
                nulls.RunInConsoleMode());
        }

        [Test]
        public void RunInConsoleModeArray()
        {
            var fixture = new Fixture();
            fixture.Customize(new AutoMoqCustomization());
            var services = fixture.CreateMany<Services.ServiceBase>();

            services.ToArray().RunInConsoleMode();
            Assert.That(true);
        }

        [Test]
        public void RunInConsoleModeOnStop()
        {
            var service = new Mock<Services.ServiceBase>();

            service.Object.RunInConsoleMode();
            service.Protected().Verify("OnStop", Times.Once());
        }

        [Test]
        public void RunInConsoleModeOnStart()
        {
            var service = new Mock<Services.ServiceBase>();

            service.Object.RunInConsoleMode();
            service.Protected().Verify(
                    "OnStart",
                    Times.Once(), 
                    new object[] { new string[] { },});
        }

        [Test]
        public void RunInConsoleModeOnManyStop()
        {
            var size = new Random().Next(25);
            var mocks = new List<Mock<Services.ServiceBase>>();
            var services = new List<Services.ServiceBase>();

            for (int i = 0; i < size; i++)
            {
                var fixture = new Fixture();
                fixture.Customize(new AutoMoqCustomization());
                mocks.Add(fixture.Freeze<Mock<Services.ServiceBase>>());
                services.Add(fixture.Create<Services.ServiceBase>());
            }

            services.ToArray().RunInConsoleMode();
            mocks.ForEach(x => x.Protected().Verify("OnStop", Times.Once()));
        }

        [Test]
        public void RunInConsoleModeOnManyStart()
        {
            var size = new Random().Next(25);
            var mocks = new List<Mock<Services.ServiceBase>>();
            var services = new List<Services.ServiceBase>();

            for (int i = 0; i < size; i++)
            {
                var fixture = new Fixture();
                fixture.Customize(new AutoMoqCustomization());
                mocks.Add(fixture.Freeze<Mock<Services.ServiceBase>>());
                services.Add(fixture.Create<Services.ServiceBase>());
            }

            services.ToArray().RunInConsoleMode();
            mocks.ForEach(x => x.Protected().Verify(
                "OnStart",
                Times.Once(),
                new object[] { new string[] { }, }));
        }
    }
}
