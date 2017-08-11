using NUnit.Framework;
using System;
using Unosquare.Swan.Networking;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class ExtensionsTest
    {
        [Test]
        public void CopyPropertiesToTest()
        {
            var source = BasicJson.GetDefault();
            var destination = new BasicJson();

            source.CopyPropertiesTo(destination);

            Assert.AreEqual(source.BoolData, destination.BoolData);
            Assert.AreEqual(source.DecimalData, destination.DecimalData);
            Assert.AreEqual(source.StringData, destination.StringData);
            Assert.AreEqual(source.StringNull, destination.StringNull);
        }

        [Test]
        public void IgnoredPropertiesTest()
        {
            var source = BasicJson.GetDefault();
            var destination = new BasicJson();
            string[] ignored = { "NegativeInt", "BoolData" };
            source.CopyPropertiesTo(destination, ignored);

            Assert.AreNotEqual(source.BoolData, destination.BoolData);
            Assert.AreNotEqual(source.NegativeInt, destination.NegativeInt);
            Assert.AreEqual(source.StringData, destination.StringData);
        }

        [Test]
        public void OnlyPropertiesTest()
        {
            var source = BasicJson.GetDefault();
            var destination = new BasicJson();
            string[] Only = { "NegativeInt", "BoolData" };
            source.CopyOnlyPropertiesTo(destination, Only);

            Assert.AreEqual(source.BoolData, destination.BoolData);
            Assert.AreEqual(source.NegativeInt, destination.NegativeInt);
            Assert.AreNotEqual(source.StringData, destination.StringData);
        }

        [Test]
        public void CopyPropertiesToNewTest()
        {
            var source = BasicJson.GetDefault();
            var destination = source.CopyPropertiesToNew<BasicJson>();
            
            Assert.IsNotNull(destination);
            Assert.AreSame(source.GetType(), destination.GetType());

            Assert.AreEqual(source.BoolData, destination.BoolData);
            Assert.AreEqual(source.DecimalData, destination.DecimalData);
            Assert.AreEqual(source.StringData, destination.StringData);
            Assert.AreEqual(source.StringNull, destination.StringNull);
        }

        [Test]
        public void CopyOnlyPropertiesToNewTest()
        {
            var source = BasicJson.GetDefault();
            string[] Only = { "BoolData", "DecimalData" };
            var destination = source.CopyOnlyPropertiesToNew<BasicJson>(Only);

            Assert.IsNotNull(destination);
            Assert.AreSame(source.GetType(), destination.GetType());

            Assert.AreEqual(source.BoolData, destination.BoolData);
            Assert.AreEqual(source.DecimalData, destination.DecimalData);
        }

        [Test]
        public void ActionRetryTest()
        {
            Assert.Throws<AggregateException>(() =>
            {
                var action =
                    new Action(() => JsonClient.GetString("http://accesscore.azurewebsites.net/api/token").Wait());

                action.Retry();
            });
        }
        
        [Test]
        public void FuncRetryTest()
        {
            var total = 0;

            var action = new Func<int>(() =>
            {
                if (total++ < 2)
                    throw new Exception();

                return total;
            });

            var result = action.Retry();
            Assert.AreEqual(3, result);
        }
    }
}
