﻿using Moq;
using Moq.Protected;
using NUnit.Framework;
using System;
using Swan.Test.Entropy;

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
            var (services, mocks) =
                GenerateArray.GetMockList<Services.ServiceBase>();

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
            var (objects, mocks) = 
                GenerateArray.GetMockList<Services.ServiceBase>();

            objects.ToArray().RunInConsoleMode();
            mocks.ForEach(x => x.Protected().Verify("OnStop", Times.Once()));
        }

        [Test]
        public void RunInConsoleModeOnManyStart()
        {
            var (objects, mocks) =
                GenerateArray.GetMockList<Services.ServiceBase>();

            objects.ToArray().RunInConsoleMode();
            mocks.ForEach(x => x.Protected().Verify(
                "OnStart",
                Times.Once(),
                new object[] { new string[] { }, }));
        }
    }
}
