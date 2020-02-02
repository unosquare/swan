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
        public void RunInConsoleMode_NullServicesArray()
        {
            Services.ServiceBase[] nulls= null;

            Assert.Throws<ArgumentNullException>(() =>
                nulls.RunInConsoleMode());
        }
    }
}
