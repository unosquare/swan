using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unosquare.Swan.Networking;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class Retry
    {
        [Test]
        public void WithNewFunction_RetryAction()
        {
            var total = 0;

            var action = new Func<int>(() =>
            {
                if(total++ < 2)
                    throw new Exception();

                return total;
            });
            
            var result = action.Retry();
            Assert.AreEqual(3, result);
        }

        [Test]
        public void WithNewAction_ThrowsAggregateException()
        {
            Assert.Throws<AggregateException>(() =>
            {
                var action =
                    new Action(() => JsonClient.GetString("http://accesscore.azurewebsites.net/api/token").Wait());

                action.Retry();
            });
        }
    }

    [TestFixture]
    public class CopyPropertiesTo
    {
        [Test]
        public void WithValidObjectAttr_CopyPropertiesToTarget()
        {
            var source = ObjectAttr.Get();
            var target = new ObjectAttr();

            source.CopyPropertiesTo(target);

            Assert.AreEqual(source.Name, target.Name);
            Assert.AreEqual(source.IsActive, target.IsActive);
        }

        [Test]
        public void WithValidDictionary_CopyPropertiesToTarget()
        {
            IDictionary<string, object> source = new Dictionary<string, object>
            {
                { "Name", "Thrall" },
                { "Email", "Warchief.Thrall@horde.com" },
                { "Role", "Warchief" }
            };
            
            var target = new UserDto();
            
            source.CopyPropertiesTo(target, null);
            
            Assert.AreEqual(source["Name"].ToString(), target.Name);
            Assert.AreEqual(source["Email"].ToString(), target.Email);
        }

        [Test]
        public void WithValidBasicJson_CopyPropertiesToTarget()
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
        public void WithValidParamsAndNewProperty_CopyPropertiesToTarget()
        {
            var source = BasicJson.GetDefault();
            source.StringNull = "1";

            var destination = new BasicJsonWithNewProperty();

            source.CopyPropertiesTo(destination);

            Assert.AreEqual(source.BoolData, destination.BoolData);
            Assert.AreEqual(source.DecimalData, destination.DecimalData);
            Assert.AreEqual(source.StringData, destination.StringData);
            Assert.AreEqual(source.StringNull, destination.StringNull.ToString());
        }

        [Test]
        public void WithValidBasicJson_CopyNotIgnoredPropertiesToTarget()
        {
            var source = BasicJson.GetDefault();
            var destination = new BasicJson();

            source.CopyPropertiesTo(destination, new[] { nameof(BasicJson.NegativeInt), nameof(BasicJson.BoolData) });

            Assert.AreNotEqual(source.BoolData, destination.BoolData);
            Assert.AreNotEqual(source.NegativeInt, destination.NegativeInt);
            Assert.AreEqual(source.StringData, destination.StringData);
        }
    }

    [TestFixture]
    public class CopyOnlyPropertiesTo
    {
        [Test]
        public void WithValidBasicJson_CopyOnlyPropertiesToTarget()
        {
            var source = BasicJson.GetDefault();
            var destination = new BasicJson { NegativeInt = 800, BoolData = false };
            source.CopyOnlyPropertiesTo(destination, new[] { nameof(BasicJson.NegativeInt), nameof(BasicJson.BoolData) });

            Assert.AreEqual(source.BoolData, destination.BoolData);
            Assert.AreEqual(source.NegativeInt, destination.NegativeInt);
            Assert.AreNotEqual(source.StringData, destination.StringData);
        }

        [Test]
        public void WithValidObjectAttr_CopyOnlyPropertiesToTarget()
        {
            var source = ObjectAttr.Get();
            var target = new ObjectAttr();

            source.CopyOnlyPropertiesTo(target);

            Assert.AreEqual(source.Name, target.Name);
            Assert.AreEqual(source.IsActive, target.IsActive);
        }
    }

    [TestFixture]
    public class CopyPropertiesToNew
    {
        [Test]
        public void WithValidParams_CopyPropertiesToNewObject()
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
        public void WithValidBasicJson_CopyPropertiesToNewBasicJson()
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
    }

    [TestFixture]
    public class CopyOnlyPropertiesToNew
    {
        [Test]
        public void WithValidParams_CopyOnlyPropertiesToNewObject()
        {
            var source = ObjectAttr.Get();
            var target = source.CopyOnlyPropertiesToNew<ObjectAttr>(new[] { nameof(ObjectAttr.Name) });
            Assert.AreEqual(source.Name, target.Name);
        }

        [Test]
        public void WithValidBasicJson_CopyOnlyPropertiesToNewBasicJson()
        {
            var source = BasicJson.GetDefault();
            var destination = source.CopyOnlyPropertiesToNew<BasicJson>(new[]
                {nameof(BasicJson.BoolData), nameof(BasicJson.DecimalData)});

            Assert.IsNotNull(destination);
            Assert.AreSame(source.GetType(), destination.GetType());

            Assert.AreEqual(source.BoolData, destination.BoolData);
            Assert.AreEqual(source.DecimalData, destination.DecimalData);
        }
    }

}