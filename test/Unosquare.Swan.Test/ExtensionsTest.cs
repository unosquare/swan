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
            source.CopyPropertiesTo(destination, new[] { nameof(BasicJson.NegativeInt), nameof(BasicJson.BoolData)});

            Assert.AreNotEqual(source.BoolData, destination.BoolData);
            Assert.AreNotEqual(source.NegativeInt, destination.NegativeInt);
            Assert.AreEqual(source.StringData, destination.StringData);
        }

        [Test]
        public void OnlyPropertiesTest()
        {
            var source = BasicJson.GetDefault();
            var destination = new BasicJson {NegativeInt = 800, BoolData = false};
            source.CopyOnlyPropertiesTo(destination, new[] { nameof(BasicJson.NegativeInt), nameof(BasicJson.BoolData) });

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
            var destination = source.CopyOnlyPropertiesToNew<BasicJson>(new[]
                {nameof(BasicJson.BoolData), nameof(BasicJson.DecimalData)});

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

        [Test]
        public void CopyEnum()
        {
            var source = new ObjectEnum
            {
                Id = 1,
                MyEnum = MyEnum.Two
            };

            var result = source.CopyPropertiesToNew<ObjectEnum>();
            Assert.AreEqual(source.MyEnum, result.MyEnum);
        }

        [Test]
        public void CopyWithAttr()
        {
            var source = ObjectAttr.Get();
            var target = new ObjectAttr();
            source.CopyPropertiesTo(target);
            Assert.AreEqual(source.Name, target.Name);
            Assert.AreEqual(source.IsActive, target.IsActive);
        }

        [Test]
        public void CopyWithAttributeToNew()
        {
            var source = ObjectAttr.Get();
            var target = source.CopyOnlyPropertiesToNew<ObjectAttr>(new[] {nameof(ObjectAttr.Name)});
            Assert.AreEqual(source.Name, target.Name);
        }
    }
}